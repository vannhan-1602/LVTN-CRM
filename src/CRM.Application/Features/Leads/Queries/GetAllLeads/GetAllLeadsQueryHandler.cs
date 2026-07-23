using CRM.Application.Common.Constants;
using CRM.Application.Common.Models;
using CRM.Application.Features.Leads.DTOs;
using CRM.Application.Features.Leads.Mappings;
using CRM.Application.Interfaces.Common;
using CRM.Application.Interfaces.Leads;
using MediatR;

namespace CRM.Application.Features.Leads.Queries.GetAllLeads
{
    public class GetAllLeadsQueryHandler : IRequestHandler<GetAllLeadsQuery, PagedResult<LeadDto>>
    {
        private readonly ILeadRepository _leadRepository;
        private readonly ICurrentUserService _currentUser;

        public GetAllLeadsQueryHandler(ILeadRepository leadRepository, ICurrentUserService currentUser)
        {
            _leadRepository = leadRepository;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<LeadDto>> Handle(GetAllLeadsQuery request, CancellationToken cancellationToken)
        {
            // Sale chỉ xem Lead mình phụ trách  Manager xem toàn đội 
            uint? ownerUserId = _currentUser.Role == Roles.Sale ? _currentUser.UserId : null;

            var result = await _leadRepository.GetPagedAsync(
                request.PageNumber, request.PageSize, request.Search, ownerUserId, request.IsDeleted, request.TinhTrang, cancellationToken);

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