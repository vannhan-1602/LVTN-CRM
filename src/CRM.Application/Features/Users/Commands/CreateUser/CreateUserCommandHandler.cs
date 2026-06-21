using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Auth;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private const string AuditTable = "HT_User";

    private readonly IUserManagementRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserManagementRepository repository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher,
        ILogger<CreateUserCommandHandler> logger)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (await _repository.UsernameExistsAsync(request.Username, ct))
            throw new BusinessRuleException($"Tên đăng nhập '{request.Username}' đã tồn tại.");

        if (!await _repository.RoleExistsAsync(request.RoleId, ct))
            throw new BusinessRuleException("Vai trò không hợp lệ.");

        if (await _repository.EmailExistsAsync(request.Email, null, ct))
            throw new BusinessRuleException($"Email '{request.Email}' đã được sử dụng.");

        if (request.PhongBanId.HasValue && !await _repository.PhongBanExistsAsync(request.PhongBanId.Value, ct))
            throw new BusinessRuleException("Phòng ban không hợp lệ.");

        if (request.ChucVuId.HasValue && !await _repository.ChucVuExistsAsync(request.ChucVuId.Value, ct))
            throw new BusinessRuleException("Chức vụ không hợp lệ.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var newUserId = await _repository.CreateAsync(
            request.Username, passwordHash, request.RoleId,
            request.HoTen.Trim(), request.Email?.Trim(), request.SoDienThoai?.Trim(),
            request.PhongBanId, request.ChucVuId, ct);

        // Repository tự SaveChanges theo từng bước (cần Id nhân sự trước khi tạo user)
        var dto = await _repository.GetByIdAsync(newUserId, ct)
            ?? throw new BusinessRuleException("Tạo tài khoản thất bại.");

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, newUserId, "INSERT",
                oldData: null, newData: dto, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for user {Id}", newUserId); }

        return dto;
    }
}
