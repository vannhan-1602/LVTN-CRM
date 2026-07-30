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

namespace CRM.Application.Features.Opportunities.Commands.UpdateOpportunity;

public class UpdateOpportunityCommandHandler : IRequestHandler<UpdateOpportunityCommand, OpportunityDto>
{
    private const string AuditTable = "BH_CoHoiBanHang";
    private readonly IOpportunityRepository _repo;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogPublisher _audit;
    private readonly ILogger<UpdateOpportunityCommandHandler> _logger;

    public UpdateOpportunityCommandHandler(IOpportunityRepository repo,
        ICustomerRepository customerRepository, ILeadRepository leadRepository, IUnitOfWork uow,
        ICurrentUserService currentUser, IAuditLogPublisher audit,
        ILogger<UpdateOpportunityCommandHandler> logger)
    {
        _repo = repo; _customerRepository = customerRepository; _leadRepository = leadRepository;
        _uow = uow; _currentUser = currentUser; _audit = audit; _logger = logger;
    }

    public async Task<OpportunityDto> Handle(UpdateOpportunityCommand req, CancellationToken ct)
    {
        var existing = await _repo.GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(nameof(CoHoiBanHang), req.Id);

        if (_currentUser.Role == Roles.Sale && existing.NhanVienPhuTrachId != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền sửa cơ hội của nhân viên khác.");

        // Cơ hội đã chốt (Thành công/Thất bại) là bản ghi lịch sử, không cho sửa nữa.
        if (existing.GiaiDoan == CoHoiGiaiDoan.ThanhCong.ToString() || existing.GiaiDoan == CoHoiGiaiDoan.ThatBai.ToString())
            throw new BusinessRuleException(
                $"Cơ hội đã ở trạng thái '{existing.GiaiDoan}', không thể chỉnh sửa.");

        // Khách hàng/Lead phải tồn tại; Sale chỉ được gắn cơ hội vào KH/Lead mình phụ trách.
        if (req.KhachHangId.HasValue)
        {
            var khachHang = await _customerRepository.GetByIdAsync(req.KhachHangId.Value, ct)
                ?? throw new NotFoundException("Khách hàng", req.KhachHangId.Value);
            if (_currentUser.Role == Roles.Sale && khachHang.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền gắn cơ hội vào khách hàng của nhân viên khác.");
        }
        if (req.LeadId.HasValue)
        {
            var lead = await _leadRepository.GetByIdAsync(req.LeadId.Value, cancellationToken: ct)
                ?? throw new NotFoundException("Lead", req.LeadId.Value);
            if (_currentUser.Role == Roles.Sale && lead.NhanVienPhuTrachId != _currentUser.UserId)
                throw new ForbiddenException("Bạn không có quyền gắn cơ hội vào lead của nhân viên khác.");
        }

        var oldDto = await _repo.GetByIdEnrichedAsync(req.Id, ct);

        existing.TenThuongVu = req.TenThuongVu.Trim();
        existing.KhachHangId = req.KhachHangId;
        existing.LeadId = req.LeadId;
        existing.TyLeThanhCong = req.TyLeThanhCong;
        existing.DoanhThuKyVong = req.DoanhThuKyVong;
        existing.GhiChu = req.GhiChu?.Trim();
        existing.NgayDuKien = req.NgayDuKien;
        // Sale không được đổi người phụ trách (chỉ Manager mới có quyền này)
        if (_currentUser.Role != Roles.Sale)
            existing.NhanVienPhuTrachId = (int?)req.NhanVienPhuTrachId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(existing, ct);
        await _uow.SaveChangesAsync(ct);

        var newDto = await _repo.GetByIdEnrichedAsync(req.Id, ct)!;
        try { await _audit.PublishAsync(AuditTable, req.Id, "UPDATE", oldDto, newDto, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit failed"); }
        return newDto!;
    }
}