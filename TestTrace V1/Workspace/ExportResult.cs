namespace TestTrace_V1.Workspace;

public sealed class ExportResult
{
    public bool Succeeded { get; init; }
    public string? FilePath { get; init; }
    public string? ErrorMessage { get; init; }

    public static ExportResult Success(string filePath)
    {
        return new ExportResult
        {
            Succeeded = true,
            FilePath = filePath
        };
    }

    public static ExportResult Failure(string message)
    {
        return new ExportResult
        {
            Succeeded = false,
            ErrorMessage = message
        };
    }
}
