using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private const string AuditTable = "HT_User";

    private readonly IUserRepository _userRepository;
    private readonly IUserManagementRepository _userManagementRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUserManagementRepository userManagementRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _userManagementRepository = userManagementRepository;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            throw new UnauthorizedException("Không xác định được người dùng hiện tại.");

        var userId = _currentUser.UserId.Value;

        var account = await _userRepository.GetByIdWithPasswordAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        // 5a. Mật khẩu hiện tại không đúng
        if (!_passwordHasher.Verify(request.CurrentPassword, account.Password))
            throw new BusinessRuleException("Mật khẩu hiện tại không chính xác.");

        var hash = _passwordHasher.Hash(request.NewPassword);
        await _userManagementRepository.UpdatePasswordAsync(userId, hash, ct);

        // Bước 8 trong luồng sự kiện chính: "yêu cầu đăng nhập lại" — thu hồi JWT hiện tại
        // ngay để buộc người dùng phải đăng nhập lại bằng mật khẩu mới.
        await _userManagementRepository.IncrementTokenVersionAsync(userId, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(
                AuditTable, userId, "UPDATE",
                oldData: new { Action = "ChangePassword", account.Username },
                newData: new { Action = "ChangePassword", account.Username, ChangedAt = DateTime.UtcNow },
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit log failed for change password {UserId}", userId);
        }

        return true;
    }
}
