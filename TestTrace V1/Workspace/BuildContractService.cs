using System.Text.RegularExpressions;
using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;

namespace TestTrace_V1.Workspace;

public sealed class BuildContractService
{
    private static readonly HashSet<string> AllowedProjectTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "FAT",
        "SAT",
        "InternalValidation",
        "Investigation"
    };

    private readonly IProjectRepository repository;
    private readonly Func<DateTimeOffset> clock;

    public BuildContractService(IProjectRepository repository, Func<DateTimeOffset>? clock = null)
    {
        this.repository = repository;
        this.clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public BuildContractResult Build(BuildContractRequest request)
    {
        var validation = Validate(request);
        if (!validation.IsValid)
        {
            return BuildContractResult.Invalid(validation);
        }

        var at = clock();
        var projectId = Guid.NewGuid();
        var reportingMetadata = NormalizeReportingMetadata(request.ReportingMetadata);
        var contractRoot = new ContractRoot
        {
            ProjectType = request.ProjectType.Trim(),
            ProjectName = request.ProjectName.Trim(),
            ProjectCode = request.ProjectCode.Trim(),
            MachineModel = request.MachineModel.Trim(),
            MachineSerialNumber = request.MachineSerialNumber.Trim(),
            CustomerName = request.CustomerName.Trim(),
            ScopeNarrative = request.ScopeNarrative.Trim(),
            LeadTestEngineer = request.LeadTestEngineer.Trim(),
            ReleaseAuthority = request.ReleaseAuthority.Trim(),
            FinalApprovalAuthority = request.FinalApprovalAuthority.Trim(),
            CreatedBy = request.CreatedBy.Trim(),
            CreatedAt = at
        };

        var project = TestTraceProject.CreateDraftContractBuilt(
            projectId,
            contractRoot,
            reportingMetadata,
            contractRoot.CreatedBy,
            at);

        var projectFolderName = $"{NormalizeForFolder(contractRoot.ProjectCode)}_{projectId.ToString("N")[..8]}";
        var projectFolder = Path.Combine(request.ProjectsRootDirectory.Trim(), projectFolderName);
        var location = ProjectLocation.FromProjectFolder(projectFolder);

        if (File.Exists(location.ProjectFile))
        {
            return BuildContractResult.PersistenceFailure(
                validation,
                $"A TestTrace project already exists at {location.ProjectFile}.");
        }

        var saveResult = repository.Save(project, location);
        if (!saveResult.Succeeded)
        {
            TryCleanupFailedInitialWrite(location);
            return BuildContractResult.PersistenceFailure(
                validation,
                saveResult.ErrorMessage ?? "Project persistence failed.");
        }

        return BuildContractResult.Success(project, location.ProjectFolder, location.ProjectFile);
    }

    public ValidationResult Validate(BuildContractRequest request)
    {
        var issues = new List<ValidationIssue>();

        Required(request.ProjectsRootDirectory, nameof(request.ProjectsRootDirectory), "Projects root directory is required.", issues);
        Required(request.ProjectType, nameof(request.ProjectType), "Project type is required.", issues);
        Required(request.ProjectName, nameof(request.ProjectName), "Project name is required.", issues);
        Required(request.ProjectCode, nameof(request.ProjectCode), "Project code is required.", issues);
        Required(request.MachineModel, nameof(request.MachineModel), "Machine model is required.", issues);
        Required(request.MachineSerialNumber, nameof(request.MachineSerialNumber), "Machine serial number is required.", issues);
        Required(request.CustomerName, nameof(request.CustomerName), "Customer name is required.", issues);
        Required(request.LeadTestEngineer, nameof(request.LeadTestEngineer), "Lead Test Engineer is required.", issues);
        Required(request.ReleaseAuthority, nameof(request.ReleaseAuthority), "Release authority is required.", issues);
        Required(request.FinalApprovalAuthority, nameof(request.FinalApprovalAuthority), "Final approval authority is required.", issues);
        Required(request.CreatedBy, nameof(request.CreatedBy), "Created by is required.", issues);

        if (!string.IsNullOrWhiteSpace(request.ProjectType) && !AllowedProjectTypes.Contains(request.ProjectType.Trim()))
        {
            issues.Add(Error("InvalidProjectType", "Project type must be FAT, SAT, InternalValidation, or Investigation.", nameof(request.ProjectType)));
        }

        if (string.IsNullOrWhiteSpace(request.ScopeNarrative) || request.ScopeNarrative.Trim().Length < 20)
        {
            issues.Add(Error("ScopeTooShort", "Scope narrative must be at least 20 characters for the MVP contract root.", nameof(request.ScopeNarrative)));
        }

        if (!string.IsNullOrWhiteSpace(request.ProjectsRootDirectory))
        {
            var root = request.ProjectsRootDirectory.Trim();
            if (Path.Exists(root) && !Directory.Exists(root))
            {
                issues.Add(Error("ProjectsRootIsNotDirectory", "Projects root path exists but is not a directory.", nameof(request.ProjectsRootDirectory)));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.ProjectCode))
        {
            var normalized = NormalizeForFolder(request.ProjectCode);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                issues.Add(Error("ProjectCodeUnsafe", "Project code cannot be converted to a safe folder name.", nameof(request.ProjectCode)));
            }
        }

        return ValidationResult.FromIssues(issues);
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

    private static ReportingMetadata? NormalizeReportingMetadata(ReportingMetadata? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        var normalized = new ReportingMetadata
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

        return HasMetadataValues(normalized) ? normalized : null;
    }

    private static string NormalizeForFolder(string input)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitized = new string(input.Trim().Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        sanitized = Regex.Replace(sanitized, @"\s+", "-");
        sanitized = Regex.Replace(sanitized, @"-+", "-");
        return sanitized.Trim('-', '.');
    }

    private static void TryCleanupFailedInitialWrite(ProjectLocation location)
    {
        try
        {
            if (!Directory.Exists(location.ProjectFolder) || File.Exists(location.ProjectFile))
            {
                return;
            }

            var hasFiles = Directory.EnumerateFiles(location.ProjectFolder, "*", SearchOption.AllDirectories).Any();
            if (!hasFiles)
            {
                Directory.Delete(location.ProjectFolder, recursive: true);
            }
        }
        catch
        {
            // Cleanup is best-effort; the failed save result remains authoritative.
        }
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool HasMetadataValues(ReportingMetadata metadata)
    {
        return metadata.ProjectStartDate is not null ||
            !string.IsNullOrWhiteSpace(metadata.CustomerAddress) ||
            !string.IsNullOrWhiteSpace(metadata.CustomerCountry) ||
            !string.IsNullOrWhiteSpace(metadata.CustomerProjectReference) ||
            !string.IsNullOrWhiteSpace(metadata.SiteContactName) ||
            !string.IsNullOrWhiteSpace(metadata.SiteContactEmail) ||
            !string.IsNullOrWhiteSpace(metadata.SiteContactPhone) ||
            !string.IsNullOrWhiteSpace(metadata.MachineConfigurationSpecification) ||
            !string.IsNullOrWhiteSpace(metadata.ControlPlatform) ||
            !string.IsNullOrWhiteSpace(metadata.MachineRoleApplication) ||
            !string.IsNullOrWhiteSpace(metadata.SoftwareVersion) ||
            !string.IsNullOrWhiteSpace(metadata.LeadTestEngineerEmail) ||
            !string.IsNullOrWhiteSpace(metadata.LeadTestEngineerPhone);
    }
}
