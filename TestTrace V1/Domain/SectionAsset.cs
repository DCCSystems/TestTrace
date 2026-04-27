using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class SectionAsset
{
    public Guid AssetId { get; init; }
    [JsonInclude]
    public int DisplayOrder { get; private set; }
    [JsonInclude]
    public ApplicabilityState Applicability { get; private set; } = ApplicabilityState.Applicable;
    [JsonInclude]
    public string? ApplicabilityReason { get; private set; }
    public string AddedBy { get; init; } = string.Empty;
    public DateTimeOffset AddedAt { get; init; }

    public static SectionAsset Create(
        Guid assetId,
        int displayOrder,
        string addedBy,
        DateTimeOffset addedAt)
    {
        return new SectionAsset
        {
            AssetId = assetId,
            DisplayOrder = displayOrder,
            AddedBy = addedBy,
            AddedAt = addedAt
        };
    }

    public static SectionAsset FromLegacyComponent(SectionComponent component)
    {
        return Create(
            component.ComponentId,
            component.DisplayOrder,
            component.AddedBy,
            component.AddedAt);
    }

    public void SetDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void SetApplicability(ApplicabilityState applicability, string? reason)
    {
        if (applicability == ApplicabilityState.NotApplicable && string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Not applicable assets require a reason.");
        }

        Applicability = applicability;
        ApplicabilityReason = applicability == ApplicabilityState.NotApplicable
            ? reason?.Trim()
            : null;
    }
}
