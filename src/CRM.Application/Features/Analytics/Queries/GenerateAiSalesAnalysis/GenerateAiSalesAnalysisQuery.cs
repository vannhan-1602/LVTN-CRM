using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Application.Features.Analytics.DTOs;
using CRM.Application.Interfaces.Analytics;
using CRM.Domain.Interfaces.Services;
using MediatR;

namespace CRM.Application.Features.Analytics.Queries.GenerateAiSalesAnalysis;

public record GenerateAiSalesAnalysisQuery(int SoThang = 6) : IRequest<AiSalesAnalysisResultDto>;

public class GenerateAiSalesAnalysisQueryHandler
    : IRequestHandler<GenerateAiSalesAnalysisQuery, AiSalesAnalysisResultDto>
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IOpenAiService _openAiService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GenerateAiSalesAnalysisQueryHandler(
        IAnalyticsRepository analyticsRepository, IOpenAiService openAiService)
    {
        _analyticsRepository = analyticsRepository;
        _openAiService = openAiService;
    }

    public async Task<AiSalesAnalysisResultDto> Handle(GenerateAiSalesAnalysisQuery request, CancellationToken ct)
    {
        var soThang = request.SoThang is < 1 or > 24 ? 6 : request.SoThang;
        var data = await _analyticsRepository.GetSalesAnalyticsDataAsync(soThang, ct);

        var prompt = BuildPrompt(data);
        string nhanDinh;
        List<AiDeXuatDto> deXuat;

        try
        {
            var raw = await _openAiService.GetChatCompletionAsync(prompt, ct);
            var parsed = ParseAiJson(raw);
            nhanDinh = parsed?.NhanDinhTongQuan ?? "AI trả lời không đúng định dạng mong đợi.";
            deXuat = parsed?.DeXuat ?? new List<AiDeXuatDto>
            {
                new()
                {
                    TieuDe = "Không đọc được phản hồi có cấu trúc từ AI",
                    MoTa = raw.Length > 500 ? raw[..500] + "..." : raw,
                    MucDoUuTien = "TrungBinh",
                    NhomVanDe = "Khac"
                }
            };
        }
        catch (Exception ex)
        {
            // Không để lỗi từ AI (hết quota, sai API key, mất mạng...) làm sập cả API —
            // vẫn trả về số liệu thô để frontend vẽ biểu đồ, chỉ phần đề xuất báo lỗi rõ ràng.
            nhanDinh = "Không thể tạo phân tích AI lúc này.";
            deXuat = new List<AiDeXuatDto>
            {
                new()
                {
                    TieuDe = "Lỗi khi gọi AI",
                    MoTa = $"{ex.Message}. Kiểm tra cấu hình OpenAI:ApiKey / OpenAI:BaseUrl, " +
                           "hoặc thử lại sau. Số liệu thống kê bên dưới vẫn chính xác và dùng được ngay.",
                    MucDoUuTien = "Cao",
                    NhomVanDe = "Khac"
                }
            };
        }

        return new AiSalesAnalysisResultDto
        {
            GeneratedAt = DateTime.UtcNow,
            DuLieu = data,
            NhanDinhTongQuan = nhanDinh,
            DeXuat = deXuat
        };
    }

    /// <summary>Model đôi khi bọc JSON trong ```json ... ``` — bóc tách trước khi parse.</summary>
    private static AiJsonResponse? ParseAiJson(string raw)
    {
        var text = raw.Trim();
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline >= 0) text = text[(firstNewline + 1)..];
            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0) text = text[..lastFence];
        }

        try
        {
            return JsonSerializer.Deserialize<AiJsonResponse>(text.Trim(), JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private class AiJsonResponse
    {
        [JsonPropertyName("nhanDinhTongQuan")]
        public string NhanDinhTongQuan { get; set; } = string.Empty;

        [JsonPropertyName("deXuat")]
        public List<AiDeXuatDto> DeXuat { get; set; } = new();
    }

    private static string BuildPrompt(SalesAnalyticsDataDto d)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là chuyên gia tư vấn kinh doanh (business consultant) cho một công ty " +
                       "giải pháp IT (TechSol). Dựa trên số liệu CRM dưới đây, hãy phân tích và đưa ra " +
                       "đề xuất hành động cụ thể cho Manager, giống phong cách 1 agency tư vấn báo cáo cho khách hàng.");
        sb.AppendLine();
        sb.AppendLine("CHỈ trả về JSON hợp lệ theo đúng schema sau, KHÔNG thêm bất kỳ văn bản, giải thích, " +
                       "hay markdown nào khác ngoài JSON:");
        sb.AppendLine("""
        {
          "nhanDinhTongQuan": "1-2 câu tổng quan tình hình kinh doanh",
          "deXuat": [
            {
              "tieuDe": "Tiêu đề ngắn gọn của đề xuất",
              "moTa": "Mô tả chi tiết 1-2 câu, giải thích vì sao và nên làm gì",
              "mucDoUuTien": "Cao | TrungBinh | Thap",
              "nhomVanDe": "DoanhThu | CoHoi | CongNo | Ticket | SanPham"
            }
          ]
        }
        """);
        sb.AppendLine("Đưa ra 4-6 đề xuất, bao quát các nhóm vấn đề khác nhau (không chỉ tập trung 1 nhóm). " +
                       "Ưu tiên đề xuất cụ thể, có thể hành động ngay (actionable), tránh chung chung.");
        sb.AppendLine();
        sb.AppendLine($"=== SỐ LIỆU {d.SoThangPhanTich} THÁNG GẦN NHẤT ===");

        sb.AppendLine("Doanh thu theo tháng:");
        if (d.DoanhThuTheoThang.Count == 0)
        {
            sb.AppendLine("- (không có hóa đơn nào trong giai đoạn này)");
        }
        foreach (var m in d.DoanhThuTheoThang)
        {
            var tenThang = new DateTime(m.Nam, m.Thang, 1).ToString("MM/yyyy", CultureInfo.InvariantCulture);
            sb.AppendLine($"- {tenThang}: {m.DoanhThu:N0} VNĐ ({m.SoHoaDon} hóa đơn)");
        }

        sb.AppendLine();
        sb.AppendLine($"Cơ hội bán hàng: tổng {d.TongSoCoHoi}, thành công {d.SoCoHoiThanhCong}, " +
                       $"thất bại {d.SoCoHoiThatBai}, tỉ lệ thắng {d.TyLeThangCoHoi}%.");

        sb.AppendLine();
        sb.AppendLine("Top sản phẩm bán chạy:");
        if (d.Top5SanPhamBanChay.Count == 0)
        {
            sb.AppendLine("- (không có giao dịch xuất bán nào trong giai đoạn này)");
        }
        foreach (var sp in d.Top5SanPhamBanChay)
        {
            sb.AppendLine($"- {sp.TenSanPham}: {sp.SoLuongBan} đơn vị");
        }

        sb.AppendLine();
        sb.AppendLine($"Ticket hỗ trợ: tổng {d.TongSoTicket}, đang mở {d.SoTicketDangMo}, " +
                       $"khẩn cấp chưa xử lý {d.SoTicketKhanCap}.");
        sb.AppendLine($"Công nợ chưa thu: {d.TongCongNoChuaThu:N0} VNĐ.");

        return sb.ToString();
    }
}
