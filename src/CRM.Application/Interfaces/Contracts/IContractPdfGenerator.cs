using CRM.Application.Features.Contracts.DTOs;

namespace CRM.Application.Interfaces.Contracts;

/// <summary>Toàn bộ dữ liệu cần để render 1 file PDF hợp đồng — số liệu "cứng" (giá trị,
/// mã hợp đồng, sản phẩm...) lấy từ DB, phần "trình bày" (bên A/B, nội dung điều khoản)
/// lấy từ request người dùng gửi lên khi bấm Gửi email (xem README ở ContractPrintDtos.cs).</summary>
public class ContractPdfModel
{
    public required string MaHopDong { get; set; }
    public DateOnly? NgayKy { get; set; }
    public int? ThoiHan { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public required string HinhThucThanhToanLabel { get; set; }
    public required decimal GiaTriHopDong { get; set; }

    public required string TenKhachHang { get; set; }
    public string? KhachMaSoThue { get; set; }
    public string? KhachDienThoai { get; set; }
    public string? KhachEmail { get; set; }

    public List<ContractPdfProductLine> SanPham { get; set; } = new();
    public List<ContractPdfLichThanhToan> LichThanhToan { get; set; } = new();

    public required ContractPartyInfoDto BenA { get; set; }
    public required ContractPartyInfoDto BenB { get; set; }
    public string? DiaDiemKy { get; set; }
    public bool VatIncluded { get; set; } = true;
    public int BaoHanhThang { get; set; } = 12;
    public decimal MucPhatViPham { get; set; } = 8;
    public int SoBan { get; set; } = 2;
    public required ContractClauseTextsDto ClauseTexts { get; set; }
}

public class ContractPdfProductLine
{
    public required string TenSP { get; set; }
    public string? DonVi { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}

public class ContractPdfLichThanhToan
{
    public int SoDot { get; set; }
    public decimal SoTien { get; set; }
    public DateOnly HanThanhToan { get; set; }
}

public interface IContractPdfGenerator
{
    /// <summary>Sinh file PDF hợp đồng từ model — thuần render, không đụng DB.</summary>
    byte[] Generate(ContractPdfModel model);
}
