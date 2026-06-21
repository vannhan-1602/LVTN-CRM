using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Users.DTOs;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Users;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Users.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(uint Id, string TrangThai) : IRequest<UserDto>;

public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    private static readonly string[] ValidStatuses = ["Active", "Locked", "Inactive"];

    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0U);
        RuleFor(x => x.TrangThai)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Trạng thái phải là Active, Locked hoặc Inactive.");
    }
}

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, UserDto>
{
    private const string AuditTable = "HT_User";

    private readonly IUserManagementRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<ToggleUserStatusCommandHandler> _logger;

    public ToggleUserStatusCommandHandler(
        IUserManagementRepository repository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ILogger<ToggleUserStatusCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<UserDto> Handle(ToggleUserStatusCommand request, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);

        await _repository.UpdateStatusAsync(request.Id, request.TrangThai, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "UPDATE",
                oldData: existing, newData: updated, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for toggle status {Id}", request.Id); }

        return updated;
    }
}
