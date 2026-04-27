namespace TestTrace_V1.Persistence;

public sealed class ProjectLocation
{
    public string ProjectFolder { get; init; } = string.Empty;
    public string ProjectFile { get; init; } = string.Empty;
    public string EvidenceFolder { get; init; } = string.Empty;
    public string ExportsFolder { get; init; } = string.Empty;

    public static ProjectLocation FromProjectFolder(string projectFolder)
    {
        return new ProjectLocation
        {
            ProjectFolder = projectFolder,
            ProjectFile = Path.Combine(projectFolder, "project.testtrace.json"),
            EvidenceFolder = Path.Combine(projectFolder, "evidence"),
            ExportsFolder = Path.Combine(projectFolder, "exports")
        };
    }
}
