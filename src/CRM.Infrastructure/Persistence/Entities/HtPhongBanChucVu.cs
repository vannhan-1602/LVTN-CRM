namespace CRM.Infrastructure.Persistence.Entities;

public class HtPhongBan
{
    public ushort Id { get; set; }
    public string TenPhongBan { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<HtThongTinNhanSu> NhanSus { get; set; } = [];
}

public class HtChucVu
{
    public ushort Id { get; set; }
    public string TenChucVu { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<HtThongTinNhanSu> NhanSus { get; set; } = [];
}

