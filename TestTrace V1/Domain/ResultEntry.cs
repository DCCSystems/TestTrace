namespace TestTrace_V1.Domain;

public sealed class ResultEntry
{
    public Guid ResultEntryId { get; init; }
    public TestResult Result { get; init; }
    public string? MeasuredValue { get; init; }
    public string? Comments { get; init; }
    public List<CapturedTestInputValue> CapturedInputValues { get; init; } = [];
    public Guid? SupersedesResultEntryId { get; init; }
    public string? WitnessedBy { get; init; }
    public DateTimeOffset? WitnessedAt { get; init; }
    public AuthorityStamp? WitnessAuthority { get; init; }
    public string? OverrideReason { get; init; }
    public string ExecutedBy { get; init; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; init; }
    public AuthorityStamp? Authority { get; init; }

    public static ResultEntry Create(
        Guid resultEntryId,
        TestResult result,
        string? measuredValue,
        string? comments,
        IReadOnlyList<CapturedTestInputValue>? capturedInputValues,
        Guid? supersedesResultEntryId,
        string? witnessedBy,
        DateTimeOffset? witnessedAt,
        AuthorityStamp? witnessAuthority,
        string? overrideReason,
        string executedBy,
        DateTimeOffset executedAt,
        AuthorityStamp? authority = null)
    {
        return new ResultEntry
        {
            ResultEntryId = resultEntryId,
            Result = result,
            MeasuredValue = string.IsNullOrWhiteSpace(measuredValue) ? null : measuredValue,
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments,
            CapturedInputValues = NormalizeCapturedInputs(capturedInputValues),
            SupersedesResultEntryId = supersedesResultEntryId,
            WitnessedBy = string.IsNullOrWhiteSpace(witnessedBy) ? null : witnessedBy.Trim(),
            WitnessedAt = witnessedAt,
            WitnessAuthority = witnessAuthority,
            OverrideReason = string.IsNullOrWhiteSpace(overrideReason) ? null : overrideReason.Trim(),
            ExecutedBy = executedBy,
            ExecutedAt = executedAt,
            Authority = authority
        };
    }

    private static List<CapturedTestInputValue> NormalizeCapturedInputs(IReadOnlyList<CapturedTestInputValue>? capturedInputValues)
    {
        if (capturedInputValues is null || capturedInputValues.Count == 0)
        {
            return [];
        }

        var normalized = new List<CapturedTestInputValue>();
        foreach (var value in capturedInputValues)
        {
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                continue;
            }

            if (normalized.Any(existing => existing.TestInputId == value.TestInputId))
            {
                throw new InvalidOperationException("Captured test input values must be unique within a result entry.");
            }

            normalized.Add(CapturedTestInputValue.Create(value.TestInputId, value.Value));
        }

        return normalized;
    }
}
