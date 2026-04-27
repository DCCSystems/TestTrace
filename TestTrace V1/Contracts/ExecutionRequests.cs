using TestTrace_V1.Domain;

namespace TestTrace_V1.Contracts;

public sealed class RecordResultRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid TestItemId { get; init; }
    public TestResult Result { get; init; }
    public string? MeasuredValue { get; init; }
    public string? Comments { get; init; }
    public IReadOnlyList<CapturedTestInputValue> CapturedInputValues { get; init; } = [];
    public Guid? SupersedesResultEntryId { get; init; }
    public string? WitnessedBy { get; init; }
    public string? OverrideReason { get; init; }
    public string ExecutedBy { get; init; } = string.Empty;
}

public sealed class AttachEvidenceRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid TestItemId { get; init; }
    public string SourceFilePath { get; init; } = string.Empty;
    public EvidenceType EvidenceType { get; init; } = EvidenceType.Other;
    public string? Description { get; init; }
    public string AttachedBy { get; init; } = string.Empty;
}
