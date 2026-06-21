using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Audit;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Entities.Sales;
using CRM.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Features.Contracts.Commands.DeleteContract;

public record DeleteContractCommand(ulong Id) : IRequest<bool>;

public class DeleteContractCommandValidator : AbstractValidator<DeleteContractCommand>
{
    public DeleteContractCommandValidator() => RuleFor(x => x.Id).GreaterThan(0UL);
}

public class DeleteContractCommandHandler : IRequestHandler<DeleteContractCommand, bool>
{
    private const string AuditTable = "HD_HopDong";
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogPublisher _auditLogPublisher;
    private readonly ILogger<DeleteContractCommandHandler> _logger;

    public DeleteContractCommandHandler(
        IContractRepository contractRepository, IUnitOfWork unitOfWork,
        IAuditLogPublisher auditLogPublisher, ILogger<DeleteContractCommandHandler> logger)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
        _auditLogPublisher = auditLogPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteContractCommand request, CancellationToken ct)
    {
        var contract = await _contractRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.Id);

        var deleted = await _contractRepository.DeleteAsync(request.Id, ct);
        if (!deleted) throw new NotFoundException(nameof(HopDong), request.Id);

        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await _auditLogPublisher.PublishAsync(AuditTable, request.Id, "DELETE",
                oldData: contract, newData: null, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit log failed for contract {Id}", request.Id); }

        return true;
    }
}
