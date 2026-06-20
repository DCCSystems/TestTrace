using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class ExecutionGovernancePrompt
{
    private ExecutionGovernancePrompt(bool requiresWitness, bool offersOverrideReason)
    {
        RequiresWitness = requiresWitness;
        OffersOverrideReason = offersOverrideReason;
    }

    public bool IsRequired => RequiresWitness || OffersOverrideReason;

    public bool RequiresWitness { get; }

    public bool OffersOverrideReason { get; }

    public static ExecutionGovernancePrompt For(TestItem testItem, TestResult result)
    {
        var offersOverrideReason = result == TestResult.Fail &&
            testItem.BehaviourRules.AllowOverrideWithReason;

        return new ExecutionGovernancePrompt(
            testItem.BehaviourRules.RequiresWitness,
            offersOverrideReason);
    }
}
