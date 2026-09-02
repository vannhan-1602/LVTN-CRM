using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Contexts;

// Tách riêng khỏi CrmDbContext.cs (file scaffold DB-First) để lần scaffold lại sau này
// không bị mất DbSet thêm tay này.
public partial class CrmDbContext
{
    public virtual DbSet<HtRefreshTokenEntity> HtRefreshTokens { get; set; } = null!;
}
