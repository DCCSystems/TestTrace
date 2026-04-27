namespace TestTrace_V1.Domain;

public sealed class AuthorityStamp
{
    public Guid? UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public AuthorityRole? Role { get; init; }
    public AuthorityScopeType? ScopeType { get; init; }
    public Guid? ScopeId { get; init; }

    public static AuthorityStamp Create(
        UserAccount user,
        AuthorityRole? role,
        AuthorityScopeType? scopeType,
        Guid? scopeId)
    {
        return new AuthorityStamp
        {
            UserId = user.UserId,
            DisplayName = user.DisplayName,
            Role = role,
            ScopeType = scopeType,
            ScopeId = scopeId
        };
    }
}
