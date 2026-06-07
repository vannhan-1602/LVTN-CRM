using CRM.Application.Common.Models;
using CRM.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Interfaces.Leads
{
    public interface ILeadRepository
    {
        Task<Lead?> GetByIdAsync(ulong id, CancellationToken cancellationToken = default);
        Task<PagedResult<Lead>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default);
        Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);
        Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(ulong id, CancellationToken cancellationToken = default);
    }
}
