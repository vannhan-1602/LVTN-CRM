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
            // Sale mặc định chỉ xem Lead mình phụ trách, TRỪ khi:
            //  - request.ChuaGan = true → xem hàng chờ Lead chưa gán (để tự nhận), mọi Sale đều thấy như nhau.
            // Manager xem toàn đội (không lọc theo owner).
            var chuaGan = request.ChuaGan;
            uint? ownerUserId;

            if (_currentUser.Role == Roles.Sale)
            {
                ownerUserId = chuaGan == true ? null : _currentUser.UserId;
            }
            else
            {
                ownerUserId = null;
            }

            var result = await _leadRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.Search,
                ownerUserId,
                request.IsDeleted,
                request.TinhTrang,
                chuaGan,
                cancellationToken);

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