using System.Text;
using System.Text.RegularExpressions;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;

namespace TestTrace_V1.Workspace;

public sealed class ExportService
{
    private readonly IProjectRepository repository;
    private readonly Func<DateTimeOffset> clock;

    public ExportService(IProjectRepository repository, Func<DateTimeOffset>? clock = null)
    {
        this.repository = repository;
        this.clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public ExportResult ExportFatReport(string projectFolderPath)
    {
        if (string.IsNullOrWhiteSpace(projectFolderPath))
        {
            return ExportResult.Failure("Project folder path is required.");
        }

        var location = ProjectLocation.FromProjectFolder(projectFolderPath.Trim());
        TestTraceProject project;
        try
        {
            project = repository.Load(location);
        }
        catch (Exception ex)
        {
            return ExportResult.Failure(ex.Message);
        }

        try
        {
            Directory.CreateDirectory(location.ExportsFolder);
            var fileName = $"FAT_REPORT_{NormalizeFilePart(project.ContractRoot.ProjectCode)}_{clock():yyyyMMdd_HHmmss}.md";
            var filePath = Path.Combine(location.ExportsFolder, fileName);
            File.WriteAllText(filePath, BuildMarkdown(project, location), Encoding.UTF8);
            return ExportResult.Success(filePath);
        }
        catch (Exception ex)
        {
            return ExportResult.Failure(ex.Message);
        }
    }

    private static string BuildMarkdown(TestTraceProject project, ProjectLocation location)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# FAT Report - {Text(project.ContractRoot.ProjectCode)}");
        builder.AppendLine();
        builder.AppendLine("## Contract Root");
        builder.AppendLine();
        AppendField(builder, "Project", project.ContractRoot.ProjectName);
        AppendField(builder, "Project code", project.ContractRoot.ProjectCode);
        AppendField(builder, "State", project.State.ToString());
        AppendField(builder, "Customer", project.ContractRoot.CustomerName);
        AppendField(builder, "Machine", $"{project.ContractRoot.MachineModel} / {project.ContractRoot.MachineSerialNumber}");
        AppendField(builder, "Project type", project.ContractRoot.ProjectType);
        AppendField(builder, "Lead test engineer", project.ContractRoot.LeadTestEngineer);
        AppendField(builder, "Release authority", project.ContractRoot.ReleaseAuthority);
        AppendField(builder, "Final approval authority", project.ContractRoot.FinalApprovalAuthority);
        AppendField(builder, "Created by", project.ContractRoot.CreatedBy);
        AppendField(builder, "Created at", project.ContractRoot.CreatedAt.LocalDateTime.ToString("g"));
        AppendField(builder, "Assets", project.Assets.Count.ToString());
        AppendField(builder, "Project folder", location.ProjectFolder);
        AppendField(builder, "Scope", project.ContractRoot.ScopeNarrative);
        builder.AppendLine();

        builder.AppendLine("## Project Metadata");
        builder.AppendLine();
        AppendField(builder, "Project Start Date", project.ReportingMetadata.ProjectStartDate?.ToString("dd MMM yyyy"));
        AppendField(builder, "Customer Address", project.ReportingMetadata.CustomerAddress);
        AppendField(builder, "Customer Country", project.ReportingMetadata.CustomerCountry);
        AppendField(builder, "Site Contact Name", project.ReportingMetadata.SiteContactName);
        AppendField(builder, "Site Contact Email", project.ReportingMetadata.SiteContactEmail);
        AppendField(builder, "Site Contact Number", project.ReportingMetadata.SiteContactPhone);
        AppendField(builder, "Customer Project Reference Name / Number", project.ReportingMetadata.CustomerProjectReference);
        AppendField(builder, "Machine Configuration / Specification", project.ReportingMetadata.MachineConfigurationSpecification);
        AppendField(builder, "Control Platform", project.ReportingMetadata.ControlPlatform);
        AppendField(builder, "Machine Role / Application", project.ReportingMetadata.MachineRoleApplication);
        AppendField(builder, "Software Version", project.ReportingMetadata.SoftwareVersion);
        AppendField(builder, "Lead Test Engineer Email", project.ReportingMetadata.LeadTestEngineerEmail);
        AppendField(builder, "Lead Test Engineer Phone", project.ReportingMetadata.LeadTestEngineerPhone);
        builder.AppendLine();

        AppendAuthorityModel(builder, project);

        builder.AppendLine("## Sections And Test Items");
        builder.AppendLine();
        if (project.Sections.Count == 0)
        {
            builder.AppendLine("_No sections defined._");
            builder.AppendLine();
        }

        foreach (var section in project.Sections.OrderBy(section => section.DisplayOrder))
        {
            builder.AppendLine($"### {section.DisplayOrder}. {Text(section.Title)}");
            builder.AppendLine();
            AppendField(builder, "Description", section.Description);
            AppendField(builder, "Approver", section.SectionApprover ?? project.ContractRoot.ReleaseAuthority);
            AppendField(builder, "Applicability", section.Applicability.ToString());
            AppendField(builder, "Applicability reason", section.ApplicabilityReason);
            AppendField(builder, "Approval", section.Approval is null
                ? "Not approved"
                : $"Approved by {section.Approval.ApprovedBy} at {section.Approval.ApprovedAt.LocalDateTime:g}");
            if (section.Approval?.Authority is not null)
            {
                AppendField(builder, "Approval authority stamp", FormatAuthority(section.Approval.Authority));
            }

            builder.AppendLine();

            if (section.Assets.Count == 0 && section.TestItems.Count == 0)
            {
                builder.AppendLine("_No assets or test items defined._");
                builder.AppendLine();
                continue;
            }

            foreach (var sectionAsset in section.Assets.OrderBy(asset => asset.DisplayOrder))
            {
                var asset = project.Assets.SingleOrDefault(candidate => candidate.AssetId == sectionAsset.AssetId);
                AppendAsset(builder, project, section, sectionAsset, asset, sectionAsset.DisplayOrder, isSubAsset: false);

                if (asset is null)
                {
                    continue;
                }

                foreach (var child in project.ChildAssets(asset.AssetId))
                {
                    AppendAsset(builder, project, section, sectionAsset, child, null, isSubAsset: true);
                }
            }

            foreach (var testItem in section.TestItems
                         .Where(testItem => testItem.AssetId is null)
                         .OrderBy(testItem => testItem.DisplayOrder))
            {
                AppendTestItem(builder, project, section, testItem);
            }
        }

        builder.AppendLine("## Release");
        builder.AppendLine();
        if (project.ReleaseRecord is null)
        {
            builder.AppendLine("_Project has not been released._");
            builder.AppendLine();
        }
        else
        {
            AppendField(builder, "Released by", project.ReleaseRecord.ReleasedBy);
            AppendField(builder, "Release authority stamp", FormatAuthority(project.ReleaseRecord.Authority));
            AppendField(builder, "Released at", project.ReleaseRecord.ReleasedAt.LocalDateTime.ToString("g"));
            AppendField(builder, "Declaration", project.ReleaseRecord.Declaration);
            builder.AppendLine();
        }

        builder.AppendLine("## Audit Trail");
        builder.AppendLine();
        foreach (var entry in project.AuditLog.OrderBy(entry => entry.At))
        {
            builder.AppendLine($"- {entry.At.LocalDateTime:g} | {entry.Actor} | {entry.Action} | {entry.Details ?? "-"}");
        }

        return builder.ToString();
    }

    private static void AppendAuthorityModel(StringBuilder builder, TestTraceProject project)
    {
        builder.AppendLine("## Authority Model");
        builder.AppendLine();
        if (project.Users.Count == 0)
        {
            builder.AppendLine("_No local users recorded._");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("### Users");
        builder.AppendLine();
        foreach (var user in project.Users.OrderBy(user => user.DisplayName))
        {
            builder.AppendLine($"- {Text(user.DisplayName)} ({Text(user.Initials)})");
            AppendIndentedField(builder, "User ID", user.UserId.ToString());
            AppendIndentedField(builder, "Email", user.Email);
            AppendIndentedField(builder, "Phone", user.Phone);
            AppendIndentedField(builder, "Organisation", user.Organisation);
            AppendIndentedField(builder, "Active", user.IsActive ? "Yes" : "No");
        }

        builder.AppendLine();
        builder.AppendLine("### Assignments");
        builder.AppendLine();
        if (project.AuthorityAssignments.Count == 0)
        {
            builder.AppendLine("- No authority assignments recorded.");
            builder.AppendLine();
            return;
        }

        foreach (var assignment in project.AuthorityAssignments
                     .Where(assignment => assignment.IsActive)
                     .OrderBy(assignment => assignment.ScopeType)
                     .ThenBy(assignment => assignment.Role)
                     .ThenBy(assignment => assignment.DisplayNameSnapshot))
        {
            builder.AppendLine($"- {assignment.Role} -> {Text(assignment.DisplayNameSnapshot)}");
            AppendIndentedField(builder, "Scope", $"{assignment.ScopeType} / {assignment.ScopeId}");
            AppendIndentedField(builder, "Assigned by", assignment.AssignedBy);
            AppendIndentedField(builder, "Assigned at", assignment.AssignedAt.LocalDateTime.ToString("g"));
            AppendIndentedField(builder, "Reason", assignment.Reason);
        }

        builder.AppendLine();
    }

    private static void AppendAsset(
        StringBuilder builder,
        TestTraceProject project,
        Section section,
        SectionAsset sectionAsset,
        Asset? asset,
        int? displayOrder,
        bool isSubAsset)
    {
        var title = isSubAsset ? "Sub-asset" : "Asset";
        var label = asset is null ? "Unknown asset" : asset.Name;
        var prefix = displayOrder is null ? string.Empty : $"{displayOrder}. ";
        builder.AppendLine(isSubAsset
            ? $"##### Sub-asset: {Text(label)}"
            : $"#### {prefix}{title}: {Text(label)}");
        builder.AppendLine();
        if (asset is not null)
        {
            AppendField(builder, "Applicability", isSubAsset && sectionAsset.Applicability == ApplicabilityState.NotApplicable
                ? "NotApplicable (inherited from parent asset assignment)"
                : sectionAsset.Applicability.ToString());
            AppendField(builder, "Applicability reason", sectionAsset.ApplicabilityReason);
            AppendField(builder, "Type", asset.Type);
            AppendField(builder, "Manufacturer", asset.Manufacturer);
            AppendField(builder, "Model", asset.Model);
            AppendField(builder, "Serial", asset.SerialNumber);
            AppendField(builder, "Notes", asset.Notes);
            builder.AppendLine();
        }

        var tests = section.TestItems
            .Where(testItem => testItem.AssetId == asset?.AssetId)
            .OrderBy(testItem => testItem.DisplayOrder)
            .ToList();

        if (tests.Count == 0)
        {
            builder.AppendLine($"_No test items defined for this {title.ToLowerInvariant()}._");
            builder.AppendLine();
            return;
        }

        foreach (var testItem in tests)
        {
            AppendTestItem(builder, project, section, testItem);
        }
    }

    private static void AppendTestItem(StringBuilder builder, TestTraceProject project, Section section, TestItem testItem)
    {
        builder.AppendLine($"##### {testItem.TestReference} - {Text(testItem.TestTitle)}");
        builder.AppendLine();
        AppendField(builder, "Description", testItem.TestDescription);
        AppendField(builder, "Expected outcome", testItem.ExpectedOutcome);
        AppendField(builder, "Applicability", project.EffectiveTestApplicability(section, testItem).ToString());
        AppendField(builder, "Applicability reason", project.EffectiveTestApplicabilityReason(section, testItem));
        AppendField(builder, "Acceptance criteria", testItem.AcceptanceCriteria.Describe());
        AppendField(builder, "Evidence requirements", testItem.EvidenceRequirements.Describe());
        AppendField(builder, "Behaviour", testItem.BehaviourRules.Describe());
        AppendTestInputs(builder, testItem);
        AppendField(builder, "Latest result", FormatResult(testItem.LatestResult));
        builder.AppendLine();
        AppendResults(builder, testItem);
        AppendEvidence(builder, testItem);
    }

    private static void AppendTestInputs(StringBuilder builder, TestItem testItem)
    {
        builder.AppendLine("- **Test inputs:**");
        if (testItem.Inputs.Count == 0)
        {
            builder.AppendLine("  - No structured inputs defined.");
            return;
        }

        foreach (var input in testItem.Inputs.OrderBy(input => input.DisplayOrder))
        {
            builder.AppendLine($"  - {Text(input.Label)}");
            builder.AppendLine($"    - Type: {input.InputType}");
            builder.AppendLine($"    - Required: {(input.Required ? "Yes" : "No")}");
            if (input.TargetValue is not null)
            {
                builder.AppendLine($"    - Target: {input.TargetValue:0.###}");
            }

            if (input.Tolerance is not null)
            {
                builder.AppendLine($"    - Tolerance: +/- {input.Tolerance:0.###}");
            }

            if (!string.IsNullOrWhiteSpace(input.Unit))
            {
                builder.AppendLine($"    - Unit: {input.Unit}");
            }
        }
    }

    private static void AppendResults(StringBuilder builder, TestItem testItem)
    {
        builder.AppendLine("Results:");
        if (testItem.ResultHistory.Count == 0)
        {
            builder.AppendLine("- No results recorded.");
            builder.AppendLine();
            return;
        }

        foreach (var result in testItem.ResultHistory.OrderByDescending(result => result.ExecutedAt))
        {
            builder.AppendLine($"- {FormatResult(result.Result)} by {result.ExecutedBy} at {result.ExecutedAt.LocalDateTime:g}");
            if (result.Authority is not null)
            {
                builder.AppendLine($"  - Authority: {FormatAuthority(result.Authority)}");
            }

            if (!string.IsNullOrWhiteSpace(result.MeasuredValue))
            {
                builder.AppendLine($"  - Measured: {result.MeasuredValue}");
            }

            AppendCapturedInputValues(builder, testItem, result);

            if (!string.IsNullOrWhiteSpace(result.Comments))
            {
                builder.AppendLine($"  - Comments: {result.Comments}");
            }
        }

        builder.AppendLine();
    }

    private static void AppendCapturedInputValues(StringBuilder builder, TestItem testItem, ResultEntry result)
    {
        if (result.CapturedInputValues.Count == 0)
        {
            return;
        }

        var definitions = testItem.Inputs.ToDictionary(input => input.TestInputId);
        builder.AppendLine("  - Captured inputs:");
        foreach (var captured in result.CapturedInputValues)
        {
            if (!definitions.TryGetValue(captured.TestInputId, out var input))
            {
                builder.AppendLine($"    - Unknown input: {Text(captured.Value)}");
                continue;
            }

            var unit = !string.IsNullOrWhiteSpace(input.Unit) ? $" {input.Unit.Trim()}" : string.Empty;
            builder.AppendLine($"    - {Text(input.Label)}: {FormatCapturedInputValue(input, captured.Value)}{unit}");
        }
    }

    private static string FormatCapturedInputValue(TestInput input, string value)
    {
        if (input.InputType == TestInputType.Boolean)
        {
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return "Yes";
            }

            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return "No";
            }
        }

        return Text(value);
    }

    private static void AppendEvidence(StringBuilder builder, TestItem testItem)
    {
        builder.AppendLine("Evidence:");
        if (testItem.EvidenceRecords.Count == 0)
        {
            builder.AppendLine("- No evidence attached.");
            builder.AppendLine();
            return;
        }

        foreach (var evidence in testItem.EvidenceRecords.OrderByDescending(evidence => evidence.AttachedAt))
        {
            builder.AppendLine($"- {evidence.StoredFileName}");
            builder.AppendLine($"  - Original: {evidence.OriginalFileName}");
            builder.AppendLine($"  - Attached by: {evidence.AttachedBy} at {evidence.AttachedAt.LocalDateTime:g}");
            if (evidence.Authority is not null)
            {
                builder.AppendLine($"  - Authority: {FormatAuthority(evidence.Authority)}");
            }

            builder.AppendLine($"  - SHA-256: {evidence.Sha256Hash}");
            if (!string.IsNullOrWhiteSpace(evidence.Description))
            {
                builder.AppendLine($"  - Description: {evidence.Description}");
            }
        }

        builder.AppendLine();
    }

    private static void AppendField(StringBuilder builder, string label, string? value)
    {
        builder.AppendLine($"- **{label}:** {Text(value)}");
    }

    private static void AppendIndentedField(StringBuilder builder, string label, string? value)
    {
        builder.AppendLine($"  - {label}: {Text(value)}");
    }

    private static string FormatAuthority(AuthorityStamp? authority)
    {
        if (authority is null)
        {
            return "-";
        }

        var role = authority.Role?.ToString() ?? "Actor";
        var scope = authority.ScopeType is null || authority.ScopeId is null
            ? string.Empty
            : $" ({authority.ScopeType} / {authority.ScopeId})";
        return $"{Text(authority.DisplayName)} as {role}{scope}";
    }

    private static string Text(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
    }

    private static string FormatResult(TestResult result)
    {
        return result == TestResult.NotApplicable
            ? "Legacy result: NotApplicable"
            : result.ToString();
    }

    private static string NormalizeFilePart(string value)
    {
        var normalized = Regex.Replace(value.Trim(), @"[^A-Za-z0-9._-]+", "-").Trim('-', '.');
        return string.IsNullOrWhiteSpace(normalized) ? "PROJECT" : normalized;
    }
}
