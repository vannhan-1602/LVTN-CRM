using CRM.Application.Common.Constants;
using CRM.Application.Features.Alerts.DTOs;
using CRM.Application.Interfaces.Alerts;
using CRM.Application.Interfaces.Common;
using MediatR;

namespace CRM.Application.Features.Alerts.Queries.GetDashboardAlerts;

/// <summary>
/// Mỗi vai trò thấy 1 tập cảnh báo khác nhau, phù hợp với phạm vi công việc của họ:
///   - Admin/Manager: Lead/Customer chưa phân công (toàn team), Ticket khẩn cấp + quá hạn SLA (toàn team).
///   - Sale: Ticket khẩn cấp / sắp hẹn xử lý / nhắc thanh toán-gia hạn của riêng mình.
///   - Accountant: Đợt thanh toán trả góp toàn hệ thống đang chờ/quá hạn.
/// Không gửi email — chỉ tổng hợp để hiển thị trên Dashboard, bấm vào thì FE điều hướng
/// tới trang chi tiết theo EntityType/EntityId.
/// </summary>
public class GetDashboardAlertsQueryHandler : IRequestHandler<GetDashboardAlertsQuery, DashboardAlertsDto>
{
    private readonly IAlertRepository _alertRepository;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardAlertsQueryHandler(IAlertRepository alertRepository, ICurrentUserService currentUser)
    {
        _alertRepository = alertRepository;
        _currentUser = currentUser;
    }

    public async Task<DashboardAlertsDto> Handle(GetDashboardAlertsQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUser.Role;
        var userId = _currentUser.UserId;

        var groups = new List<DashboardAlertGroupDto>();

        if (role == Roles.Admin || role == Roles.Manager)
        {
            groups.Add(await _alertRepository.GetLeadsChuaPhanCongAsync(cancellationToken));
            groups.Add(await _alertRepository.GetCustomersChuaPhanCongAsync(cancellationToken));
            groups.Add(await _alertRepository.GetTicketKhanCapAsync(nhanVienId: null, cancellationToken));
            groups.Add(await _alertRepository.GetTicketQuaHanSlaAsync(nhanVienId: null, cancellationToken));
        }
        else if (role == Roles.Sale)
        {
            groups.Add(await _alertRepository.GetTicketKhanCapAsync(userId, cancellationToken));
            groups.Add(await _alertRepository.GetTicketSapHenXuLyAsync(userId, cancellationToken));
            groups.Add(await _alertRepository.GetTicketNhacThanhToanGiaHanAsync(userId, cancellationToken));
        }
        else if (role == Roles.Accountant)
        {
            groups.Add(await _alertRepository.GetDotThanhToanCanXuLyAsync(cancellationToken));
            groups.Add(await _alertRepository.GetTicketNhacThanhToanGiaHanAsync(nhanVienId: null, cancellationToken));
        }

        // Bỏ các nhóm rỗng để FE không phải hiển thị thẻ trống.
        groups.RemoveAll(g => g.Count == 0);

        return new DashboardAlertsDto
        {
            TongSoCanhBao = groups.Sum(g => g.Count),
            Groups = groups
        };
    }
}
