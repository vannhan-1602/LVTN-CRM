using CRM.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Contexts;


public partial class CrmDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplySoftDeleteQueryFilters();
    }
}
