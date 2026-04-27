using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class UserAccount
{
    public Guid UserId { get; init; }
    [JsonInclude]
    public string DisplayName { get; private set; } = string.Empty;
    [JsonInclude]
    public string Initials { get; private set; } = string.Empty;
    [JsonInclude]
    public string? Email { get; private set; }
    [JsonInclude]
    public string? Phone { get; private set; }
    [JsonInclude]
    public string? Organisation { get; private set; }
    [JsonInclude]
    public bool IsActive { get; private set; } = true;
    [JsonInclude]
    public string? DeactivatedReason { get; private set; }
    [JsonInclude]
    public DateTimeOffset? DeactivatedAt { get; private set; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    public static UserAccount Create(
        Guid userId,
        string displayName,
        string? email,
        string? phone,
        string? organisation,
        string createdBy,
        DateTimeOffset createdAt)
    {
        var normalizedName = NormalizeRequired(displayName, "User display name is required.");
        return new UserAccount
        {
            UserId = userId,
            DisplayName = normalizedName,
            Initials = BuildInitials(normalizedName),
            Email = NormalizeOptional(email),
            Phone = NormalizeOptional(phone),
            Organisation = NormalizeOptional(organisation),
            IsActive = true,
            CreatedBy = NormalizeRequired(createdBy, "Created by is required."),
            CreatedAt = createdAt
        };
    }

    public void UpdateProfile(
        string displayName,
        string? email,
        string? phone,
        string? organisation)
    {
        var normalized = NormalizeRequired(displayName, "User display name is required.");
        DisplayName = normalized;
        Initials = BuildInitials(normalized);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        Organisation = NormalizeOptional(organisation);
    }

    public void Deactivate(string reason, DateTimeOffset at)
    {
        if (!IsActive)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("A reason is required to deactivate a user.");
        }

        IsActive = false;
        DeactivatedReason = reason.Trim();
        DeactivatedAt = at;
    }

    public void Reactivate(DateTimeOffset at)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        DeactivatedReason = null;
        DeactivatedAt = null;
        _ = at;
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

    private static string NormalizeRequired(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(message);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
