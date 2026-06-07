namespace CRM.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
