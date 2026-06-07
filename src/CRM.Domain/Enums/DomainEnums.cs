namespace CRM.Domain.Enums;

public enum UserStatus
{
    Active,
    Locked,
    Inactive
}

public enum AuditAction
{
    Insert,
    Update,
    Delete
}

public enum BaoGiaStatus
{
    Nhap,
    DaGui,
    TuChoi,
    ChapNhan
}

public enum HopDongStatus
{
    DangThucHien,
    TamDung,
    ThanhLy
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
    public const string Moi = "Mới";
    public const string DangChamSoc = "Đang chăm sóc";
    public const string DaChuyenDoi = "Đã chuyển đổi";
    public const string ThatBai = "Thất bại";
}
