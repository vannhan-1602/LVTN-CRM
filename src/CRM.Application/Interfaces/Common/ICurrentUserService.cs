namespace CRM.Application.Interfaces.Common;

public interface ICurrentUserService
{
    uint? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }

   
    //Id nhân sự (HT_ThongTinNhanSu.Id) — dùng để filter dữ liệu theo phạm vi phụ trách
    uint? NhanSuId { get; }

    //Tên vai trò hiện tại (Admin/Manager/Sale/Accountant), đọc từ  Role.
    string? Role { get; }
}
