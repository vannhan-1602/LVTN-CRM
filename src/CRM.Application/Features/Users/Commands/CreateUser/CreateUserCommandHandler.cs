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
       
        var validation = await _repository.ValidateNewUserAsync(
            request.Username, request.Email, request.RoleId, request.PhongBanId, request.ChucVuId, ct);

        if (!validation.UsernameAvailable)
            throw new BusinessRuleException($"Tên đăng nhập '{request.Username}' đã tồn tại.");

        if (!validation.RoleValid)
            throw new BusinessRuleException("Vai trò không hợp lệ.");

        if (!validation.EmailAvailable)
            throw new BusinessRuleException($"Email '{request.Email}' đã được sử dụng.");

        if (!validation.PhongBanValid)
            throw new BusinessRuleException("Phòng ban không hợp lệ.");

        if (!validation.ChucVuValid)
            throw new BusinessRuleException("Chức vụ không hợp lệ.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var newUserId = await _repository.CreateAsync(
            request.Username, passwordHash, request.RoleId,
            request.HoTen.Trim(), request.Email?.Trim(), request.SoDienThoai?.Trim(),
            request.PhongBanId, request.ChucVuId, ct);

       
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