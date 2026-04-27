namespace TestTrace_V1;

public static class TestTraceAppEnvironment
{
    public const string ProjectsRootEnvironmentVariable = "TESTTRACE_PROJECTS_ROOT";

    public static bool IsSandbox => SandboxRoot is not null;

    public static string ModeLabel => IsSandbox ? "SANDBOX" : "LIVE";

    public static string? SandboxRoot => FindSandboxRoot(AppContext.BaseDirectory);

    public static string DefaultProjectsRoot()
    {
        var configuredRoot = Environment.GetEnvironmentVariable(ProjectsRootEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            return configuredRoot.Trim();
        }

        var sandboxRoot = SandboxRoot;
        if (!string.IsNullOrWhiteSpace(sandboxRoot))
        {
            return Path.Combine(sandboxRoot, "sandbox-data", "TestTraceProjects");
        }

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documents, "TestTraceProjects");
    }

    private static string? FindSandboxRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        while (current is not null)
        {
            if (IsKnownSandboxRoot(current))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool IsKnownSandboxRoot(DirectoryInfo candidate)
    {
        if (!candidate.Exists)
        {
            return false;
        }

        if (!candidate.Name.Equals("DCC Systems CODEX", StringComparison.OrdinalIgnoreCase) &&
            !candidate.Name.Equals("DCC Systems Backup 17.04.26", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Directory.Exists(Path.Combine(candidate.FullName, "projects")) &&
            Directory.Exists(Path.Combine(candidate.FullName, "reference")) &&
            Directory.Exists(Path.Combine(candidate.FullName, "workflows"));
    }
}
