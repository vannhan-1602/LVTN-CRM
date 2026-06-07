using CRM.Application.Common.Models;
using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Mappings;
using CRM.Application.Interfaces.Leads;
using MediatR;


namespace CRM.Application.Features.Leads.Queries.GetAllLeads
{
    public class GetAllLeadsQueryHandler : IRequestHandler<GetAllLeadsQuery, PagedResult<LeadDto>>
    {
        private readonly ILeadRepository _leadRepository;
        public GetAllLeadsQueryHandler(ILeadRepository leadRepository)
            => _leadRepository = leadRepository;

        public async Task<PagedResult<LeadDto>> Handle(GetAllLeadsQuery request, CancellationToken cancellationToken)
        {
            var result = await _leadRepository.GetPagedAsync(
                request.PageNumber, request.PageSize, request.Search, cancellationToken);

            return new PagedResult<LeadDto>
            {
                Items = result.Items.Select(LeadMapper.ToDto).ToList(),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }
    }
}
