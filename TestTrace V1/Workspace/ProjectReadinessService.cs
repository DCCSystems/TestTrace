using TestTrace_V1.Domain;

namespace TestTrace_V1.Workspace;

public static class ProjectReadinessService
{
    public static ReadinessReport Evaluate(
        TestTraceProject project,
        Section? selectedSection,
        Asset? selectedAsset,
        TestItem? selectedTestItem)
    {
        var issues = new List<ReadinessIssue>();
        var isReleased = project.State == ProjectState.Released;
        var canStructure = project.State == ProjectState.DraftContractBuilt;
        var canAddAsset = canStructure && selectedSection is not null && selectedSection.Approval is null;
        var canAddSubAsset = canStructure && selectedAsset is not null && selectedAsset.ParentAssetId is null;
        var canAddTestItem = canStructure && selectedSection is not null && selectedAsset is not null;
        var hasEditableSelection = selectedSection is not null || selectedAsset is not null || selectedTestItem is not null;
        var canRenameSelection = canStructure && hasEditableSelection;
        var canDeleteSelection = canStructure && hasEditableSelection;
        var selectedTestApplicable = selectedSection is not null &&
            selectedTestItem is not null &&
            project.EffectiveTestApplicability(selectedSection, selectedTestItem) == ApplicabilityState.Applicable;
        var canOpenForExecution = project.State == ProjectState.DraftContractBuilt &&
            project.Sections.Count > 0 &&
            !EmptySectionAssets(project).Any() &&
            (!ApplicableSections(project).Any() ||
             ApplicableSections(project).Any(section =>
                 section.TestItems.Any(testItem => project.EffectiveTestApplicability(section, testItem) == ApplicabilityState.Applicable))) &&
            ApplicableSections(project).All(section =>
                section.TestItems.Count == 0 || section.TestItems.Any(testItem => project.EffectiveTestApplicability(section, testItem) == ApplicabilityState.Applicable));
        var canExecuteSelectedTest = project.State == ProjectState.Executable && selectedTestApplicable;
        var canApproveSection = CanApproveSelectedSection(project, selectedSection);
        var canRelease = project.State == ProjectState.Executable &&
            project.Sections.Count > 0 &&
            project.Sections.All(section => section.Approval is not null);

        if (isReleased)
        {
            Add(issues, "Structure", "Released projects are read-only.");
            Add(issues, "Execution", "Released projects are read-only.");
            Add(issues, "Approval", "Released projects are already closed.");
            Add(issues, "Release", "Project has already been released.");
        }
        else if (!canStructure)
        {
            Add(issues, "Structure", "Structure is frozen after the project is opened for execution.");
        }

        if (!canAddAsset)
        {
            Add(issues, "Structure", selectedSection is null
                ? "Select a section before adding or assigning an asset."
                : selectedSection.Approval is not null
                    ? "Approved sections cannot be changed."
                    : "Assets can only be assigned before execution.");
        }

        if (!canAddSubAsset)
        {
            Add(issues, "Structure", selectedAsset is null
                ? "Select a top-level asset before adding a sub-asset."
                : selectedAsset.ParentAssetId is not null
                    ? "This MVP only supports one sub-asset level."
                    : "Sub-assets can only be added before execution.");
        }

        if (!canAddTestItem)
        {
            Add(issues, "Structure", selectedAsset is null
                ? "Select an asset or sub-asset before adding a test item."
                : "Test items can only be added before execution.");
        }

        if (!canRenameSelection)
        {
            Add(issues, "Structure", canStructure
                ? "Select a section, asset, sub-asset, or test item before renaming."
                : "Rename is blocked because structure is frozen.");
        }

        if (!canDeleteSelection)
        {
            Add(issues, "Structure", canStructure
                ? "Select a section, asset, sub-asset, or test item before deleting."
                : "Delete is blocked because structure is frozen.");
        }

        if (!canOpenForExecution)
        {
            foreach (var message in OpenForExecutionIssues(project))
            {
                Add(issues, "Open", message);
            }
        }

        if (!canExecuteSelectedTest)
        {
            Add(issues, "Execution", ExecutionIssue(project, selectedSection, selectedTestItem));
        }

        if (!canApproveSection)
        {
            Add(issues, "Approval", ApprovalIssue(project, selectedSection));
        }

        if (!canRelease)
        {
            foreach (var message in ReleaseIssues(project))
            {
                Add(issues, "Release", message);
            }
        }

        return new ReadinessReport
        {
            IsReleased = isReleased,
            CanAddSection = canStructure,
            CanAddAsset = canAddAsset,
            CanAddSubAsset = canAddSubAsset,
            CanAddTestItem = canAddTestItem,
            CanRenameSelection = canRenameSelection,
            CanDeleteSelection = canDeleteSelection,
            CanOpenForExecution = canOpenForExecution,
            CanRecordResults = canExecuteSelectedTest,
            CanAttachEvidence = canExecuteSelectedTest,
            CanApproveSection = canApproveSection,
            CanRelease = canRelease,
            Issues = issues
        };
    }

    private static bool CanApproveSelectedSection(TestTraceProject project, Section? selectedSection)
    {
        if (project.State != ProjectState.Executable ||
            selectedSection is null ||
            selectedSection.Approval is not null)
        {
            return false;
        }

        if (selectedSection.Applicability == ApplicabilityState.NotApplicable)
        {
            return true;
        }

        var applicableTests = selectedSection.TestItems
            .Where(testItem => project.EffectiveTestApplicability(selectedSection, testItem) == ApplicabilityState.Applicable)
            .ToList();
        return applicableTests.Count > 0 &&
            !EmptySectionAssets(project, selectedSection).Any() &&
            applicableTests.All(testItem => !testItem.LatestFailureBlocksProgression() && testItem.LatestResult != TestResult.NotTested) &&
            applicableTests.All(testItem => project.MissingEvidenceTypes(testItem).Count == 0);
    }

    private static IEnumerable<string> OpenForExecutionIssues(TestTraceProject project)
    {
        if (project.State != ProjectState.DraftContractBuilt)
        {
            yield return "Only draft contract projects can be opened for execution.";
        }

        if (project.Sections.Count == 0)
        {
            yield return "Add at least one section.";
        }

        if (ApplicableSections(project).Any() &&
            !ApplicableSections(project).Any(section =>
                section.TestItems.Any(testItem => project.EffectiveTestApplicability(section, testItem) == ApplicabilityState.Applicable)))
        {
            yield return "Add at least one applicable test item, or mark out-of-scope sections as not applicable.";
        }

        foreach (var section in ApplicableSections(project).Where(section =>
                     section.TestItems.Count > 0 &&
                     section.TestItems.All(testItem => project.EffectiveTestApplicability(section, testItem) == ApplicabilityState.NotApplicable)))
        {
            yield return $"Section has no applicable test items: {section.Title}";
        }

        foreach (var message in EmptySectionAssets(project))
        {
            yield return message;
        }
    }

    private static string ExecutionIssue(TestTraceProject project, Section? selectedSection, TestItem? selectedTestItem)
    {
        if (project.State != ProjectState.Executable)
        {
            return "Project must be ready for execution before results or evidence can be recorded.";
        }

        if (selectedSection is null || selectedTestItem is null)
        {
            return "Select an applicable test item before recording results or attaching evidence.";
        }

        return project.EffectiveTestApplicability(selectedSection, selectedTestItem) == ApplicabilityState.NotApplicable
            ? "Selected test is out of scope for this machine/project."
            : "Select an applicable test item before recording results or attaching evidence.";
    }

    private static string ApprovalIssue(TestTraceProject project, Section? selectedSection)
    {
        if (project.State != ProjectState.Executable)
        {
            return "Project must be ready for execution before section approval.";
        }

        if (selectedSection is null)
        {
            return "Select a section before approval.";
        }

        if (selectedSection.Approval is not null)
        {
            return "Selected section is already approved.";
        }

        if (selectedSection.Applicability == ApplicabilityState.NotApplicable)
        {
            return string.Empty;
        }

        var applicableTests = selectedSection.TestItems
            .Where(testItem => project.EffectiveTestApplicability(selectedSection, testItem) == ApplicabilityState.Applicable)
            .ToList();
        if (applicableTests.Count == 0)
        {
            return "Section must contain at least one applicable test item before approval.";
        }

        var emptyAsset = EmptySectionAssets(project, selectedSection).FirstOrDefault();
        if (emptyAsset is not null)
        {
            return emptyAsset;
        }

        if (applicableTests.Any(testItem => testItem.LatestFailureBlocksProgression()))
        {
            return "Section cannot be approved while any applicable latest failed result blocks progression.";
        }

        if (applicableTests.Any(testItem => testItem.LatestResult == TestResult.NotTested))
        {
            return "Section cannot be approved while any applicable test item is not tested.";
        }

        var missingEvidence = applicableTests
            .Select(testItem => new
            {
                TestItem = testItem,
                Missing = project.MissingEvidenceTypes(testItem)
            })
            .FirstOrDefault(item => item.Missing.Count > 0);
        if (missingEvidence is not null)
        {
            return $"{missingEvidence.TestItem.TestReference} requires evidence before approval: {string.Join(", ", missingEvidence.Missing)}.";
        }

        return "Section cannot be approved yet.";
    }

    private static IEnumerable<string> ReleaseIssues(TestTraceProject project)
    {
        if (project.State != ProjectState.Executable)
        {
            yield return "Project must be ready for execution before release.";
        }

        if (project.Sections.Count == 0)
        {
            yield return "Project must contain at least one section.";
        }

        foreach (var section in project.Sections.Where(section => section.Approval is null))
        {
            yield return $"Section not approved: {section.Title}";
        }
    }

    private static void Add(List<ReadinessIssue> issues, string area, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (issues.Any(issue =>
                string.Equals(issue.Area, area, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(issue.Message, message, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        issues.Add(new ReadinessIssue
        {
            Area = area,
            Message = message
        });
    }

    private static IEnumerable<Section> ApplicableSections(TestTraceProject project)
    {
        return project.Sections.Where(section => section.Applicability == ApplicabilityState.Applicable);
    }

    private static IEnumerable<string> EmptySectionAssets(TestTraceProject project)
    {
        foreach (var section in ApplicableSections(project))
        {
            foreach (var message in EmptySectionAssets(project, section))
            {
                yield return message;
            }
        }
    }

    private static IEnumerable<string> EmptySectionAssets(TestTraceProject project, Section section)
    {
        foreach (var sectionAsset in section.Assets.Where(asset => asset.Applicability == ApplicabilityState.Applicable))
        {
            var scopedAssetIds = project.AssetScope(sectionAsset.AssetId);
            if (section.TestItems.Any(testItem =>
                    testItem.AssetId is not null &&
                    scopedAssetIds.Contains(testItem.AssetId.Value) &&
                    project.EffectiveTestApplicability(section, testItem) == ApplicabilityState.Applicable))
            {
                continue;
            }

            var asset = project.Assets.SingleOrDefault(candidate => candidate.AssetId == sectionAsset.AssetId);
            yield return $"Applicable asset has no applicable test items: {section.Title} / {asset?.Name ?? "Unknown asset"}";
        }
    }
}
