using TestTrace_V1.Domain;

namespace TestTrace_V1.Contracts;

public sealed class SaveResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }

    public static SaveResult Success() => new() { Succeeded = true };
    public static SaveResult Failure(string message) => new() { Succeeded = false, ErrorMessage = message };
}

public sealed class BuildContractResult
{
    public bool Succeeded => Validation.IsValid && SaveSucceeded;
    public bool SaveSucceeded { get; init; }
    public ValidationResult Validation { get; init; } = ValidationResult.Success();
    public string? ErrorMessage { get; init; }
    public TestTraceProject? Project { get; init; }
    public string? ProjectFolderPath { get; init; }
    public string? ProjectFilePath { get; init; }

    public static BuildContractResult Invalid(ValidationResult validation) => new()
    {
        SaveSucceeded = false,
        Validation = validation,
        ErrorMessage = "Create Project validation failed."
    };

    public static BuildContractResult PersistenceFailure(ValidationResult validation, string message) => new()
    {
        SaveSucceeded = false,
        Validation = validation,
        ErrorMessage = message
    };

    public static BuildContractResult Success(TestTraceProject project, string folderPath, string filePath) => new()
    {
        SaveSucceeded = true,
        Validation = ValidationResult.Success(),
        Project = project,
        ProjectFolderPath = folderPath,
        ProjectFilePath = filePath
    };
}

public sealed class OperationResult
{
    public bool Succeeded => Validation.IsValid && SaveSucceeded;
    public bool SaveSucceeded { get; init; }
    public ValidationResult Validation { get; init; } = ValidationResult.Success();
    public string? ErrorMessage { get; init; }
    public TestTraceProject? Project { get; init; }
    public Guid? TargetId { get; init; }
    public string? ProjectFilePath { get; init; }

    public static OperationResult Invalid(ValidationResult validation) => new()
    {
        SaveSucceeded = false,
        Validation = validation,
        ErrorMessage = "Operation validation failed."
    };

    public static OperationResult GuardFailure(string code, string message, string? targetField = null) => Invalid(
        ValidationResult.FromIssues([
            new ValidationIssue
            {
                Code = code,
                Message = message,
                TargetField = targetField,
                Severity = Severity.Error
            }
        ]));

    public static OperationResult PersistenceFailure(ValidationResult validation, string message) => new()
    {
        SaveSucceeded = false,
        Validation = validation,
        ErrorMessage = message
    };

    public static OperationResult Success(TestTraceProject project, Guid? targetId, string projectFilePath) => new()
    {
        SaveSucceeded = true,
        Validation = ValidationResult.Success(),
        Project = project,
        TargetId = targetId,
        ProjectFilePath = projectFilePath
    };
}
