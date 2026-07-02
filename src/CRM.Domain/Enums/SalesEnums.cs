namespace CRM.Domain.Enums;

/// <summary>Trạng thái sản phẩm/dịch vụ. Map với cột BH_SanPham.TrangThai (tinyint 0/1).</summary>
public enum ProductStatus
{
    NgungKinhDoanh = 0,
    DangKinhDoanh = 1
}

/// <summary>Trạng thái báo giá. Khớp enum HD_BaoGia.TrangThai trong DB.</summary>
public static class QuoteStatus
{
    public const string Nhap = "Nhap";
    public const string DaGui = "DaGui";
    public const string TuChoi = "TuChoi";
    public const string ChapNhan = "ChapNhan";

    public static readonly IReadOnlyList<string> All = [Nhap, DaGui, TuChoi, ChapNhan];
}

/// <summary>Trạng thái hợp đồng. Khớp enum HD_HopDong.TrangThai trong DB.</summary>
public static class ContractStatus
{
    public const string DangThucHien = "DangThucHien";
    public const string TamDung = "TamDung";
    public const string ThanhLy = "ThanhLy";

    public static readonly IReadOnlyList<string> All = [DangThucHien, TamDung, ThanhLy];
}

/// <summary>Trạng thái thanh toán hóa đơn. Khớp enum KT_HoaDon.TrangThaiThanhToan trong DB.</summary>
public static class InvoiceStatus
{
    public const string ChuaThanhToan = "ChuaThanhToan";
    public const string ThanhToan1Phan = "ThanhToan1Phan";
    public const string HoanTat = "HoanTat";

    public static readonly IReadOnlyList<string> All = [ChuaThanhToan, ThanhToan1Phan, HoanTat];
}

/// <summary>Loại phiếu thu/chi. Khớp enum KT_PhieuThuChi.LoaiPhieu trong DB.</summary>
public static class PaymentVoucherType
{
    public const string Thu = "Thu";
    public const string Chi = "Chi";

    public static readonly IReadOnlyList<string> All = [Thu, Chi];
}

/// <summary>Loại giao dịch kho. Khớp enum Kho_TheKho.LoaiGiaoDich trong DB.</summary>
public static class StockTransactionType
{
    public const string NhapMua = "NhapMua";
    public const string XuatBan = "XuatBan";
    public const string NhapTraKhach = "NhapTraKhach";
    public const string XuatTraNCC = "XuatTraNCC";
    public const string XuatHuy = "XuatHuy";
    public const string KiemKe = "KiemKe";

    public static readonly IReadOnlyList<string> All =
        [NhapMua, XuatBan, NhapTraKhach, XuatTraNCC, XuatHuy, KiemKe];

    /// <summary>Các loại giao dịch làm TĂNG tồn kho (số lượng dương).</summary>
    public static readonly IReadOnlyList<string> IncreaseTypes = [NhapMua, NhapTraKhach];

    /// <summary>Các loại giao dịch làm GIẢM tồn kho (số lượng âm).</summary>
    public static readonly IReadOnlyList<string> DecreaseTypes = [XuatBan, XuatTraNCC, XuatHuy];
}
