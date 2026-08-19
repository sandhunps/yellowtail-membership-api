namespace Yellowtail.Data.Enums;

/// <summary>
/// The role a member holds within the club. Stored for future use; not currently
/// enforced, since this POC has no authentication/authorization.
/// </summary>
public enum MemberRole
{
    /// <summary>
    /// A standard member with no special privileges.
    /// </summary>
    Member = 0,

    /// <summary>
    /// A member who also coaches one or more sports.
    /// </summary>
    Coach = 1,

    /// <summary>
    /// A member with administrative privileges.
    /// </summary>
    Admin = 2
}
