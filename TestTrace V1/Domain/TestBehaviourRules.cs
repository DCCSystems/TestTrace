using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class TestBehaviourRules
{
    [JsonInclude]
    public bool BlockProgressionIfFailed { get; private set; } = true;

    [JsonInclude]
    public bool AllowOverrideWithReason { get; private set; }

    [JsonInclude]
    public bool RequiresWitness { get; private set; }

    public static TestBehaviourRules Default()
    {
        return new TestBehaviourRules();
    }

    public static TestBehaviourRules Create(
        bool blockProgressionIfFailed,
        bool allowOverrideWithReason,
        bool requiresWitness)
    {
        return new TestBehaviourRules
        {
            BlockProgressionIfFailed = blockProgressionIfFailed,
            AllowOverrideWithReason = allowOverrideWithReason,
            RequiresWitness = requiresWitness
        };
    }

    public string Describe()
    {
        var rules = new List<string>();
        if (BlockProgressionIfFailed)
        {
            rules.Add("Block progression if failed");
        }

        if (AllowOverrideWithReason)
        {
            rules.Add("Allow override with reason");
        }

        if (RequiresWitness)
        {
            rules.Add("Witness required");
        }

        return rules.Count == 0 ? "No additional behaviour rules" : string.Join(", ", rules);
    }
}
