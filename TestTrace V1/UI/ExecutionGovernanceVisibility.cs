using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class ExecutionGovernanceVisibility
{
    private ExecutionGovernanceVisibility(IReadOnlyList<string> lines)
    {
        Lines = lines;
    }

    public IReadOnlyList<string> Lines { get; }

    public string Text => Lines.Count == 0
        ? "Governance: clear"
        : "Governance: " + string.Join(" | ", Lines);

    public static ExecutionGovernanceVisibility For(TestTraceProject? project, TestItem testItem)
    {
        var lines = new List<string>();
        var missingEvidence = project?.MissingEvidenceTypes(testItem) ?? [];
        if (missingEvidence.Count > 0)
        {
            lines.Add("Evidence " + string.Join(", ", missingEvidence));
        }

        if (testItem.BehaviourRules.RequiresWitness)
        {
            lines.Add("Witness required");
        }

        var latest = testItem.ResultHistory.LastOrDefault();
        if (latest?.Result == TestResult.Fail)
        {
            if (testItem.LatestFailureBlocksProgression())
            {
                lines.Add("Blocking fail");
            }
            else if (!string.IsNullOrWhiteSpace(latest.OverrideReason))
            {
                lines.Add("Override accepted");
            }
        }

        return new ExecutionGovernanceVisibility(lines);
    }
}
