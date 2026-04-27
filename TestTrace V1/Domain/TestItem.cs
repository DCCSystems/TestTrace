using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class TestItem
{
    public Guid TestItemId { get; init; }
    [JsonInclude]
    public string TestReference { get; private set; } = string.Empty;
    [JsonInclude]
    public string TestTitle { get; private set; } = string.Empty;
    [JsonInclude]
    public string? TestDescription { get; private set; }
    [JsonInclude]
    public string? ExpectedOutcome { get; private set; }
    [JsonInclude]
    public AcceptanceCriteria AcceptanceCriteria { get; private set; } = TestTrace_V1.Domain.AcceptanceCriteria.ManualConfirmation();
    [JsonInclude]
    public EvidenceRequirements EvidenceRequirements { get; private set; } = TestTrace_V1.Domain.EvidenceRequirements.None();
    [JsonInclude]
    public TestBehaviourRules BehaviourRules { get; private set; } = TestTrace_V1.Domain.TestBehaviourRules.Default();
    [JsonInclude]
    public int DisplayOrder { get; private set; }
    [JsonInclude]
    public Guid? AssetId { get; private set; }
    [JsonInclude]
    public Guid? ComponentId { get; private set; }
    [JsonInclude]
    public ApplicabilityState Applicability { get; private set; } = ApplicabilityState.Applicable;
    [JsonInclude]
    public string? ApplicabilityReason { get; private set; }
    public List<TestInput> Inputs { get; init; } = [];
    public List<ResultEntry> ResultHistory { get; init; } = [];
    public List<EvidenceRecord> EvidenceRecords { get; init; } = [];
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }

    public TestResult LatestResult => ResultHistory.Count == 0
        ? TestResult.NotTested
        : ResultHistory[^1].Result;

    public static TestItem Create(
        Guid testItemId,
        string testReference,
        string testTitle,
        string? testDescription,
        string? expectedOutcome,
        AcceptanceCriteria? acceptanceCriteria,
        EvidenceRequirements? evidenceRequirements,
        TestBehaviourRules? behaviourRules,
        IReadOnlyList<TestInput>? inputs,
        int displayOrder,
        Guid? assetId,
        string createdBy,
        DateTimeOffset createdAt)
    {
        return new TestItem
        {
            TestItemId = testItemId,
            TestReference = testReference,
            TestTitle = testTitle,
            TestDescription = string.IsNullOrWhiteSpace(testDescription) ? null : testDescription,
            ExpectedOutcome = string.IsNullOrWhiteSpace(expectedOutcome) ? null : expectedOutcome,
            AcceptanceCriteria = acceptanceCriteria ?? TestTrace_V1.Domain.AcceptanceCriteria.ManualConfirmation(),
            EvidenceRequirements = evidenceRequirements ?? TestTrace_V1.Domain.EvidenceRequirements.None(),
            BehaviourRules = behaviourRules ?? TestTrace_V1.Domain.TestBehaviourRules.Default(),
            Inputs = NormalizeInputs(inputs),
            DisplayOrder = displayOrder,
            AssetId = assetId,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }

    public ResultEntry RecordResult(
        TestResult result,
        string? measuredValue,
        string? comments,
        IReadOnlyList<CapturedTestInputValue>? capturedInputValues,
        Guid? supersedesResultEntryId,
        string executedBy,
        DateTimeOffset executedAt,
        AuthorityStamp? authority = null)
    {
        if (result == TestResult.NotTested)
        {
            throw new InvalidOperationException("NotTested is derived from empty result history and cannot be recorded as an execution result.");
        }

        if (result == TestResult.NotApplicable)
        {
            throw new InvalidOperationException("Use applicability instead.");
        }

        if (EvidenceRequirements.CommentAlwaysRequired && string.IsNullOrWhiteSpace(comments))
        {
            throw new InvalidOperationException("This test requires comments for every result.");
        }

        if (result == TestResult.Fail &&
            EvidenceRequirements.CommentRequiredOnFail &&
            string.IsNullOrWhiteSpace(comments))
        {
            throw new InvalidOperationException("This test requires comments when the result is Fail.");
        }

        if (supersedesResultEntryId is not null && ResultHistory.All(r => r.ResultEntryId != supersedesResultEntryId.Value))
        {
            throw new InvalidOperationException("Superseded result entry was not found on this test item.");
        }

        ValidateCapturedInputs(capturedInputValues);

        var entry = ResultEntry.Create(
            Guid.NewGuid(),
            result,
            measuredValue,
            comments,
            capturedInputValues,
            supersedesResultEntryId,
            executedBy,
            executedAt,
            authority);

        ResultHistory.Add(entry);
        return entry;
    }

    private void ValidateCapturedInputs(IReadOnlyList<CapturedTestInputValue>? capturedInputValues)
    {
        var captured = capturedInputValues ?? [];
        foreach (var value in captured)
        {
            if (Inputs.All(input => input.TestInputId != value.TestInputId))
            {
                throw new InvalidOperationException("Captured input value does not match a declared test input.");
            }
        }

        foreach (var input in Inputs.Where(input => input.Required))
        {
            if (captured.All(value =>
                    value.TestInputId != input.TestInputId ||
                    string.IsNullOrWhiteSpace(value.Value)))
            {
                throw new InvalidOperationException($"Required input is missing: {input.Label}");
            }
        }
    }

    public void AttachEvidence(EvidenceRecord evidenceRecord)
    {
        if (evidenceRecord.TestItemId != TestItemId)
        {
            throw new InvalidOperationException("Evidence record is not bound to this test item.");
        }

        EvidenceRecords.Add(evidenceRecord);
    }

    public void SetApplicability(ApplicabilityState applicability, string? reason)
    {
        if (applicability == ApplicabilityState.NotApplicable && string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Not applicable tests require a reason.");
        }

        if (applicability == ApplicabilityState.NotApplicable && (ResultHistory.Count > 0 || EvidenceRecords.Count > 0))
        {
            throw new InvalidOperationException("Tests with results or evidence cannot be marked not applicable.");
        }

        Applicability = applicability;
        ApplicabilityReason = applicability == ApplicabilityState.NotApplicable
            ? reason?.Trim()
            : null;
    }

    public void Rename(string testReference, string testTitle)
    {
        if (string.IsNullOrWhiteSpace(testReference))
        {
            throw new InvalidOperationException("Test reference is required.");
        }

        if (string.IsNullOrWhiteSpace(testTitle))
        {
            throw new InvalidOperationException("Test title is required.");
        }

        TestReference = testReference.Trim();
        TestTitle = testTitle.Trim();
    }

    public void NormalizeLegacyAssetId()
    {
        if (AssetId is null && ComponentId is not null)
        {
            AssetId = ComponentId;
        }

        ComponentId = null;
    }

    public void SetDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    private static List<TestInput> NormalizeInputs(IReadOnlyList<TestInput>? inputs)
    {
        if (inputs is null || inputs.Count == 0)
        {
            return [];
        }

        var normalized = new List<TestInput>();
        foreach (var input in inputs.OrderBy(input => input.DisplayOrder))
        {
            if (normalized.Any(existing =>
                    string.Equals(existing.Label, input.Label, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Test input labels must be unique within a test item.");
            }

            normalized.Add(TestInput.Create(
                input.TestInputId == Guid.Empty ? Guid.NewGuid() : input.TestInputId,
                input.Label,
                input.InputType,
                input.TargetValue,
                input.Tolerance,
                input.Unit,
                input.Required,
                normalized.Count + 1));
        }

        return normalized;
    }
}
