using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class Asset
{
    public Guid AssetId { get; init; }
    [JsonInclude]
    public string Name { get; private set; } = string.Empty;
    [JsonInclude]
    public string Type { get; private set; } = "Component";
    [JsonInclude]
    public Guid? ParentAssetId { get; private set; }
    [JsonInclude]
    public string? Manufacturer { get; private set; }
    [JsonInclude]
    public string? Model { get; private set; }
    [JsonInclude]
    public string? SerialNumber { get; private set; }
    [JsonInclude]
    public string? Notes { get; private set; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    [JsonIgnore]
    public bool IsSubAsset => ParentAssetId is not null;

    public static Asset Create(
        Guid assetId,
        string name,
        string? type,
        Guid? parentAssetId,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        string createdBy,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Asset name is required.");
        }

        return new Asset
        {
            AssetId = assetId,
            Name = name.Trim(),
            Type = string.IsNullOrWhiteSpace(type) ? "Component" : type.Trim(),
            ParentAssetId = parentAssetId,
            Manufacturer = TrimToNull(manufacturer),
            Model = TrimToNull(model),
            SerialNumber = TrimToNull(serialNumber),
            Notes = TrimToNull(notes),
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }

    public static Asset FromLegacyComponent(Component component)
    {
        return Create(
            component.ComponentId,
            component.Name,
            "Component",
            null,
            null,
            null,
            null,
            component.Description,
            component.CreatedBy,
            component.CreatedAt);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Asset name is required.");
        }

        Name = name.Trim();
    }

    public void UpdateMetadata(
        string? type,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes)
    {
        Type = string.IsNullOrWhiteSpace(type) ? "Component" : type.Trim();
        Manufacturer = TrimToNull(manufacturer);
        Model = TrimToNull(model);
        SerialNumber = TrimToNull(serialNumber);
        Notes = TrimToNull(notes);
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
