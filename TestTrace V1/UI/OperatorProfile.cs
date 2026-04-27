namespace TestTrace_V1.UI;

public sealed class OperatorProfile
{
    public Guid OperatorId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Initials { get; init; } = string.Empty;
    public string? JobRole { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Organisation { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastActiveAt { get; init; }

    public static OperatorProfile Create(
        string displayName,
        string? jobRole,
        string? email,
        string? phone,
        string? organisation,
        DateTimeOffset at)
    {
        var name = (displayName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Display name is required.");
        }

        return new OperatorProfile
        {
            OperatorId = Guid.NewGuid(),
            DisplayName = name,
            Initials = BuildInitials(name),
            JobRole = Trim(jobRole),
            Email = Trim(email),
            Phone = Trim(phone),
            Organisation = Trim(organisation),
            CreatedAt = at,
            LastActiveAt = at
        };
    }

    public OperatorProfile WithProfile(string displayName, string? jobRole, string? email, string? phone, string? organisation)
    {
        var name = (displayName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Display name is required.");
        }

        return new OperatorProfile
        {
            OperatorId = OperatorId,
            DisplayName = name,
            Initials = BuildInitials(name),
            JobRole = Trim(jobRole),
            Email = Trim(email),
            Phone = Trim(phone),
            Organisation = Trim(organisation),
            CreatedAt = CreatedAt,
            LastActiveAt = LastActiveAt
        };
    }

    public OperatorProfile WithLastActive(DateTimeOffset at)
    {
        return new OperatorProfile
        {
            OperatorId = OperatorId,
            DisplayName = DisplayName,
            Initials = Initials,
            JobRole = JobRole,
            Email = Email,
            Phone = Phone,
            Organisation = Organisation,
            CreatedAt = CreatedAt,
            LastActiveAt = at
        };
    }

    private static string BuildInitials(string displayName)
    {
        var parts = displayName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part => part.Length > 0)
            .ToArray();
        if (parts.Length == 0)
        {
            return "?";
        }
        var initials = parts.Length == 1
            ? parts[0][..Math.Min(parts[0].Length, 2)]
            : string.Concat(parts.Take(3).Select(part => part[0]));
        return initials.ToUpperInvariant();
    }

    private static string? Trim(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
