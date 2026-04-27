using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;

namespace TestTrace_V1.Workspace;

public sealed class ApprovalService
{
    private readonly IProjectRepository repository;
    private readonly Func<DateTimeOffset> clock;

    public ApprovalService(IProjectRepository repository, Func<DateTimeOffset>? clock = null)
    {
        this.repository = repository;
        this.clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public OperationResult ApproveSection(ApproveSectionRequest request)
    {
        var validation = ValidateApproveSection(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.ApproveSection(
                request.SectionId,
                request.ApprovedBy.Trim(),
                clock(),
                TrimToNull(request.Comments)).ApprovalId);
    }

    public OperationResult ReleaseProject(ReleaseProjectRequest request)
    {
        var validation = ValidateReleaseProject(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.ReleaseProject(
                request.ReleasedBy.Trim(),
                clock(),
                request.Declaration.Trim()).ReleaseId);
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

    private static ValidationResult ValidateApproveSection(ApproveSectionRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath);
        if (request.SectionId == Guid.Empty)
        {
            issues.Add(Error("Required", "Section id is required.", nameof(request.SectionId)));
        }

        Required(request.ApprovedBy, nameof(request.ApprovedBy), "Approved by is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateReleaseProject(ReleaseProjectRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath);
        Required(request.ReleasedBy, nameof(request.ReleasedBy), "Released by is required.", issues);
        Required(request.Declaration, nameof(request.Declaration), "Release declaration is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static List<ValidationIssue> CommonProjectIssues(string projectFolderPath)
    {
        var issues = new List<ValidationIssue>();
        Required(projectFolderPath, nameof(projectFolderPath), "Project folder path is required.", issues);
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

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
