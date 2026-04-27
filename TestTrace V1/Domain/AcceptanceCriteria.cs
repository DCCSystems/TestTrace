using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class AcceptanceCriteria
{
    [JsonInclude]
    public PassConditionType ConditionType { get; private set; } = PassConditionType.ManualConfirmation;

    [JsonInclude]
    public decimal? TargetValue { get; private set; }

    [JsonInclude]
    public decimal? Tolerance { get; private set; }

    [JsonInclude]
    public string? Unit { get; private set; }

    public static AcceptanceCriteria ManualConfirmation()
    {
        return new AcceptanceCriteria();
    }

    public static AcceptanceCriteria BooleanCondition()
    {
        return new AcceptanceCriteria
        {
            ConditionType = PassConditionType.BooleanCondition
        };
    }

    public static AcceptanceCriteria NumericRange(decimal targetValue, decimal tolerance, string? unit)
    {
        if (tolerance < 0)
        {
            throw new InvalidOperationException("Tolerance cannot be negative.");
        }

        return new AcceptanceCriteria
        {
            ConditionType = PassConditionType.NumericRange,
            TargetValue = targetValue,
            Tolerance = tolerance,
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim()
        };
    }

    public string Describe()
    {
        return ConditionType switch
        {
            PassConditionType.NumericRange => $"Numeric range: target {TargetValue:0.###}{UnitSuffix()} +/- {Tolerance:0.###}",
            PassConditionType.BooleanCondition => "Boolean condition",
            _ => "Manual confirmation"
        };
    }

    private string UnitSuffix()
    {
        return string.IsNullOrWhiteSpace(Unit) ? string.Empty : " " + Unit.Trim();
    }
}
