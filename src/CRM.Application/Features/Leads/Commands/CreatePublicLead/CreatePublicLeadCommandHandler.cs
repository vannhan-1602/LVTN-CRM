using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using CRM.Domain.Enums;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Leads.Commands.CreatePublicLead;

public class CreatePublicLeadCommandValidator : AbstractValidator<CreatePublicLeadCommand>
{
    public CreatePublicLeadCommandValidator()
    {
        RuleFor(x => x.TenLead)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(150).WithMessage("Họ tên không quá 150 ký tự.");

        RuleFor(x => x.TenCongTy).MaximumLength(255).WithMessage("Tên công ty không quá 255 ký tự.");

        RuleFor(x => x.SoDienThoai)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .MaximumLength(20).WithMessage("Số điện thoại không quá 20 ký tự.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(150).WithMessage("Email không quá 150 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

// Tạo Lead từ form công khai trên landing page (không đăng nhập). Khác CreateLeadCommand
// (dùng nội bộ bởi Sale/Manager) ở chỗ: không có NhanVienPhuTrachId — lead vào hệ thống ở
// trạng thái "chưa gán", NguonLead cố định = "Website" để phân biệt với Lead tạo tay, và
// không throw nếu Email trùng với Lead khác (khách có thể điền lại form nhiều lần).
public class CreatePublicLeadCommandHandler : IRequestHandler<CreatePublicLeadCommand, ulong>
{
    private const string AuditTable = "KH_Lead";
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<CreatePublicLeadCommandHandler> _logger;

    public CreatePublicLeadCommandHandler(
        ILeadRepository leadRepository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ILogger<CreatePublicLeadCommandHandler> logger)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<ulong> Handle(CreatePublicLeadCommand request, CancellationToken ct)
    {
        var lead = new Lead
        {
            TenLead = request.TenLead.Trim(),
            TenCongTy = request.TenCongTy?.Trim(),
            SoDienThoai = request.SoDienThoai?.Trim(),
            Email = request.Email?.Trim(),
            NguonLead = "Website",
            TinhTrang = LeadTinhTrang.Moi,
            NhanVienPhuTrachId = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _leadRepository.AddAsync(lead, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, created.Id, "INSERT",
                oldData: null, newData: created, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for public lead {Id}", created.Id); }

        return created.Id;
    }
}