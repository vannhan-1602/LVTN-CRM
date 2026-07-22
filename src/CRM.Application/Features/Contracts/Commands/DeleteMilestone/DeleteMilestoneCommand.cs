using CRM.Application.Common.Exceptions;
using CRM.Application.Interfaces.Contracts;
using FluentValidation;
using MediatR;

namespace CRM.Application.Features.Contracts.Commands.DeleteMilestone;

public record DeleteMilestoneCommand(ulong Id) : IRequest<bool>;

public class DeleteMilestoneCommandValidator : AbstractValidator<DeleteMilestoneCommand>
{
    public DeleteMilestoneCommandValidator() => RuleFor(x => x.Id).GreaterThan(0UL);
}

public class DeleteMilestoneCommandHandler : IRequestHandler<DeleteMilestoneCommand, bool>
{
    private readonly IContractMilestoneRepository _milestoneRepository;
    public DeleteMilestoneCommandHandler(IContractMilestoneRepository milestoneRepository) =>
        _milestoneRepository = milestoneRepository;

    public async Task<bool> Handle(DeleteMilestoneCommand request, CancellationToken ct)
    {
        var ok = await _milestoneRepository.DeleteAsync(request.Id, ct);
        if (!ok) throw new NotFoundException("HD_MocTrienKhai", request.Id);
        return true;
    }
}
