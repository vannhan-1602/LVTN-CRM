using System.Globalization;
using System.Text;
using CRM.Application.Interfaces.Contracts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRM.Infrastructure.Services.Pdf;

// Sinh PDF hợp đồng ở server bằng QuestPDF (Community license — miễn phí, không cần
// trình duyệt/headless-chrome). Nội dung & thứ tự các Điều được dựng LẠI y hệt
// ContractPrintPage.jsx ở frontend để bản PDF gửi email khớp với bản người dùng đã
// xem trước và tinh chỉnh. Không đọc/ghi gì vào DB — thuần render từ ContractPdfModel.
public class ContractPdfGenerator : IContractPdfGenerator
{
    static ContractPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public byte[] Generate(ContractPdfModel m)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.7f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(4);

                    col.Item().AlignCenter().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold();
                    col.Item().AlignCenter().Text("Độc lập - Tự do - Hạnh phúc").Bold();
                    col.Item().AlignCenter().Text("—————oOo—————");

                    col.Item().PaddingTop(12).AlignCenter()
                        .Text("HỢP ĐỒNG CUNG CẤP GIẢI PHÁP PHẦN MỀM").Bold().FontSize(14);
                    col.Item().AlignCenter().Text($"Số: {m.MaHopDong}");

                    col.Item().PaddingTop(10).Text("Căn cứ:");
                    col.Item().Text("• Bộ luật Dân sự số 91/2015/QH13 ngày 24/11/2015;");
                    col.Item().Text("• Luật Thương mại số 36/2005/QH11 ngày 14/06/2005;");
                    col.Item().Text("• Luật Sở hữu trí tuệ số 50/2005/QH11 (sửa đổi, bổ sung 2009, 2019, 2022);");
                    col.Item().Text("• Nhu cầu và khả năng thực tế của hai bên.");

                    var ngayKyText = m.NgayKy.HasValue ? m.NgayKy.Value.ToString("dd/MM/yyyy") : "…";
                    var diaDiemText = string.IsNullOrWhiteSpace(m.DiaDiemKy) ? "" : $", tại {m.DiaDiemKy}";
                    col.Item().PaddingTop(4).Text($"Hôm nay, ngày {ngayKyText}{diaDiemText}, chúng tôi gồm:");

                    col.Item().PaddingTop(6).Text("BÊN A (BÊN CUNG CẤP):").Bold();
                    PartyBlock(col, m.BenA, null, null, null);

                    col.Item().PaddingTop(6).Text("BÊN B (BÊN SỬ DỤNG DỊCH VỤ):").Bold();
                    PartyBlock(col, m.BenB, m.TenKhachHang, m.KhachMaSoThue, m.KhachDienThoai, m.KhachEmail);

                    col.Item().PaddingTop(8).Text("Hai bên đồng ý ký kết hợp đồng với các điều khoản sau:");

                    // Điều 1
                    Heading(col, "Điều 1. Đối tượng và nội dung hợp đồng");
                    if (m.SanPham.Count > 0)
                        ProductTable(col, m.SanPham);
                    else
                        col.Item().Text($"Bên A cung cấp giải pháp/dịch vụ phần mềm theo thỏa thuận giữa hai bên, giá trị: {FormatMoney(m.GiaTriHopDong)}.");

                    // Điều 2
                    Heading(col, "Điều 2. Giá trị hợp đồng và thuế");
                    var vatText = m.VatIncluded
                        ? "đã bao gồm thuế GTGT theo quy định hiện hành"
                        : "chưa bao gồm thuế GTGT; thuế GTGT tính theo quy định hiện hành tại thời điểm xuất hóa đơn";
                    col.Item().Text($"Tổng giá trị hợp đồng: {FormatMoney(m.GiaTriHopDong)} ({vatText}). Bên A có trách nhiệm xuất hóa đơn điện tử theo quy định tại Nghị định số 123/2020/NĐ-CP khi Bên B thanh toán từng đợt hoặc theo thỏa thuận cụ thể giữa hai bên.");
                    col.Item().Text(t =>
                    {
                        t.Span("Bằng chữ: ").Italic();
                        t.Span(SoTienBangChu(m.GiaTriHopDong) + ".").Italic();
                    });

                    // Điều 3
                    Heading(col, "Điều 3. Phương thức và tiến độ thanh toán");
                    col.Item().Text($"Hình thức thanh toán: {m.HinhThucThanhToanLabel}. Bên B thanh toán bằng hình thức chuyển khoản vào tài khoản của Bên A nêu tại phần thông tin các bên, hoặc tiền mặt theo thỏa thuận cụ thể giữa hai bên.");
                    if (m.LichThanhToan.Count > 0)
                        PaymentScheduleTable(col, m.LichThanhToan);
                    col.Item().Text(Clause(m.ClauseTexts.Dieu3Cham));

                    Heading(col, "Điều 4. Thời hạn và tiến độ thực hiện");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu4));

                    Heading(col, "Điều 5. Quyền và nghĩa vụ của Bên A");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu5));

                    Heading(col, "Điều 6. Quyền và nghĩa vụ của Bên B");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu6));

                    Heading(col, "Điều 7. Bảo hành, bảo trì và hỗ trợ kỹ thuật");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu7));

                    Heading(col, "Điều 8. Quyền sở hữu trí tuệ");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu8));

                    Heading(col, "Điều 9. Bảo mật thông tin và dữ liệu cá nhân");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu9));

                    Heading(col, "Điều 10. Phạt vi phạm và bồi thường thiệt hại");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu10));

                    Heading(col, "Điều 11. Sự kiện bất khả kháng");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu11));

                    Heading(col, "Điều 12. Giải quyết tranh chấp");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu12));

                    Heading(col, "Điều 13. Chấm dứt hợp đồng");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu13));

                    Heading(col, "Điều 14. Hiệu lực và điều khoản chung");
                    col.Item().Text(Clause(m.ClauseTexts.Dieu14));

                    col.Item().PaddingTop(24).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("ĐẠI DIỆN BÊN A").Bold();
                            c.Item().Text("(Ký, ghi rõ họ tên, đóng dấu)").Italic().FontSize(9);
                        });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("ĐẠI DIỆN BÊN B").Bold();
                            c.Item().Text("(Ký, ghi rõ họ tên, đóng dấu)").Italic().FontSize(9);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                    t.Span("Trang ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static string Clause(string? text) =>
        string.IsNullOrWhiteSpace(text) ? "" : text;

    private static void Heading(ColumnDescriptor col, string text) =>
        col.Item().PaddingTop(6).Text(text).Bold();

    private static void PartyBlock(
        ColumnDescriptor col, CRM.Application.Features.Contracts.DTOs.ContractPartyInfoDto p,
        string? tenKhachHangDb, string? maSoThueDb, string? dienThoaiDb, string? emailDb = null)
    {
        var ten = tenKhachHangDb ?? (string.IsNullOrWhiteSpace(p.TenCongTy) ? "……………………………………" : p.TenCongTy);
        col.Item().Text(ten!);
        col.Item().Text($"Địa chỉ: {(string.IsNullOrWhiteSpace(p.DiaChi) ? "……………………………………" : p.DiaChi)}");

        var mst = maSoThueDb ?? p.MaSoThue;
        var mstLine = $"Mã số thuế: {(string.IsNullOrWhiteSpace(mst) ? "……………" : mst)}";
        if (!string.IsNullOrWhiteSpace(p.GiayDkkd)) mstLine += $" — GCN ĐKKD số: {p.GiayDkkd}";
        col.Item().Text(mstLine);

        var dienThoai = dienThoaiDb ?? p.DienThoai;
        var email = emailDb ?? p.Email;
        col.Item().Text($"Điện thoại: {(string.IsNullOrWhiteSpace(dienThoai) ? "……………" : dienThoai)} — Email: {(string.IsNullOrWhiteSpace(email) ? "……………" : email)}");

        if (!string.IsNullOrWhiteSpace(p.SoTaiKhoan) || !string.IsNullOrWhiteSpace(p.NganHang))
        {
            var stkLine = $"Số tài khoản: {(string.IsNullOrWhiteSpace(p.SoTaiKhoan) ? "……………" : p.SoTaiKhoan)}";
            if (!string.IsNullOrWhiteSpace(p.NganHang)) stkLine += $" tại {p.NganHang}";
            col.Item().Text(stkLine);
        }

        var dienDienDaiDien = $"Đại diện: {(string.IsNullOrWhiteSpace(p.NguoiDaiDien) ? "……………" : p.NguoiDaiDien)} — Chức vụ: {(string.IsNullOrWhiteSpace(p.ChucVu) ? "……………" : p.ChucVu)}";
        if (!string.IsNullOrWhiteSpace(p.Cccd)) dienDienDaiDien += $" — CCCD số: {p.Cccd}";
        col.Item().Text(dienDienDaiDien);
    }

    private static void ProductTable(ColumnDescriptor col, List<ContractPdfProductLine> lines)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(28);
                c.RelativeColumn(3);
                c.ConstantColumn(40);
                c.ConstantColumn(30);
                c.RelativeColumn(1.4f);
                c.RelativeColumn(1.6f);
            });

            table.Header(h =>
            {
                CellHeader(h.Cell(), "STT");
                CellHeader(h.Cell(), "Sản phẩm / Dịch vụ");
                CellHeader(h.Cell(), "ĐVT");
                CellHeader(h.Cell(), "SL");
                CellHeader(h.Cell(), "Đơn giá");
                CellHeader(h.Cell(), "Thành tiền");
            });

            int stt = 1;
            foreach (var l in lines)
            {
                Cell(table.Cell(), (stt++).ToString());
                Cell(table.Cell(), l.TenSP);
                Cell(table.Cell(), l.DonVi ?? "");
                Cell(table.Cell(), l.SoLuong.ToString());
                Cell(table.Cell(), FormatMoney(l.DonGia));
                Cell(table.Cell(), FormatMoney(l.ThanhTien));
            }
        });
    }

    private static void PaymentScheduleTable(ColumnDescriptor col, List<ContractPdfLichThanhToan> rows)
    {
        col.Item().PaddingTop(4).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1);
                c.RelativeColumn(2);
                c.RelativeColumn(2);
            });

            table.Header(h =>
            {
                CellHeader(h.Cell(), "Đợt");
                CellHeader(h.Cell(), "Số tiền");
                CellHeader(h.Cell(), "Hạn thanh toán");
            });

            foreach (var r in rows)
            {
                Cell(table.Cell(), $"Đợt {r.SoDot}");
                Cell(table.Cell(), FormatMoney(r.SoTien));
                Cell(table.Cell(), r.HanThanhToan.ToString("dd/MM/yyyy"));
            }
        });
    }

    private static void CellHeader(IContainer container, string text) =>
        container.Border(0.5f).Background(Colors.Grey.Lighten3).Padding(3)
            .Text(text).Bold().FontSize(9);

    private static void Cell(IContainer container, string text) =>
        container.Border(0.5f).Padding(3).Text(text).FontSize(9);

    private static string FormatMoney(decimal n) => n.ToString("N0", Vi) + " đ";

    // ── Đọc số tiền ra chữ (đơn vị: đồng) — cùng thuật toán với soTienBangChu ở
    // ContractPrintPage.jsx, port sang C# để bản PDF server khớp với bản xem trước. ──
    private static readonly string[] ChuSo =
        { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

    private static string DocBaSo(int so, bool daySo)
    {
        int tram = so / 100;
        int chuc = (so % 100) / 10;
        int donVi = so % 10;
        var sb = new StringBuilder();

        if (tram == 0 && !daySo)
        {
            // nhóm đầu tiên, không cần đọc "không trăm"
        }
        else
        {
            sb.Append(ChuSo[tram]).Append(" trăm ");
        }

        if (chuc == 0 && donVi > 0 && (tram > 0 || daySo)) sb.Append("linh ");
        else if (chuc == 1) sb.Append("mười ");
        else if (chuc > 1) sb.Append(ChuSo[chuc]).Append(" mươi ");

        if (chuc >= 2 && donVi == 1) sb.Append("mốt");
        else if (chuc >= 1 && donVi == 5) sb.Append("lăm");
        else if (donVi > 0) sb.Append(ChuSo[donVi]);

        return sb.ToString().Trim();
    }

    private static string SoTienBangChu(decimal n)
    {
        long so = (long)Math.Round(n, MidpointRounding.AwayFromZero);
        if (so == 0) return "Không đồng";

        var donVi = new[] { "", "nghìn", "triệu", "tỷ" };
        var groups = new List<int>();
        while (so > 0)
        {
            groups.Insert(0, (int)(so % 1000));
            so /= 1000;
        }

        var parts = new List<string>();
        for (int idx = 0; idx < groups.Count; idx++)
        {
            var g = groups[idx];
            if (g == 0) continue;
            bool daySo = idx > 0 && groups.Take(idx).Any(x => x > 0);
            var chu = DocBaSo(g, daySo);
            var donViText = donVi[groups.Count - 1 - idx];
            parts.Add(string.IsNullOrEmpty(donViText) ? chu : $"{chu} {donViText}");
        }

        var s = string.Join(" ", parts).Trim();
        if (s.Length == 0) return "Không đồng";
        return char.ToUpper(s[0]) + s.Substring(1) + " đồng";
    }
}
