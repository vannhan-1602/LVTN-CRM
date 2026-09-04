using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    uint Id,
    uint RoleId,
    string HoTen,
    string? Email,
    string? SoDienThoai,
    ushort? PhongBanId,
    ushort? ChucVuId
) : IRequest<UserDto>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0U);
        RuleFor(x => x.RoleId).GreaterThan(0U).WithMessage("Vui lòng chọn vai trò.");
        RuleFor(x => x.HoTen).NotEmpty().WithMessage("Họ tên không được để trống.").MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email không hợp lệ.");
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private const string AuditTable = "HT_User";

    private readonly IUserManagementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IUserManagementRepository repository,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);

       
        var validation = await _repository.ValidateUserUpdateAsync(
            request.Email, existing.NhanSuId, request.RoleId, request.PhongBanId, request.ChucVuId, ct);

        if (!validation.RoleValid)
            throw new BusinessRuleException("Vai trò không hợp lệ.");

        
        if (string.Equals(existing.RoleName, Roles.Admin, StringComparison.OrdinalIgnoreCase) &&
            request.RoleId != existing.RoleId)
        {
            var allUsers = await _repository.GetAllAsync(ct);
            var soAdminKhac = allUsers.Count(u =>
                u.Id != request.Id &&
                string.Equals(u.RoleName, Roles.Admin, StringComparison.OrdinalIgnoreCase) &&
                u.TrangThai == "Active");

            if (soAdminKhac == 0)
                throw new BusinessRuleException(
                    "Không thể hạ quyền Admin cuối cùng đang hoạt động trong hệ thống.");
        }

        if (!validation.EmailAvailable)
            throw new BusinessRuleException($"Email '{request.Email}' đã được sử dụng bởi nhân sự khác.");

        if (!validation.PhongBanValid)
            throw new BusinessRuleException("Phòng ban không hợp lệ.");

        if (!validation.ChucVuValid)
            throw new BusinessRuleException("Chức vụ không hợp lệ.");

        await _repository.UpdateAsync(
            request.Id, request.RoleId,
            request.HoTen.Trim(), request.Email?.Trim(), request.SoDienThoai?.Trim(),
            request.PhongBanId, request.ChucVuId, ct);

       
        if (request.RoleId != existing.RoleId)
            await _repository.IncrementTokenVersionAsync(request.Id, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: existing, newData: updated, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for user {Id}", request.Id); }

        return updated;
    }
}