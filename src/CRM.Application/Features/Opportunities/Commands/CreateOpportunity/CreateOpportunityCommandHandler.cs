using CRM.Application.Common.Constants;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Opportunities;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Opportunities.Commands.CreateOpportunity;

public class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, OpportunityDto>
{
    private const string AuditTable = "BH_CoHoiBanHang";
    private readonly IOpportunityRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogPublisher _audit;
    private readonly ILogger<CreateOpportunityCommandHandler> _logger;

    public CreateOpportunityCommandHandler(IOpportunityRepository repo, IUnitOfWork uow,
        ICurrentUserService currentUser, IAuditLogPublisher audit,
        ILogger<CreateOpportunityCommandHandler> logger)
    { _repo = repo; _uow = uow; _currentUser = currentUser; _audit = audit; _logger = logger; }

    public async Task<OpportunityDto> Handle(CreateOpportunityCommand req, CancellationToken ct)
    {
        var entity = new CoHoiBanHang
        {
            TenThuongVu = req.TenThuongVu.Trim(),
            GiaiDoan = CoHoiGiaiDoan.KhaoSat.ToString(),
            KhachHangId = req.KhachHangId,
            LeadId = req.LeadId,
            TyLeThanhCong = req.TyLeThanhCong,
            DoanhThuKyVong = req.DoanhThuKyVong,
            GhiChu = req.GhiChu?.Trim(),
            NgayDuKien = req.NgayDuKien,
            // Sale tự động gán cho chính mình; Manager có thể để null (quản lý toàn bộ)
            NhanVienPhuTrachId = _currentUser.Role == Roles.Sale ? (int?)_currentUser.UserId : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        var dto = await _repo.GetByIdEnrichedAsync(created.Id, ct)!;
        try { await _audit.PublishAsync(AuditTable, created.Id, "INSERT", null, dto, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit failed"); }
        return dto!;
    }
}