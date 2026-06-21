using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Users.Commands.ResetPassword;

public record ResetPasswordCommand(uint Id, string NewPassword) : IRequest<bool>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0U);
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu tối thiểu 6 ký tự.");
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private const string AuditTable = "HT_User";

    private readonly IUserManagementRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUserManagementRepository repository, IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork, IAuditLogPublisher auditLogPublisher,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);

        var hash = _passwordHasher.Hash(request.NewPassword);
        await _repository.UpdatePasswordAsync(request.Id, hash, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            // Không log mật khẩu thật vào audit — chỉ ghi nhận sự kiện
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: new { Action = "ResetPassword", existing.Username },
                newData: new { Action = "ResetPassword", existing.Username, ResetAt = DateTime.UtcNow },
                ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for reset password {Id}", request.Id); }

        return true;
    }
}
