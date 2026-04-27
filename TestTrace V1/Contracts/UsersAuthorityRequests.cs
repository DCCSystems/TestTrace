using TestTrace_V1.Domain;

namespace TestTrace_V1.Contracts;

public sealed class AddUserRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Organisation { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class UpdateUserRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Organisation { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class DeactivateUserRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
}

public sealed class ReactivateUserRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class AssignAuthorityRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public AuthorityRole Role { get; init; }
    public AuthorityScopeType ScopeType { get; init; }
    public Guid ScopeId { get; init; }
    public string? Reason { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class RevokeAuthorityRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid AssignmentId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
}
