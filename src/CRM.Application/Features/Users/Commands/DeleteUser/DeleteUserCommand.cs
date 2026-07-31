using CRM.Application.Common.Constants;
using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(uint Id) : IRequest<bool>;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator() => RuleFor(x => x.Id).GreaterThan(0U);
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private const string AuditTable = "HT_User";

    private readonly IUserManagementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUserManagementRepository repository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ICurrentUserService currentUser,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);

        // Chặn Admin tự xóa chính mình -> tránh khóa hệ thống không còn ai quản trị
        if (_currentUser.UserId.HasValue && _currentUser.UserId.Value == request.Id)
            throw new BusinessRuleException("Không thể xóa chính tài khoản đang đăng nhập.");

        // Chặn xóa Admin cuối cùng còn đang hoạt động (trường hợp Admin A xóa Admin B
        // khi B là Admin active duy nhất còn lại).
        if (string.Equals(existing.RoleName, Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            var allUsers = await _repository.GetAllAsync(ct);
            var soAdminKhac = allUsers.Count(u =>
                u.Id != request.Id &&
                string.Equals(u.RoleName, Roles.Admin, StringComparison.OrdinalIgnoreCase) &&
                u.TrangThai == "Active");

            if (soAdminKhac == 0)
                throw new BusinessRuleException(
                    "Không thể xóa Admin cuối cùng đang hoạt động trong hệ thống.");
        }

        var deleted = await _repository.DeleteAsync(request.Id, ct);
        if (!deleted) throw new NotFoundException("User", request.Id);

        // Xóa cứng HT_User có thể đụng FK RESTRICT ở nhiều bảng nghiệp vụ (Khách hàng,
        // Lead, Cơ hội, Hợp đồng, Mốc triển khai, Phiếu thu/chi, Thẻ kho...) nếu tài khoản
        // này đã từng tạo/phụ trách dữ liệu thật. Bắt lỗi ở đây để trả thông báo rõ ràng
        // thay vì để lộ ra 1 lỗi SQL thô — trường hợp này nên dùng ToggleUserStatus (Locked)
        // để vô hiệu hóa tài khoản thay vì xóa.
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new BusinessRuleException(
                "Không thể xóa tài khoản này vì đã có dữ liệu nghiệp vụ gắn với tài khoản " +
                "(khách hàng, lead, hợp đồng, phiếu thu/chi...). Vui lòng khóa tài khoản " +
                "(chuyển trạng thái sang 'Locked') thay vì xóa.");
        }

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "DELETE",
                oldData: existing, newData: null, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for delete user {Id}", request.Id); }

        return true;
    }
}