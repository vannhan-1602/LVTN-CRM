using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Mappings;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Leads.Commands.AssignLead;

/// <summary>
/// Gán (hoặc tự nhận) nhân viên phụ trách cho Lead — dùng chủ yếu cho hàng chờ Lead
/// chưa gán (vd: Lead từ form public trên landing page, NhanVienPhuTrachId = null).
///
/// Quy tắc quyền hạn: Sale chỉ được thao tác trên Lead đang chưa ai phụ trách hoặc
/// đang là của chính mình (tự nhận) — không được chuyển Lead đang thuộc về đồng
/// nghiệp khác cho mình hoặc cho người khác. Manager không bị giới hạn ở đây,
/// có thể điều phối/chuyển Lead cho bất kỳ nhân viên nào bất kể trạng thái hiện tại.
/// </summary>
public class AssignLeadCommandHandler : IRequestHandler<AssignLeadCommand, LeadDto>
{
    private const string AuditTable = "KH_Lead";
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AssignLeadCommandHandler> _logger;

    public AssignLeadCommandHandler(
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ICurrentUserService currentUser,
        ILogger<AssignLeadCommandHandler> logger)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<LeadDto> Handle(AssignLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Lead), request.Id);

        uint? restrictIfCurrentOwnerNot = null;
        if (_currentUser.Role == Roles.Sale)
        {
            // Sale chỉ được thao tác trên Lead hiện đang là của mình (hoặc chưa ai
            // phụ trách — NhanVienPhuTrachId null) và chỉ được tự gán cho chính mình,
            // không được chuyển Lead cho đồng nghiệp khác.
            var isOwnedByMeOrUnassigned = lead.NhanVienPhuTrachId == null || lead.NhanVienPhuTrachId == _currentUser.UserId;
            if (!isOwnedByMeOrUnassigned)
                throw new ForbiddenException("Bạn không có quyền thao tác trên Lead của nhân viên khác.");

            if (request.NhanVienPhuTrachId.HasValue && request.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn chỉ có thể tự nhận phụ trách Lead, không thể gán cho người khác.");

            // Điều kiện chống race condition được đẩy xuống tận câu SQL (TryAssignAsync) —
            // check phía trên chỉ để trả lỗi RÕ NGHĨA (403) cho trường hợp thường gặp; check
            // dưới DB mới là lớp an toàn THẬT khi 2 Sale bấm tự nhận cùng lúc.
            restrictIfCurrentOwnerNot = _currentUser.UserId;
        }

        if (lead.TinhTrang == LeadTinhTrang.DaChuyenDoi)
            throw new BusinessRuleException("Lead đã chuyển đổi thành khách hàng, không thể gán phụ trách.");

        var oldDto = LeadMapper.ToDto(lead);

        var assigned = await _leadRepository.TryAssignAsync(
            request.Id, request.NhanVienPhuTrachId, restrictIfCurrentOwnerNot, cancellationToken);
        if (!assigned)
            throw new BusinessRuleException(
                "Lead này vừa được nhân viên khác nhận trước — vui lòng tải lại trang.");

        var updated = await _leadRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(Lead), request.Id);
        var newDto = LeadMapper.ToDto(updated);
        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, lead.Id, "UPDATE",
                oldData: oldDto, newData: newDto, cancellationToken);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for lead {Id}", lead.Id); }

        return newDto;
    }
}