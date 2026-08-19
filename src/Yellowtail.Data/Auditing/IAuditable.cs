namespace Yellowtail.Data.Auditing;

/// <summary>
/// Marks an entity as carrying automatic creation/modification timestamps, stamped by
/// <see cref="YellowtailDbContext"/> on save. CreatedBy/ModifiedBy are deliberately not
/// included yet: this POC has no authentication, so there is no genuine identity to
/// populate them with. Add them here once real auth exists.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// The UTC date and time the entity was created. Stamped automatically on insert;
    /// never overwritten after that.
    /// </summary>
    DateTime CreatedOn { get; set; }

    /// <summary>
    /// The UTC date and time the entity was last modified, or <see langword="null"/> if it
    /// has never been updated since creation. Stamped automatically on every update.
    /// </summary>
    DateTime? ModifiedOn { get; set; }
}
