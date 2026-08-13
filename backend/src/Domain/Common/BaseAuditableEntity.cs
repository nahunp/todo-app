namespace TodoApp.Domain.Common;

/// <summary>
/// An entity that tracks who created/modified it and when.
/// Split out from BaseEntity because not every entity needs auditing,
/// and forcing it on everything is a YAGNI violation waiting to happen.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
