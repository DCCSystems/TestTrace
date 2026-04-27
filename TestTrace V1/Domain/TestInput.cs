using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class TestInput
{
    public Guid TestInputId { get; init; }

    [JsonInclude]
    public string Label { get; private set; } = string.Empty;

    [JsonInclude]
    public TestInputType InputType { get; private set; } = TestInputType.Numeric;

    [JsonInclude]
    public decimal? TargetValue { get; private set; }

    [JsonInclude]
    public decimal? Tolerance { get; private set; }

    [JsonInclude]
    public string? Unit { get; private set; }

    [JsonInclude]
    public bool Required { get; private set; } = true;

    [JsonInclude]
    public int DisplayOrder { get; private set; }

    public static TestInput Create(
        Guid testInputId,
        string label,
        TestInputType inputType,
        decimal? targetValue,
        decimal? tolerance,
        string? unit,
        bool required,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new InvalidOperationException("Test input label is required.");
        }

        if (tolerance < 0)
        {
            throw new InvalidOperationException("Test input tolerance cannot be negative.");
        }

        return new TestInput
        {
            TestInputId = testInputId,
            Label = label.Trim(),
            InputType = inputType,
            TargetValue = targetValue,
            Tolerance = tolerance,
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            Required = required,
            DisplayOrder = displayOrder
        };
    }

    public string DescribeDefinition()
    {
        var requiredText = Required ? "required" : "optional";
        return InputType switch
        {
            TestInputType.Numeric => $"{Label}: numeric, target {FormatValue(TargetValue)}{UnitSuffix()}, +/- {FormatValue(Tolerance)} ({requiredText})",
            TestInputType.Boolean => $"{Label}: boolean ({requiredText})",
            TestInputType.Text => $"{Label}: text ({requiredText})",
            _ => $"{Label}: {InputType} ({requiredText})"
        };
    }

    private string UnitSuffix()
    {
        return string.IsNullOrWhiteSpace(Unit) ? string.Empty : " " + Unit.Trim();
    }

    private static string FormatValue(decimal? value)
    {
        return value is null ? "-" : value.Value.ToString("0.###");
    }
}
