using CRM.Application.Common.Exceptions;
using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Mappings;
using CRM.Application.Interfaces.Leads;
using CRM.Domain.Entities.Customers;
using MediatR;


namespace CRM.Application.Features.Leads.Queries.GetLeadById
{
    public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto>
    {
        private readonly ILeadRepository _leadRepository;
        public GetLeadByIdQueryHandler(ILeadRepository leadRepository)
            => _leadRepository = leadRepository;

        public async Task<LeadDto> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
        {
            var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Lead), request.Id);
            return LeadMapper.ToDto(lead);
        }
    }
}
