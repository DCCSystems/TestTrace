namespace TestTrace_V1.Domain;

public sealed class CapturedTestInputValue
{
    public Guid TestInputId { get; init; }
    public string Value { get; init; } = string.Empty;

    public static CapturedTestInputValue Create(Guid testInputId, string? value)
    {
        if (testInputId == Guid.Empty)
        {
            throw new InvalidOperationException("Captured test input id is required.");
        }

        return new CapturedTestInputValue
        {
            TestInputId = testInputId,
            Value = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()
        };
    }
}
