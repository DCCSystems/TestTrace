namespace TestTrace_V1.Workspace;

public sealed class ReadinessReport
{
    public bool IsReleased { get; init; }
    public bool CanAddSection { get; init; }
    public bool CanAddAsset { get; init; }
    public bool CanAddSubAsset { get; init; }
    public bool CanAddComponent => CanAddAsset;
    public bool CanAddTestItem { get; init; }
    public bool CanRenameSelection { get; init; }
    public bool CanDeleteSelection { get; init; }
    public bool CanOpenForExecution { get; init; }
    public bool CanRecordResults { get; init; }
    public bool CanAttachEvidence { get; init; }
    public bool CanApproveSection { get; init; }
    public bool CanRelease { get; init; }
    public IReadOnlyList<ReadinessIssue> Issues { get; init; } = [];

    public IEnumerable<string> MessagesFor(string area)
    {
        return Issues
            .Where(issue => string.Equals(issue.Area, area, StringComparison.OrdinalIgnoreCase))
            .Select(issue => issue.Message);
    }
}
