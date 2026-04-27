using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;

namespace TestTrace_V1.Workspace;

public sealed class WorkspaceService
{
    private readonly IProjectRepository repository;
    private readonly Func<DateTimeOffset> clock;

    public WorkspaceService(IProjectRepository repository, Func<DateTimeOffset>? clock = null)
    {
        this.repository = repository;
        this.clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public OperationResult AddSection(AddSectionRequest request)
    {
        var validation = ValidateAddSection(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.AddSection(
                request.Title.Trim(),
                TrimToNull(request.Description),
                TrimToNull(request.SectionApprover),
                request.Actor.Trim(),
                clock()).SectionId);
    }

    public OperationResult AddAsset(AddAssetRequest request)
    {
        var validation = ValidateAddAsset(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.AddAsset(
                request.AssetName.Trim(),
                TrimToNull(request.AssetType),
                TrimToNull(request.Manufacturer),
                TrimToNull(request.Model),
                TrimToNull(request.SerialNumber),
                TrimToNull(request.Notes),
                request.Actor.Trim(),
                clock()).AssetId);
    }

    public OperationResult AddSubAsset(AddSubAssetRequest request)
    {
        var validation = ValidateAddSubAsset(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.AddSubAsset(
                request.ParentAssetId,
                request.AssetName.Trim(),
                TrimToNull(request.AssetType),
                TrimToNull(request.Manufacturer),
                TrimToNull(request.Model),
                TrimToNull(request.SerialNumber),
                TrimToNull(request.Notes),
                request.Actor.Trim(),
                clock()).AssetId);
    }

    public OperationResult AddAssetToSection(AddAssetToSectionRequest request)
    {
        var validation = ValidateAddAssetToSection(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.AddAssetToSection(
                request.SectionId,
                request.AssetName.Trim(),
                TrimToNull(request.AssetType),
                TrimToNull(request.Manufacturer),
                TrimToNull(request.Model),
                TrimToNull(request.SerialNumber),
                TrimToNull(request.Notes),
                request.Actor.Trim(),
                clock()).AssetId);
    }

    public OperationResult AssignAssetToSection(AssignAssetToSectionRequest request)
    {
        var validation = ValidateAssignAssetToSection(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.AssignAssetToSection(request.SectionId, request.AssetId, request.Actor.Trim(), clock());
                return request.AssetId;
            });
    }

    public OperationResult AddTestItem(AddTestItemRequest request)
    {
        var validation = ValidateAddTestItem(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project => project.AddTestItem(
                request.SectionId,
                request.EffectiveAssetId,
                request.TestReference.Trim(),
                request.TestTitle.Trim(),
                TrimToNull(request.TestDescription),
                TrimToNull(request.ExpectedOutcome),
                request.AcceptanceCriteria,
                request.EvidenceRequirements,
                request.BehaviourRules,
                request.TestInputs,
                request.Actor.Trim(),
                clock()).TestItemId);
    }

    public OperationResult AddComponentToSection(AddComponentToSectionRequest request)
    {
        return AddAssetToSection(new AddAssetToSectionRequest
        {
            ProjectFolderPath = request.ProjectFolderPath,
            SectionId = request.SectionId,
            AssetName = request.ComponentName,
            AssetType = "Component",
            Notes = request.ComponentDescription,
            Actor = request.Actor
        });
    }

    public OperationResult OpenForExecution(OpenForExecutionRequest request)
    {
        var validation = ValidateOpenForExecution(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.OpenForExecution(request.Actor.Trim(), clock());
                return project.ProjectId;
            });
    }

    public OperationResult RenameSection(RenameSectionRequest request)
    {
        var validation = ValidateRenameSection(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.RenameSection(request.SectionId, request.Title.Trim(), request.Actor.Trim(), clock());
                return request.SectionId;
            });
    }

    public OperationResult RenameAsset(RenameAssetRequest request)
    {
        var validation = ValidateRenameAsset(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.RenameAsset(request.AssetId, request.AssetName.Trim(), request.Actor.Trim(), clock());
                return request.AssetId;
            });
    }

    public OperationResult UpdateAssetMetadata(UpdateAssetMetadataRequest request)
    {
        var validation = ValidateUpdateAssetMetadata(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.UpdateAssetMetadata(
                    request.AssetId,
                    TrimToNull(request.AssetType),
                    TrimToNull(request.Manufacturer),
                    TrimToNull(request.Model),
                    TrimToNull(request.SerialNumber),
                    TrimToNull(request.Notes),
                    request.Actor.Trim(),
                    clock());
                return request.AssetId;
            });
    }

    public OperationResult UpdateProjectMetadata(UpdateProjectMetadataRequest request)
    {
        var validation = ValidateUpdateProjectMetadata(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.UpdateProjectMetadata(
                    NormalizeReportingMetadata(request.Metadata),
                    request.Actor.Trim(),
                    clock());
                return project.ProjectId;
            });
    }

    public OperationResult RenameComponent(RenameComponentRequest request)
    {
        return RenameAsset(new RenameAssetRequest
        {
            ProjectFolderPath = request.ProjectFolderPath,
            AssetId = request.ComponentId,
            AssetName = request.ComponentName,
            Actor = request.Actor
        });
    }

    public OperationResult RenameTestItem(RenameTestItemRequest request)
    {
        var validation = ValidateRenameTestItem(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.RenameTestItem(
                    request.TestItemId,
                    request.TestReference.Trim(),
                    request.TestTitle.Trim(),
                    request.Actor.Trim(),
                    clock());
                return request.TestItemId;
            });
    }

    public OperationResult SetSectionApplicability(SetSectionApplicabilityRequest request)
    {
        var validation = ValidateSetSectionApplicability(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.SetSectionApplicability(
                    request.SectionId,
                    request.Applicability,
                    TrimToNull(request.Reason),
                    request.Actor.Trim(),
                    clock());
                return request.SectionId;
            });
    }

    public OperationResult SetAssetApplicability(SetAssetApplicabilityRequest request)
    {
        var validation = ValidateSetAssetApplicability(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.SetAssetApplicability(
                    request.SectionId,
                    request.AssetId,
                    request.Applicability,
                    TrimToNull(request.Reason),
                    request.Actor.Trim(),
                    clock());
                return request.AssetId;
            });
    }

    public OperationResult SetTestApplicability(SetTestApplicabilityRequest request)
    {
        var validation = ValidateSetTestApplicability(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.SetTestApplicability(
                    request.TestItemId,
                    request.Applicability,
                    TrimToNull(request.Reason),
                    request.Actor.Trim(),
                    clock());
                return request.TestItemId;
            });
    }

    public OperationResult DeleteSection(DeleteSectionRequest request)
    {
        var validation = ValidateDeleteSection(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.DeleteSection(request.SectionId, request.Actor.Trim(), clock());
                return request.SectionId;
            });
    }

    public OperationResult DeleteAsset(DeleteAssetRequest request)
    {
        var validation = ValidateDeleteAsset(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.DeleteAsset(request.AssetId, request.Actor.Trim(), clock());
                return request.AssetId;
            });
    }

    public OperationResult RemoveAssetFromSection(RemoveAssetFromSectionRequest request)
    {
        var validation = ValidateRemoveAssetFromSection(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.RemoveAssetFromSection(
                    request.SectionId,
                    request.AssetId,
                    request.Actor.Trim(),
                    clock());
                return request.AssetId;
            });
    }

    public OperationResult RemoveComponentFromSection(RemoveComponentFromSectionRequest request)
    {
        return RemoveAssetFromSection(new RemoveAssetFromSectionRequest
        {
            ProjectFolderPath = request.ProjectFolderPath,
            SectionId = request.SectionId,
            AssetId = request.ComponentId,
            Actor = request.Actor
        });
    }

    public OperationResult DeleteTestItem(DeleteTestItemRequest request)
    {
        var validation = ValidateDeleteTestItem(request);
        if (!validation.IsValid)
        {
            return OperationResult.Invalid(validation);
        }

        return MutateProject(
            request.ProjectFolderPath,
            project =>
            {
                project.DeleteTestItem(request.TestItemId, request.Actor.Trim(), clock());
                return request.TestItemId;
            });
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

    private static ValidationResult ValidateAddSection(AddSectionRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        Required(request.Title, nameof(request.Title), "Section title is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateAddAsset(AddAssetRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        Required(request.AssetName, nameof(request.AssetName), "Asset name is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateAddSubAsset(AddSubAssetRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.ParentAssetId, nameof(request.ParentAssetId), "Parent asset id is required.", issues);
        Required(request.AssetName, nameof(request.AssetName), "Asset name is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateAddAssetToSection(AddAssetToSectionRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        Required(request.AssetName, nameof(request.AssetName), "Asset name is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateAssignAssetToSection(AssignAssetToSectionRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        RequiredId(request.AssetId, nameof(request.AssetId), "Asset id is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateAddTestItem(AddTestItemRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        RequiredId(request.EffectiveAssetId, nameof(request.AssetId), "Asset id is required.", issues);
        Required(request.TestReference, nameof(request.TestReference), "Test reference is required.", issues);
        Required(request.TestTitle, nameof(request.TestTitle), "Test title is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateOpenForExecution(OpenForExecutionRequest request)
    {
        return ValidationResult.FromIssues(CommonProjectIssues(request.ProjectFolderPath, request.Actor));
    }

    private static ValidationResult ValidateRenameSection(RenameSectionRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        Required(request.Title, nameof(request.Title), "Section title is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateRenameAsset(RenameAssetRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.AssetId, nameof(request.AssetId), "Asset id is required.", issues);
        Required(request.AssetName, nameof(request.AssetName), "Asset name is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateUpdateAssetMetadata(UpdateAssetMetadataRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.AssetId, nameof(request.AssetId), "Asset id is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateRenameTestItem(RenameTestItemRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.TestItemId, nameof(request.TestItemId), "Test item id is required.", issues);
        Required(request.TestReference, nameof(request.TestReference), "Test reference is required.", issues);
        Required(request.TestTitle, nameof(request.TestTitle), "Test title is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateUpdateProjectMetadata(UpdateProjectMetadataRequest request)
    {
        return ValidationResult.FromIssues(CommonProjectIssues(request.ProjectFolderPath, request.Actor));
    }

    private static ValidationResult ValidateSetSectionApplicability(SetSectionApplicabilityRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        RequiredNotApplicableReason(request.Applicability, request.Reason, nameof(request.Reason), issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateSetAssetApplicability(SetAssetApplicabilityRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        RequiredId(request.AssetId, nameof(request.AssetId), "Asset id is required.", issues);
        RequiredNotApplicableReason(request.Applicability, request.Reason, nameof(request.Reason), issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateSetTestApplicability(SetTestApplicabilityRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.TestItemId, nameof(request.TestItemId), "Test item id is required.", issues);
        RequiredNotApplicableReason(request.Applicability, request.Reason, nameof(request.Reason), issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateDeleteSection(DeleteSectionRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateDeleteAsset(DeleteAssetRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.AssetId, nameof(request.AssetId), "Asset id is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateRemoveAssetFromSection(RemoveAssetFromSectionRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.SectionId, nameof(request.SectionId), "Section id is required.", issues);
        RequiredId(request.AssetId, nameof(request.AssetId), "Asset id is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static ValidationResult ValidateDeleteTestItem(DeleteTestItemRequest request)
    {
        var issues = CommonProjectIssues(request.ProjectFolderPath, request.Actor);
        RequiredId(request.TestItemId, nameof(request.TestItemId), "Test item id is required.", issues);
        return ValidationResult.FromIssues(issues);
    }

    private static List<ValidationIssue> CommonProjectIssues(string projectFolderPath, string actor)
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
            issues.Add(Error("Required", message, field));
        }
    }

    private static void RequiredId(Guid value, string field, string message, List<ValidationIssue> issues)
    {
        if (value == Guid.Empty)
        {
            issues.Add(Error("Required", message, field));
        }
    }

    private static void RequiredNotApplicableReason(
        ApplicabilityState applicability,
        string? reason,
        string field,
        List<ValidationIssue> issues)
    {
        if (applicability == ApplicabilityState.NotApplicable && string.IsNullOrWhiteSpace(reason))
        {
            issues.Add(Error("Required", "A reason is required when marking an item not applicable.", field));
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

    private static ReportingMetadata NormalizeReportingMetadata(ReportingMetadata metadata)
    {
        return new ReportingMetadata
        {
            ProjectStartDate = metadata.ProjectStartDate,
            CustomerAddress = TrimToNull(metadata.CustomerAddress),
            CustomerCountry = TrimToNull(metadata.CustomerCountry),
            CustomerProjectReference = TrimToNull(metadata.CustomerProjectReference),
            SiteContactName = TrimToNull(metadata.SiteContactName),
            SiteContactEmail = TrimToNull(metadata.SiteContactEmail),
            SiteContactPhone = TrimToNull(metadata.SiteContactPhone),
            MachineConfigurationSpecification = TrimToNull(metadata.MachineConfigurationSpecification),
            ControlPlatform = TrimToNull(metadata.ControlPlatform),
            MachineRoleApplication = TrimToNull(metadata.MachineRoleApplication),
            SoftwareVersion = TrimToNull(metadata.SoftwareVersion),
            LeadTestEngineerEmail = TrimToNull(metadata.LeadTestEngineerEmail),
            LeadTestEngineerPhone = TrimToNull(metadata.LeadTestEngineerPhone)
        };
    }
}
