using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class AuthorityAssignment
{
    public Guid AssignmentId { get; init; }
    public Guid UserId { get; init; }
    public string DisplayNameSnapshot { get; init; } = string.Empty;
    public AuthorityRole Role { get; init; }
    public AuthorityScopeType ScopeType { get; init; }
    public Guid ScopeId { get; init; }
    public string AssignedBy { get; init; } = string.Empty;
    public DateTimeOffset AssignedAt { get; init; }
    public string? Reason { get; init; }
    [JsonInclude]
    public DateTimeOffset? RevokedAt { get; private set; }
    [JsonInclude]
    public string? RevokedBy { get; private set; }
    [JsonInclude]
    public string? RevokedReason { get; private set; }

    public bool IsActive => RevokedAt is null;

    public static AuthorityAssignment Create(
        Guid assignmentId,
        UserAccount user,
        AuthorityRole role,
        AuthorityScopeType scopeType,
        Guid scopeId,
        string assignedBy,
        DateTimeOffset assignedAt,
        string? reason)
    {
        if (scopeId == Guid.Empty)
        {
            throw new InvalidOperationException("Authority assignment scope id is required.");
        }

        if (string.IsNullOrWhiteSpace(assignedBy))
        {
            throw new InvalidOperationException("Assigned by is required.");
        }

        return new AuthorityAssignment
        {
            AssignmentId = assignmentId,
            UserId = user.UserId,
            DisplayNameSnapshot = user.DisplayName,
            Role = role,
            ScopeType = scopeType,
            ScopeId = scopeId,
            AssignedBy = assignedBy.Trim(),
            AssignedAt = assignedAt,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
        };
    }

    public void Revoke(string revokedBy, string reason, DateTimeOffset at)
    {
        if (!IsActive)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(revokedBy))
        {
            throw new InvalidOperationException("Revoked by is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("A reason is required to revoke an authority assignment.");
        }

        RevokedAt = at;
        RevokedBy = revokedBy.Trim();
        RevokedReason = reason.Trim();
    }
}
