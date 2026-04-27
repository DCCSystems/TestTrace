namespace TestTrace_V1.Contracts;

public sealed class ApproveSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string ApprovedBy { get; init; } = string.Empty;
    public string? Comments { get; init; }
}

public sealed class ReleaseProjectRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public string ReleasedBy { get; init; } = string.Empty;
    public string Declaration { get; init; } = string.Empty;
}
