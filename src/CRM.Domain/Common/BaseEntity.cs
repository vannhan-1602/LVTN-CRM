namespace CRM.Domain.Common;

public abstract class BaseEntity<TKey> where TKey : struct
{
    public TKey Id { get; set; }
}

public abstract class AuditableEntity<TKey> : BaseEntity<TKey> where TKey : struct
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class SoftDeletableEntity<TKey> : AuditableEntity<TKey> where TKey : struct
{
    public bool IsDeleted { get; set; }
}
