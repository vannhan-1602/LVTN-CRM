using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Opportunities.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Customers;
using CRM.Application.Interfaces.Leads;
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
    private readonly ICustomerRepository _customerRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogPublisher _audit;
    private readonly ILogger<CreateOpportunityCommandHandler> _logger;

    public CreateOpportunityCommandHandler(IOpportunityRepository repo,
        ICustomerRepository customerRepository, ILeadRepository leadRepository, IUnitOfWork uow,
        ICurrentUserService currentUser, IAuditLogPublisher audit,
        ILogger<CreateOpportunityCommandHandler> logger)
    {
        _repo = repo; _customerRepository = customerRepository; _leadRepository = leadRepository;
        _uow = uow; _currentUser = currentUser; _audit = audit; _logger = logger;
    }

    public async Task<OpportunityDto> Handle(CreateOpportunityCommand req, CancellationToken ct)
    {
        // Khách hàng/Lead phải tồn tại; Sale chỉ được tạo cơ hội cho KH/Lead mình phụ trách.
        if (req.KhachHangId.HasValue)
        {
            var khachHang = await _customerRepository.GetByIdAsync(req.KhachHangId.Value, ct)
                ?? throw new NotFoundException("Khách hàng", req.KhachHangId.Value);
            if (_currentUser.Role == Roles.Sale && khachHang.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền tạo cơ hội bán hàng cho khách hàng của nhân viên khác.");
        }
        if (req.LeadId.HasValue)
        {
            var lead = await _leadRepository.GetByIdAsync(req.LeadId.Value, cancellationToken: ct)
                ?? throw new NotFoundException("Lead", req.LeadId.Value);
            if (_currentUser.Role == Roles.Sale && lead.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền tạo cơ hội bán hàng cho lead của nhân viên khác.");
        }

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
            // Sale tự động gán cho chính mình (không được chọn người khác);
            // Manager được quyền chỉ định người phụ trách bất kỳ (hoặc để trống).
            NhanVienPhuTrachId = _currentUser.Role == Roles.Sale
                ? (int?)_currentUser.UserId
                : (int?)req.NhanVienPhuTrachId,
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