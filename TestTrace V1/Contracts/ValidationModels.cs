namespace TestTrace_V1.Contracts;

public sealed class ValidationResult
{
    public bool IsValid => Issues.Count == 0;
    public List<ValidationIssue> Issues { get; init; } = [];

    public static ValidationResult Success() => new();

    public static ValidationResult FromIssues(IEnumerable<ValidationIssue> issues) => new()
    {
        Issues = issues.ToList()
    };
}

public sealed class ValidationIssue
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? TargetField { get; init; }
    public Severity Severity { get; init; } = Severity.Error;
}

public enum Severity
{
    Info,
    Warning,
    Error
}
