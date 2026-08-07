using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Opportunities;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Opportunities.Commands.DeleteOpportunity;

public class DeleteOpportunityCommandHandler : IRequestHandler<DeleteOpportunityCommand, bool>
{
    private const string AuditTable = "BH_CoHoiBanHang";
    private readonly IOpportunityRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogPublisher _audit;
    private readonly ILogger<DeleteOpportunityCommandHandler> _logger;

    public DeleteOpportunityCommandHandler(IOpportunityRepository repo, IUnitOfWork uow,
        ICurrentUserService currentUser, IAuditLogPublisher audit,
        ILogger<DeleteOpportunityCommandHandler> logger)
    { _repo = repo; _uow = uow; _currentUser = currentUser; _audit = audit; _logger = logger; }

    public async Task<bool> Handle(DeleteOpportunityCommand req, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(nameof(CoHoiBanHang), req.Id);

        if (_currentUser.Role == Roles.Sale && entity.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền xóa cơ hội của nhân viên khác.");

        // Đồng bộ với UpdateOpportunityCommand: cơ hội đã chốt (Thành công/Thất bại) là bản ghi
        // lịch sử dùng để thống kê tỷ lệ chốt đơn — không cho xóa để tránh sai lệch số liệu.
        if (entity.GiaiDoan == CoHoiGiaiDoan.ThanhCong.ToString() || entity.GiaiDoan == CoHoiGiaiDoan.ThatBai.ToString())
            throw new BusinessRuleException(
                $"Cơ hội đã ở trạng thái '{entity.GiaiDoan}', không thể xóa (dữ liệu lịch sử dùng cho thống kê).");

        await _repo.SoftDeleteAsync(req.Id, ct);
        await _uow.SaveChangesAsync(ct);
        try { await _audit.PublishAsync(AuditTable, req.Id, "DELETE", entity, null, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit failed"); }
        return true;
    }
}