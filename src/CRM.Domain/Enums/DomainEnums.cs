namespace CRM.Domain.Enums;

public enum UserStatus
{
    Active,
    Locked,
    Inactive
}

public enum TicketPriority
{
    Thap,
    TrungBinh,
    Cao,
    KhanCap
}

public enum TicketStatus
{
    Moi,
    DangXuLy,
    ChoPhanHoi,
    Dong
}

public enum TicketSource
{
    Email,
    Phone,
    Web,
    Zalo,
    TrucTiep
}
public enum TicketPhanHoiLoai
{
    NoiBoXuLy,
    PhanHoiKhachHang,
    YeuCauBoSung,
    DongTicket
}

public enum CoHoiGiaiDoan
{
    KhaoSat,
    DeXuat,
    ThuongLuong,
    ThanhCong,
    ThatBai
}

public static class LeadTinhTrang
{
    public const string Moi = "Moi";
    public const string DangChamSoc = "DangChamSoc";
    public const string DaChuyenDoi = "DaChuyenDoi";
    public const string ThatBai = "ThatBai";

    public static readonly IReadOnlyList<string> All = [Moi, DangChamSoc, DaChuyenDoi, ThatBai];
}
