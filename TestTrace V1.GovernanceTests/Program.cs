using TestTrace_V1.Domain;
using TestTrace_V1.UI;
using TestTrace_V1.Workspace;

namespace TestTrace_V1.GovernanceTests;

internal static class Program
{
    private static int Main()
    {
        var tests = new (string Name, Action Run)[]
        {
            ("No prompt for ordinary pass", NoPromptForOrdinaryPass),
            ("Witness prompt for witnessed pass", WitnessPromptForWitnessedPass),
            ("Override prompt for overridable fail", OverridePromptForOverridableFail),
            ("Combined prompt for witnessed overridable fail", CombinedPromptForWitnessedOverridableFail),
            ("Missing required evidence blocks section readiness", MissingRequiredEvidenceBlocksSectionReadiness),
            ("Accepted override failure does not block section readiness", AcceptedOverrideFailureDoesNotBlockSectionReadiness),
            ("Failed result without override blocks section readiness", FailedResultWithoutOverrideBlocksSectionReadiness),
            ("Released project readiness is read-only", ReleasedProjectReadinessIsReadOnly),
            ("Visibility summary shows missing evidence", VisibilitySummaryShowsMissingEvidence),
            ("Visibility summary shows accepted override", VisibilitySummaryShowsAcceptedOverride),
            ("Visibility summary shows blocking failure", VisibilitySummaryShowsBlockingFailure),
            ("Visibility summary shows witness requirement", VisibilitySummaryShowsWitnessRequirement)
        };

        var failures = new List<string>();
        foreach (var test in tests)
        {
            try
            {
                test.Run();
                Console.WriteLine($"PASS {test.Name}");
            }
            catch (Exception ex)
            {
                failures.Add($"{test.Name}: {ex.Message}");
                Console.Error.WriteLine($"FAIL {test.Name}: {ex.Message}");
            }
        }

        if (failures.Count == 0)
        {
            Console.WriteLine($"{tests.Length} governance test(s) passed.");
            return 0;
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine($"{failures.Count} governance test(s) failed.");
        return 1;
    }

    private static void NoPromptForOrdinaryPass()
    {
        var prompt = ExecutionGovernancePrompt.For(TestWithRules(TestBehaviourRules.Default()), TestResult.Pass);

        AssertFalse(prompt.IsRequired, "Ordinary pass should not require governance prompt.");
        AssertFalse(prompt.RequiresWitness, "Ordinary pass should not require witness.");
        AssertFalse(prompt.OffersOverrideReason, "Ordinary pass should not offer override reason.");
    }

    private static void WitnessPromptForWitnessedPass()
    {
        var prompt = ExecutionGovernancePrompt.For(
            TestWithRules(TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: false,
                requiresWitness: true)),
            TestResult.Pass);

        AssertTrue(prompt.IsRequired, "Witnessed pass should require governance prompt.");
        AssertTrue(prompt.RequiresWitness, "Witnessed pass should require witness.");
        AssertFalse(prompt.OffersOverrideReason, "Witnessed pass should not offer override reason.");
    }

    private static void OverridePromptForOverridableFail()
    {
        var prompt = ExecutionGovernancePrompt.For(
            TestWithRules(TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: true,
                requiresWitness: false)),
            TestResult.Fail);

        AssertTrue(prompt.IsRequired, "Overridable fail should require governance prompt.");
        AssertFalse(prompt.RequiresWitness, "Overridable fail should not require witness.");
        AssertTrue(prompt.OffersOverrideReason, "Overridable fail should offer override reason.");
    }

    private static void CombinedPromptForWitnessedOverridableFail()
    {
        var prompt = ExecutionGovernancePrompt.For(
            TestWithRules(TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: true,
                requiresWitness: true)),
            TestResult.Fail);

        AssertTrue(prompt.IsRequired, "Witnessed overridable fail should require governance prompt.");
        AssertTrue(prompt.RequiresWitness, "Witnessed overridable fail should require witness.");
        AssertTrue(prompt.OffersOverrideReason, "Witnessed overridable fail should offer override reason.");
    }

    private static TestItem TestWithRules(TestBehaviourRules rules)
    {
        return TestItem.Create(
            Guid.NewGuid(),
            "TST-001",
            "Governed test",
            null,
            null,
            AcceptanceCriteria.ManualConfirmation(),
            EvidenceRequirements.None(),
            rules,
            [],
            1,
            Guid.NewGuid(),
            "Tester",
            DateTimeOffset.UtcNow);
    }

    private static void MissingRequiredEvidenceBlocksSectionReadiness()
    {
        var context = CreateExecutableProject(
            EvidenceRequirements.Create(
                photoRequired: true,
                measurementRequired: false,
                signatureRequired: false,
                fileUploadRequired: false,
                commentRequiredOnFail: false,
                commentAlwaysRequired: false),
            TestBehaviourRules.Default());

        context.Project.RecordResult(
            context.TestItem.TestItemId,
            TestResult.Pass,
            null,
            "Passed without evidence.",
            [],
            null,
            null,
            null,
            LeadEngineer,
            context.Now);

        var readiness = ProjectReadinessService.Evaluate(context.Project, context.Section, null, context.TestItem);

        AssertFalse(readiness.CanApproveSection, "Section readiness should block approval while required evidence is missing.");
        AssertContains(readiness.Issues.Select(issue => issue.Message), "requires evidence before approval: Photo");
    }

    private static void AcceptedOverrideFailureDoesNotBlockSectionReadiness()
    {
        var context = CreateExecutableProject(
            EvidenceRequirements.None(),
            TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: true,
                requiresWitness: false));

        context.Project.RecordResult(
            context.TestItem.TestItemId,
            TestResult.Fail,
            null,
            "Deviation accepted by customer.",
            [],
            null,
            null,
            "Accepted documented deviation.",
            LeadEngineer,
            context.Now);

        var readiness = ProjectReadinessService.Evaluate(context.Project, context.Section, null, context.TestItem);

        AssertTrue(readiness.CanApproveSection, "Accepted override failure should not block section readiness.");
    }

    private static void FailedResultWithoutOverrideBlocksSectionReadiness()
    {
        var context = CreateExecutableProject(
            EvidenceRequirements.None(),
            TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: true,
                requiresWitness: false));

        context.Project.RecordResult(
            context.TestItem.TestItemId,
            TestResult.Fail,
            null,
            "Deviation not yet accepted.",
            [],
            null,
            null,
            null,
            LeadEngineer,
            context.Now);

        var readiness = ProjectReadinessService.Evaluate(context.Project, context.Section, null, context.TestItem);

        AssertFalse(readiness.CanApproveSection, "Failed result without override reason should block section readiness.");
        AssertContains(readiness.Issues.Select(issue => issue.Message), "latest failed result blocks progression");
    }

    private static void ReleasedProjectReadinessIsReadOnly()
    {
        var context = CreateExecutableProject(EvidenceRequirements.None(), TestBehaviourRules.Default());
        context.Project.RecordResult(
            context.TestItem.TestItemId,
            TestResult.Pass,
            null,
            "Passed.",
            [],
            null,
            null,
            null,
            LeadEngineer,
            context.Now);
        context.Project.ApproveSection(context.Section.SectionId, ReleaseAuthority, context.Now, "Approved.");
        context.Project.ReleaseProject(ReleaseAuthority, context.Now, "Released for customer issue.");

        var readiness = ProjectReadinessService.Evaluate(context.Project, context.Section, null, context.TestItem);

        AssertTrue(readiness.IsReleased, "Readiness should report released project state.");
        AssertFalse(readiness.CanRecordResults, "Released project should not allow result recording.");
        AssertFalse(readiness.CanAttachEvidence, "Released project should not allow evidence attachment.");
        AssertFalse(readiness.CanApproveSection, "Released project should not allow section approval.");
        AssertFalse(readiness.CanRelease, "Released project should not allow release again.");
    }

    private static void VisibilitySummaryShowsMissingEvidence()
    {
        var context = CreateExecutableProject(
            EvidenceRequirements.Create(
                photoRequired: true,
                measurementRequired: true,
                signatureRequired: false,
                fileUploadRequired: false,
                commentRequiredOnFail: false,
                commentAlwaysRequired: false),
            TestBehaviourRules.Default());

        var summary = ExecutionGovernanceVisibility.For(context.Project, context.TestItem);

        AssertContains(summary.Lines, "Evidence Photo, Measurement");
        AssertContains([summary.Text], "Evidence Photo, Measurement");
    }

    private static void VisibilitySummaryShowsAcceptedOverride()
    {
        var context = CreateExecutableProject(
            EvidenceRequirements.None(),
            TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: true,
                requiresWitness: false));
        context.Project.RecordResult(
            context.TestItem.TestItemId,
            TestResult.Fail,
            null,
            "Deviation accepted.",
            [],
            null,
            null,
            "Customer accepted deviation.",
            LeadEngineer,
            context.Now);

        var summary = ExecutionGovernanceVisibility.For(context.Project, context.TestItem);

        AssertContains(summary.Lines, "Override accepted");
    }

    private static void VisibilitySummaryShowsBlockingFailure()
    {
        var context = CreateExecutableProject(
            EvidenceRequirements.None(),
            TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: true,
                requiresWitness: false));
        context.Project.RecordResult(
            context.TestItem.TestItemId,
            TestResult.Fail,
            null,
            "Deviation not accepted.",
            [],
            null,
            null,
            null,
            LeadEngineer,
            context.Now);

        var summary = ExecutionGovernanceVisibility.For(context.Project, context.TestItem);

        AssertContains(summary.Lines, "Blocking fail");
    }

    private static void VisibilitySummaryShowsWitnessRequirement()
    {
        var context = CreateExecutableProject(
            EvidenceRequirements.None(),
            TestBehaviourRules.Create(
                blockProgressionIfFailed: true,
                allowOverrideWithReason: false,
                requiresWitness: true));

        var summary = ExecutionGovernanceVisibility.For(context.Project, context.TestItem);

        AssertContains(summary.Lines, "Witness required");
    }

    private static GovernanceProjectContext CreateExecutableProject(
        EvidenceRequirements evidenceRequirements,
        TestBehaviourRules behaviourRules)
    {
        var now = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);
        var project = TestTraceProject.CreateDraftContractBuilt(
            Guid.NewGuid(),
            new ContractRoot
            {
                ProjectType = "FAT",
                ProjectName = "Governance Harness",
                ProjectCode = "GOV-001",
                MachineModel = "Model X",
                MachineSerialNumber = "SN-001",
                CustomerName = "Customer",
                ScopeNarrative = "Governed regression harness project for FAT execution readiness.",
                LeadTestEngineer = LeadEngineer,
                ReleaseAuthority = ReleaseAuthority,
                FinalApprovalAuthority = ReleaseAuthority,
                CreatedBy = ContractAuthor,
                CreatedAt = now
            },
            new ReportingMetadata(),
            ContractAuthor,
            now);

        var section = project.AddSection("Mechanical Safety", "Guarding checks.", ReleaseAuthority, ContractAuthor, now);
        var asset = project.AddAsset("Guard Assembly", "Guard", null, null, null, null, ContractAuthor, now);
        project.AssignAssetToSection(section.SectionId, asset.AssetId, ContractAuthor, now);
        var testItem = project.AddTestItem(
            section.SectionId,
            asset.AssetId,
            "MS-001",
            "Guard interlock",
            "Open the guard and confirm motion stops.",
            "Motion stops.",
            AcceptanceCriteria.ManualConfirmation(),
            evidenceRequirements,
            behaviourRules,
            [],
            ContractAuthor,
            now);
        project.OpenForExecution(ContractAuthor, now);

        return new GovernanceProjectContext(project, section, testItem, now);
    }

    private static void AssertTrue(bool value, string message)
    {
        if (!value)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertFalse(bool value, string message)
    {
        if (value)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertContains(IEnumerable<string> values, string expectedSubstring)
    {
        if (!values.Any(value => value.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Expected message containing '{expectedSubstring}'.");
        }
    }

    private const string ContractAuthor = "Contract Author";
    private const string LeadEngineer = "Lead Engineer";
    private const string ReleaseAuthority = "Release Authority";

    private sealed record GovernanceProjectContext(
        TestTraceProject Project,
        Section Section,
        TestItem TestItem,
        DateTimeOffset Now);
}
