using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class Section
{
    public Guid SectionId { get; init; }
    [JsonInclude]
    public string Title { get; private set; } = string.Empty;
    [JsonInclude]
    public string? Description { get; private set; }
    [JsonInclude]
    public int DisplayOrder { get; private set; }
    [JsonInclude]
    public string? SectionApprover { get; private set; }
    [JsonInclude]
    public SectionApprovalRecord? Approval { get; private set; }
    [JsonInclude]
    public ApplicabilityState Applicability { get; private set; } = ApplicabilityState.Applicable;
    [JsonInclude]
    public string? ApplicabilityReason { get; private set; }
    public List<SectionAsset> Assets { get; init; } = [];
    [JsonInclude]
    public List<SectionComponent>? Components { get; private set; }
    public List<TestItem> TestItems { get; init; } = [];
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    public static Section Create(
        Guid sectionId,
        string title,
        string? description,
        int displayOrder,
        string? sectionApprover,
        string createdBy,
        DateTimeOffset createdAt)
    {
        return new Section
        {
            SectionId = sectionId,
            Title = title,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            DisplayOrder = displayOrder,
            SectionApprover = string.IsNullOrWhiteSpace(sectionApprover) ? null : sectionApprover,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }

    public void NormalizeLegacyAssetAssignments()
    {
        if (Assets.Count == 0 && Components is not null)
        {
            foreach (var component in Components.OrderBy(component => component.DisplayOrder))
            {
                Assets.Add(SectionAsset.FromLegacyComponent(component));
            }
        }

        Components = null;

        foreach (var testItem in TestItems)
        {
            testItem.NormalizeLegacyAssetId();
        }
    }

    public void AddTestItem(TestItem testItem)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot add test items to an approved section.");
        }

        TestItems.Add(testItem);
    }

    public SectionAsset AddAsset(Guid assetId, string actor, DateTimeOffset at)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot add assets to an approved section.");
        }

        if (Assets.Any(asset => asset.AssetId == assetId))
        {
            throw new InvalidOperationException("Asset is already assigned to this section.");
        }

        var asset = SectionAsset.Create(assetId, Assets.Count + 1, actor, at);
        Assets.Add(asset);
        return asset;
    }

    public SectionApprovalRecord Approve(
        string approvedBy,
        string releaseAuthority,
        DateTimeOffset approvedAt,
        string? comments,
        Func<Guid, IReadOnlySet<Guid>> assetScope,
        Func<Section, TestItem, bool> testIsApplicable,
        AuthorityStamp? authority = null)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Section is already approved.");
        }

        if (Applicability == ApplicabilityState.Applicable && TestItems.Count == 0)
        {
            throw new InvalidOperationException("Section must contain at least one test item before approval.");
        }

        foreach (var sectionAsset in Assets.Where(asset => asset.Applicability == ApplicabilityState.Applicable))
        {
            var scopedAssetIds = assetScope(sectionAsset.AssetId);
            if (TestItems.All(testItem =>
                    testItem.AssetId is null ||
                    !scopedAssetIds.Contains(testItem.AssetId.Value) ||
                    !testIsApplicable(this, testItem)))
            {
                throw new InvalidOperationException("Every assigned asset must contain at least one test item before approval.");
            }
        }

        if (TestItems.Any(t => testIsApplicable(this, t) && t.LatestResult == TestResult.Fail))
        {
            throw new InvalidOperationException("Section cannot be approved while a latest test result is Fail.");
        }

        if (TestItems.Any(t => testIsApplicable(this, t) && t.LatestResult == TestResult.NotTested))
        {
            throw new InvalidOperationException("Section cannot be approved while a test item is NotTested.");
        }

        var declaredApprover = string.IsNullOrWhiteSpace(SectionApprover)
            ? releaseAuthority
            : SectionApprover;

        if (!AuthorityMatches(approvedBy, declaredApprover) && !AuthorityMatches(approvedBy, releaseAuthority))
        {
            throw new InvalidOperationException("Approver must match the section approver or project release authority.");
        }

        Approval = SectionApprovalRecord.Create(
            Guid.NewGuid(),
            SectionId,
            approvedBy,
            approvedAt,
            comments,
            authority);

        return Approval;
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Section title is required.");
        }

        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot rename an approved section.");
        }

        Title = title.Trim();
    }

    public void SetApplicability(ApplicabilityState applicability, string? reason)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot change applicability after section approval.");
        }

        if (applicability == ApplicabilityState.NotApplicable && string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Not applicable sections require a reason.");
        }

        if (applicability == ApplicabilityState.NotApplicable &&
            TestItems.Any(testItem => testItem.ResultHistory.Count > 0 || testItem.EvidenceRecords.Count > 0))
        {
            throw new InvalidOperationException("Sections with results or evidence cannot be marked not applicable.");
        }

        Applicability = applicability;
        ApplicabilityReason = applicability == ApplicabilityState.NotApplicable
            ? reason?.Trim()
            : null;
    }

    public void SetAssetApplicability(
        Guid assetId,
        ApplicabilityState applicability,
        string? reason,
        IReadOnlySet<Guid> scopedAssetIds)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot change applicability after section approval.");
        }

        var asset = Assets.SingleOrDefault(asset => asset.AssetId == assetId)
            ?? throw new InvalidOperationException("Asset assignment was not found.");

        if (applicability == ApplicabilityState.NotApplicable &&
            TestItems.Any(testItem =>
                testItem.AssetId is not null &&
                scopedAssetIds.Contains(testItem.AssetId.Value) &&
                (testItem.ResultHistory.Count > 0 || testItem.EvidenceRecords.Count > 0)))
        {
            throw new InvalidOperationException("Assets with results or evidence cannot be marked not applicable.");
        }

        asset.SetApplicability(applicability, reason);
    }

    public void SetTestApplicability(
        Guid testItemId,
        ApplicabilityState applicability,
        string? reason)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot change applicability after section approval.");
        }

        var testItem = TestItems.SingleOrDefault(testItem => testItem.TestItemId == testItemId)
            ?? throw new InvalidOperationException("Test item was not found.");

        testItem.SetApplicability(applicability, reason);
    }

    public void RemoveAsset(Guid assetId, IReadOnlySet<Guid> scopedAssetIds)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot remove assets from an approved section.");
        }

        if (TestItems.Any(testItem => testItem.AssetId is not null && scopedAssetIds.Contains(testItem.AssetId.Value)))
        {
            throw new InvalidOperationException("Delete test items before removing an asset assignment.");
        }

        var asset = Assets.SingleOrDefault(asset => asset.AssetId == assetId)
            ?? throw new InvalidOperationException("Asset assignment was not found.");

        Assets.Remove(asset);
        ReindexAssets();
    }

    public void DeleteTestItem(Guid testItemId)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot delete test items from an approved section.");
        }

        var testItem = TestItems.SingleOrDefault(testItem => testItem.TestItemId == testItemId)
            ?? throw new InvalidOperationException("Test item was not found.");

        if (testItem.ResultHistory.Count > 0 || testItem.EvidenceRecords.Count > 0)
        {
            throw new InvalidOperationException("Test items with results or evidence cannot be deleted.");
        }

        TestItems.Remove(testItem);
        ReindexTestItems(testItem.AssetId);
    }

    public void DeleteTestItemsForAssets(IReadOnlySet<Guid> assetIds)
    {
        if (Approval is not null)
        {
            throw new InvalidOperationException("Cannot delete test items from an approved section.");
        }

        if (TestItems.Any(testItem =>
                testItem.AssetId is not null &&
                assetIds.Contains(testItem.AssetId.Value) &&
                (testItem.ResultHistory.Count > 0 || testItem.EvidenceRecords.Count > 0)))
        {
            throw new InvalidOperationException("Test items with results or evidence cannot be deleted.");
        }

        TestItems.RemoveAll(testItem => testItem.AssetId is not null && assetIds.Contains(testItem.AssetId.Value));
        ReindexAllTestItems();
    }

    public void RemoveAssetAssignments(IReadOnlySet<Guid> assetIds)
    {
        Assets.RemoveAll(asset => assetIds.Contains(asset.AssetId));
        ReindexAssets();
    }

    public void SetDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    private void ReindexAssets()
    {
        var order = 1;
        foreach (var asset in Assets.OrderBy(asset => asset.DisplayOrder))
        {
            asset.SetDisplayOrder(order++);
        }
    }

    private void ReindexAllTestItems()
    {
        foreach (var assetId in TestItems.Select(testItem => testItem.AssetId).Distinct())
        {
            ReindexTestItems(assetId);
        }
    }

    private void ReindexTestItems(Guid? assetId)
    {
        var order = 1;
        foreach (var testItem in TestItems
                     .Where(testItem => testItem.AssetId == assetId)
                     .OrderBy(testItem => testItem.DisplayOrder))
        {
            testItem.SetDisplayOrder(order++);
        }
    }

    private static bool AuthorityMatches(string actual, string expected)
    {
        return string.Equals(actual.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
