using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;

namespace TestTrace_V1.Workspace;

public sealed class UsersAuthorityService
{
    private readonly IProjectRepository repository;
    private readonly Func<DateTimeOffset> clock;

    public UsersAuthorityService(IProjectRepository repository, Func<DateTimeOffset>? clock = null)
    {
        this.repository = repository;
        this.clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public OperationResult AddUser(AddUserRequest request)
    {
        var validation = ValidateAddUser(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return Mutate(
            request.ProjectFolderPath,
            project => project.AddUser(
                request.DisplayName.Trim(),
                Trim(request.Email),
                Trim(request.Phone),
                Trim(request.Organisation),
                request.Actor.Trim(),
                clock()).UserId);
    }

    public OperationResult UpdateUser(UpdateUserRequest request)
    {
        var validation = ValidateUpdateUser(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return Mutate(
            request.ProjectFolderPath,
            project =>
            {
                project.UpdateUser(
                    request.UserId,
                    request.DisplayName.Trim(),
                    Trim(request.Email),
                    Trim(request.Phone),
                    Trim(request.Organisation),
                    request.Actor.Trim(),
                    clock());
                return request.UserId;
            });
    }

    public OperationResult DeactivateUser(DeactivateUserRequest request)
    {
        var validation = ValidateDeactivateUser(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return Mutate(
            request.ProjectFolderPath,
            project =>
            {
                project.DeactivateUser(request.UserId, request.Reason.Trim(), request.Actor.Trim(), clock());
                return request.UserId;
            });
    }

    public OperationResult ReactivateUser(ReactivateUserRequest request)
    {
        var validation = ValidateProjectAndActor(request.ProjectFolderPath, request.Actor);
        RequiredId(request.UserId, nameof(request.UserId), "User id is required.", validation.IssuesMutable);
        var resolved = ValidationResult.FromIssues(validation.IssuesMutable);
        if (!resolved.IsValid)
        {
            return OperationResult.Invalid(resolved);
        }

        return Mutate(
            request.ProjectFolderPath,
            project =>
            {
                project.ReactivateUser(request.UserId, request.Actor.Trim(), clock());
                return request.UserId;
            });
    }

    public OperationResult AssignAuthority(AssignAuthorityRequest request)
    {
        var validation = ValidateAssignAuthority(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return Mutate(
            request.ProjectFolderPath,
            project => project.AssignAuthority(
                request.UserId,
                request.Role,
                request.ScopeType,
                request.ScopeId,
                request.Actor.Trim(),
                clock(),
                Trim(request.Reason)).AssignmentId);
    }

    public OperationResult RevokeAuthority(RevokeAuthorityRequest request)
    {
        var validation = ValidateRevokeAuthority(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return Mutate(
            request.ProjectFolderPath,
            project =>
            {
                project.RevokeAuthority(request.AssignmentId, request.Reason.Trim(), request.Actor.Trim(), clock());
                return request.AssignmentId;
            });
    }

    private OperationResult Mutate(string projectFolderPath, Func<TestTraceProject, Guid?> mutation)
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

    private static ValidationResult ValidateAddUser(AddUserRequest request)
    {
        var issues = CommonProjectAndActor(request.ProjectFolderPath, request.Actor);
        Required(request.DisplayName, nameof(request.DisplayName), "User display name is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateUpdateUser(UpdateUserRequest request)
    {
        var issues = CommonProjectAndActor(request.ProjectFolderPath, request.Actor);
        RequiredId(request.UserId, nameof(request.UserId), "User id is required.", issues);
        Required(request.DisplayName, nameof(request.DisplayName), "User display name is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateDeactivateUser(DeactivateUserRequest request)
    {
        var issues = CommonProjectAndActor(request.ProjectFolderPath, request.Actor);
        RequiredId(request.UserId, nameof(request.UserId), "User id is required.", issues);
        Required(request.Reason, nameof(request.Reason), "A reason is required to deactivate a user.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateAssignAuthority(AssignAuthorityRequest request)
    {
        var issues = CommonProjectAndActor(request.ProjectFolderPath, request.Actor);
        RequiredId(request.UserId, nameof(request.UserId), "User id is required.", issues);
        RequiredId(request.ScopeId, nameof(request.ScopeId), "Scope id is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateRevokeAuthority(RevokeAuthorityRequest request)
    {
        var issues = CommonProjectAndActor(request.ProjectFolderPath, request.Actor);
        RequiredId(request.AssignmentId, nameof(request.AssignmentId), "Assignment id is required.", issues);
        Required(request.Reason, nameof(request.Reason), "A reason is required to revoke an authority assignment.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ProjectActorValidation ValidateProjectAndActor(string projectFolderPath, string actor)
    {
        var issues = CommonProjectAndActor(projectFolderPath, actor);
        return new ProjectActorValidation(issues);
    }

    private static List<ValidationIssue> CommonProjectAndActor(string projectFolderPath, string actor)
    {
        var issues = new List<ValidationIssue>();
        Required(projectFolderPath, nameof(projectFolderPath), "Project folder path is required.", issues);
        Required(actor, nameof(actor), "Actor is required.", issues);
        return issues;
    }

    private static void Required(string? value, string field, string message, List<ValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new ValidationIssue
            {
                Code = "Required",
                Message = message,
                TargetField = field,
                Severity = Severity.Error
            });
        }
    }

    private static void RequiredId(Guid value, string field, string message, List<ValidationIssue> issues)
    {
        if (value == Guid.Empty)
        {
            issues.Add(new ValidationIssue
            {
                Code = "Required",
                Message = message,
                TargetField = field,
                Severity = Severity.Error
            });
        }
    }

    private static string? Trim(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed class ProjectActorValidation
    {
        public ProjectActorValidation(List<ValidationIssue> issues)
        {
            IssuesMutable = issues;
        }

        public List<ValidationIssue> IssuesMutable { get; }
    }
}
