using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class SectionComponent
{
    public Guid ComponentId { get; init; }
    [JsonInclude]
    public int DisplayOrder { get; private set; }
    public string AddedBy { get; init; } = string.Empty;
    public DateTimeOffset AddedAt { get; init; }

    public static SectionComponent Create(
        Guid componentId,
        int displayOrder,
        string addedBy,
        DateTimeOffset addedAt)
    {
        return new SectionComponent
        {
            ComponentId = componentId,
            DisplayOrder = displayOrder,
            AddedBy = addedBy,
            AddedAt = addedAt
        };
    }

    public void SetDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }
}
