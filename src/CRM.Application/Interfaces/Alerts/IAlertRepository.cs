using CRM.Application.Features.Alerts.DTOs;

namespace CRM.Application.Interfaces.Alerts;

public interface IAlertRepository
{
    /// <summary>Lead chưa gán NhanVienPhuTrachId — Admin/Manager cần phân công.</summary>
    Task<DashboardAlertGroupDto> GetLeadsChuaPhanCongAsync(CancellationToken ct = default);

    /// <summary>Customer chưa gán NhanVienPhuTrachId — Admin/Manager cần phân công.</summary>
    Task<DashboardAlertGroupDto> GetCustomersChuaPhanCongAsync(CancellationToken ct = default);

    /// <summary>Ticket chưa gán cả NhanVienTiepNhan lẫn NhanVienXuLy, chưa đóng — Admin/Manager cần phân công.</summary>
    Task<DashboardAlertGroupDto> GetTicketsChuaPhanCongAsync(CancellationToken ct = default);

    /// <summary>Ticket ưu tiên KhanCap, chưa đóng. nhanVienId = null nghĩa là toàn team (Manager/Admin).</summary>
    Task<DashboardAlertGroupDto> GetTicketKhanCapAsync(uint? nhanVienId, CancellationToken ct = default);

    /// <summary>Ticket đã quá hạn xử lý SLA (ThoiHanSLA đã qua), chưa đóng.</summary>
    Task<DashboardAlertGroupDto> GetTicketQuaHanSlaAsync(uint? nhanVienId, CancellationToken ct = default);

    /// <summary>Ticket sắp tới giờ hẹn xử lý (NgayHenXuLy trong 48h tới), chưa đóng.</summary>
    Task<DashboardAlertGroupDto> GetTicketSapHenXuLyAsync(uint? nhanVienId, CancellationToken ct = default);

    /// <summary>Ticket tự động loại "Nhắc thanh toán" / "Nhắc gia hạn hợp đồng", chưa đóng.</summary>
    Task<DashboardAlertGroupDto> GetTicketNhacThanhToanGiaHanAsync(uint? nhanVienId, CancellationToken ct = default);

    /// <summary>Đợt trong lịch trả góp (HD_LichThanhToan) đang chờ/quá hạn thanh toán — toàn hệ thống, cho Accountant.</summary>
    Task<DashboardAlertGroupDto> GetDotThanhToanCanXuLyAsync(CancellationToken ct = default);


    Task<DashboardAlertGroupDto> GetHoaDonConNoAsync(CancellationToken ct = default);
}