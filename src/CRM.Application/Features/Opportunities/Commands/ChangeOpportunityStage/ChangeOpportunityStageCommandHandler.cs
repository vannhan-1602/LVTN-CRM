using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Opportunities;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Opportunities.Commands.ChangeOpportunityStage;

public class ChangeOpportunityStageCommandHandler : IRequestHandler<ChangeOpportunityStageCommand, OpportunityDto>
{
    private const string AuditTable = "BH_CoHoiBanHang";
    private readonly IOpportunityRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogPublisher _audit;
    private readonly ILogger<ChangeOpportunityStageCommandHandler> _logger;

    public ChangeOpportunityStageCommandHandler(IOpportunityRepository repo, IUnitOfWork uow,
        ICurrentUserService currentUser, IAuditLogPublisher audit,
        ILogger<ChangeOpportunityStageCommandHandler> logger)
    { _repo = repo; _uow = uow; _currentUser = currentUser; _audit = audit; _logger = logger; }

    public async Task<OpportunityDto> Handle(ChangeOpportunityStageCommand req, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(nameof(CoHoiBanHang), req.Id);

        if (_currentUser.Role == Roles.Sale && entity.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền cập nhật cơ hội của nhân viên khác.");

        // Khi ThanhCong → TyLeThanhCong = 100; ThatBai → 0
        if (req.GiaiDoan == CoHoiGiaiDoan.ThanhCong.ToString()) entity.TyLeThanhCong = 100;
        if (req.GiaiDoan == CoHoiGiaiDoan.ThatBai.ToString()) entity.TyLeThanhCong = 0;
        if (!string.IsNullOrWhiteSpace(req.GhiChu)) entity.GhiChu = req.GhiChu.Trim();
        entity.GiaiDoan = req.GiaiDoan;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        var dto = await _repo.GetByIdEnrichedAsync(req.Id, ct)!;
        try { await _audit.PublishAsync(AuditTable, req.Id, "UPDATE", null, dto, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit failed"); }
        return dto!;
    }
}