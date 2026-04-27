using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class Component
{
    public Guid ComponentId { get; init; }
    [JsonInclude]
    public string Name { get; private set; } = string.Empty;
    [JsonInclude]
    public string? Description { get; private set; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    public static Component Create(
        Guid componentId,
        string name,
        string? description,
        string createdBy,
        DateTimeOffset createdAt)
    {
        return new Component
        {
            ComponentId = componentId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Component name is required.");
        }

        Name = name.Trim();
    }
}
