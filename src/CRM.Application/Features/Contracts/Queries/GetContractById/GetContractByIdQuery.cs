using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Entities.Sales;
using MediatR;

namespace CRM.Application.Features.Contracts.Queries.GetContractById;

public record GetContractByIdQuery(ulong Id) : IRequest<ContractDto>;

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDto>
{
    private readonly IContractRepository _contractRepository;
    public GetContractByIdQueryHandler(IContractRepository contractRepository) => _contractRepository = contractRepository;

    public async Task<ContractDto> Handle(GetContractByIdQuery request, CancellationToken ct) =>
        await _contractRepository.GetByIdEnrichedAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(HopDong), request.Id);
}
