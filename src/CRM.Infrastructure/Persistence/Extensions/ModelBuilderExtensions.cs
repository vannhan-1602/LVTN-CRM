using System.Linq.Expressions;
using CRM.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CRM.Infrastructure.Persistence.Extensions;

public static class ModelBuilderExtensions
{

    /// Các bảng có cột IsDeleted trong db BH_CoHoiBanHang, KH_KhachHang, TK_Ticket
  
    private static readonly HashSet<string> SoftDeleteTableNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "BH_CoHoiBanHang",
        "KH_KhachHang",
        "TK_Ticket"
    };

    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName is null || !SoftDeleteTableNames.Contains(tableName))
            {
                continue;
            }

            var isDeletedProperty = entityType.FindProperty("IsDeleted");
            if (isDeletedProperty is null || isDeletedProperty.ClrType != typeof(bool))
            {
                continue;
            }

            ApplySoftDeleteFilter(modelBuilder, entityType.ClrType);
        }
    }

    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder, Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        var lambda = Expression.Lambda(condition, parameter);

        modelBuilder.Entity(entityType).HasQueryFilter(lambda);
    }
}

public static class DbContextExtensions
{
    public static void SoftDelete<TEntity>(this DbContext context, TEntity entity)
        where TEntity : class, ISoftDeletable
    {
        entity.IsDeleted = true;
        context.Entry(entity).State = EntityState.Modified;
    }
}
