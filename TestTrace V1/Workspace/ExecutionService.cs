using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;

namespace TestTrace_V1.Workspace;

public sealed class ExecutionService
{
    private readonly IProjectRepository repository;
    private readonly Func<DateTimeOffset> clock;

    public ExecutionService(IProjectRepository repository, Func<DateTimeOffset>? clock = null)
    {
        this.repository = repository;
        this.clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public OperationResult RecordResult(RecordResultRequest request)
    {
        var validation = ValidateRecordResult(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.RecordResult(
                request.TestItemId,
                request.Result,
                TrimToNull(request.MeasuredValue),
                TrimToNull(request.Comments),
                request.CapturedInputValues,
                request.SupersedesResultEntryId,
                request.ExecutedBy.Trim(),
                clock()).ResultEntryId);
    }

    public OperationResult AttachEvidence(AttachEvidenceRequest request)
    {
        var validation = ValidateAttachEvidence(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        var location = ProjectLocation.FromProjectFolder(request.ProjectFolderPath.Trim());
        TestTraceProject project;
        try
        {
            project = repository.Load(location);
        }
        catch (Exception ex)
        {
            return OperationResult.GuardFailure("ProjectLoadFailed", ex.Message, nameof(request.ProjectFolderPath));
        }

        var sourceFile = new FileInfo(request.SourceFilePath.Trim());
        var evidenceId = Guid.NewGuid();
        var storedFileName = CreateStoredEvidenceFileName(project, evidenceId, sourceFile.Name);
        var tempPath = Path.Combine(location.EvidenceFolder, storedFileName + ".tmp");
        var finalPath = Path.Combine(location.EvidenceFolder, storedFileName);

        try
        {
            Directory.CreateDirectory(location.EvidenceFolder);
            File.Copy(sourceFile.FullName, tempPath, overwrite: false);
            var hash = ComputeSha256(tempPath);
            File.Move(tempPath, finalPath);

            var attachedAt = clock();
            var authority = project.RequireAuthorityStamp(
                request.AttachedBy.Trim(),
                AuthorityRole.TestExecutor,
                AuthorityScopeType.TestItem,
                request.TestItemId,
                attachedAt);

            var evidenceRecord = EvidenceRecord.Create(
                evidenceId,
                sourceFile.Name,
                storedFileName,
                sourceFile.Extension,
                hash,
                request.EvidenceType,
                request.Description,
                request.TestItemId,
                request.AttachedBy.Trim(),
                attachedAt,
                authority);

            project.AttachEvidence(evidenceRecord);

            var saveResult = repository.Save(project, location);
            if (!saveResult.Succeeded)
            {
                return OperationResult.PersistenceFailure(
                    ValidationResult.Success(),
                    saveResult.ErrorMessage ?? "Project persistence failed after evidence copy.");
            }

            return OperationResult.Success(project, evidenceId, location.ProjectFile);
        }
        catch (InvalidOperationException ex)
        {
            TryDelete(tempPath);
            TryDelete(finalPath);
            return OperationResult.GuardFailure("DomainGuardFailed", ex.Message);
        }
        catch (Exception ex)
        {
            TryDelete(tempPath);
            return OperationResult.PersistenceFailure(ValidationResult.Success(), ex.Message);
        }
    }

    private OperationResult MutateProject(string projectFolderPath, Func<TestTraceProject, Guid?> mutation)
    {
        var location = ProjectLocation.FromProjectFolder(projectFolderPath.Trim());

        TestTraceProject project;
        try
        {
            project = repository.Load(location);
        }
        catch (Exception ex)
        {
            return OperationResult.GuardFailure("ProjectLoadFailed", ex.Message, nameof(projectFolderPath));
        }

        Guid? targetId;
        try
        {
            targetId = mutation(project);
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult.GuardFailure("DomainGuardFailed", ex.Message);
        }

        var saveResult = repository.Save(project, location);
        if (!saveResult.Succeeded)
        {
            return OperationResult.PersistenceFailure(
                ValidationResult.Success(),
                saveResult.ErrorMessage ?? "Project persistence failed.");
        }

        return OperationResult.Success(project, targetId, location.ProjectFile);
    }

    private static ValidationResult ValidateRecordResult(RecordResultRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.ExecutedBy, nameof(request.ExecutedBy));
        if (request.TestItemId == Guid.Empty)
        {
            issues.Add(Error("Required", "Test item id is required.", nameof(request.TestItemId)));
        }

        if (request.Result == TestResult.NotTested)
        {
            issues.Add(Error("InvalidResult", "NotTested cannot be recorded as an execution result.", nameof(request.Result)));
        }

        if (request.Result == TestResult.NotApplicable)
        {
            issues.Add(Error("InvalidResult", "Use applicability instead.", nameof(request.Result)));
        }

        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateAttachEvidence(AttachEvidenceRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.AttachedBy, nameof(request.AttachedBy));
        if (request.TestItemId == Guid.Empty)
        {
            issues.Add(Error("Required", "Test item id is required.", nameof(request.TestItemId)));
        }

        if (!Enum.IsDefined(request.EvidenceType))
        {
            issues.Add(Error("InvalidEvidenceType", "Evidence type is invalid.", nameof(request.EvidenceType)));
        }

        if (string.IsNullOrWhiteSpace(request.SourceFilePath))
        {
            issues.Add(Error("Required", "Source evidence file path is required.", nameof(request.SourceFilePath)));
        }
        else if (!File.Exists(request.SourceFilePath.Trim()))
        {
            issues.Add(Error("SourceFileMissing", "Source evidence file was not found.", nameof(request.SourceFilePath)));
        }

        return ValidationResult.FromIssues(issues);
    }

    private static List<ValidationIssue> CommonProjectIssues(string projectFolderPath, string actor, string actorField)
    {
        var issues = new List<ValidationIssue>();
        Required(projectFolderPath, nameof(projectFolderPath), "Project folder path is required.", issues);
        Required(actor, actorField, "Actor is required.", issues);
        return issues;
    }

    private static void Required(string? value, string field, string message, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(Error("Required", message, field));
        }
    }

    private static ValidationIssue Error(string code, string message, string field)
    {
        return new ValidationIssue
        {
            Code = code,
            Message = message,
            TargetField = field,
            Severity = Severity.Error
        };
    }

    private static string CreateStoredEvidenceFileName(TestTraceProject project, Guid evidenceId, string originalFileName)
    {
        var nextNumber = project.Sections
            .SelectMany(s => s.TestItems)
            .SelectMany(t => t.EvidenceRecords)
            .Count() + 1;

        var safeOriginal = NormalizeFileName(originalFileName);
        return $"EV-{nextNumber:000000}_{evidenceId.ToString("N")[..8]}_{safeOriginal}";
    }

    private static string NormalizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitized = new string(fileName.Trim().Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        sanitized = Regex.Replace(sanitized, @"\s+", "-");
        sanitized = Regex.Replace(sanitized, @"-+", "-");
        sanitized = sanitized.Trim('-', '.');
        return string.IsNullOrWhiteSpace(sanitized) ? "evidence-file" : sanitized;
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best-effort cleanup. The project JSON remains the source of truth.
        }
    }
}
