namespace TestTrace_V1.UI;

internal sealed class MotorDataPlateMetadata
{
    public string? RatedVoltage { get; init; }
    public string? RatedCurrent { get; init; }
    public string? PowerRating { get; init; }
    public string? Frequency { get; init; }
    public string? SpeedRpm { get; init; }
    public string? Phase { get; init; }

    public bool HasValues =>
        !string.IsNullOrWhiteSpace(RatedVoltage) ||
        !string.IsNullOrWhiteSpace(RatedCurrent) ||
        !string.IsNullOrWhiteSpace(PowerRating) ||
        !string.IsNullOrWhiteSpace(Frequency) ||
        !string.IsNullOrWhiteSpace(SpeedRpm) ||
        !string.IsNullOrWhiteSpace(Phase);
}

internal static class MotorDataPlateSerializer
{
    private const string StartMarker = "[MotorDataPlate]";
    private const string EndMarker = "[/MotorDataPlate]";

    public static (string? Notes, MotorDataPlateMetadata DataPlate) Parse(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return (null, new MotorDataPlateMetadata());
        }

        var text = notes.Trim();
        var start = text.IndexOf(StartMarker, StringComparison.Ordinal);
        var end = text.IndexOf(EndMarker, StringComparison.Ordinal);
        if (start < 0 || end <= start)
        {
            return (text, new MotorDataPlateMetadata());
        }

        var before = text[..start].Trim();
        var after = text[(end + EndMarker.Length)..].Trim();
        var remaining = string.Join(Environment.NewLine + Environment.NewLine, new[] { before, after }.Where(part => !string.IsNullOrWhiteSpace(part)));

        var block = text[(start + StartMarker.Length)..end];
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawLine in block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = rawLine.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            var key = rawLine[..separator].Trim();
            var value = rawLine[(separator + 1)..].Trim();
            values[key] = value;
        }

        return (TrimToNull(remaining), new MotorDataPlateMetadata
        {
            RatedVoltage = GetValue(values, "RatedVoltage"),
            RatedCurrent = GetValue(values, "RatedCurrent"),
            PowerRating = GetValue(values, "PowerRating"),
            Frequency = GetValue(values, "Frequency"),
            SpeedRpm = GetValue(values, "SpeedRpm"),
            Phase = GetValue(values, "Phase")
        });
    }

    public static string? Compose(string? notes, MotorDataPlateMetadata dataPlate)
    {
        var trimmedNotes = TrimToNull(notes);
        if (!dataPlate.HasValues)
        {
            return trimmedNotes;
        }

        var lines = new List<string>
        {
            StartMarker,
            $"RatedVoltage: {ValueOrBlank(dataPlate.RatedVoltage)}",
            $"RatedCurrent: {ValueOrBlank(dataPlate.RatedCurrent)}",
            $"PowerRating: {ValueOrBlank(dataPlate.PowerRating)}",
            $"Frequency: {ValueOrBlank(dataPlate.Frequency)}",
            $"SpeedRpm: {ValueOrBlank(dataPlate.SpeedRpm)}",
            $"Phase: {ValueOrBlank(dataPlate.Phase)}",
            EndMarker
        };

        var block = string.Join(Environment.NewLine, lines);
        return string.IsNullOrWhiteSpace(trimmedNotes)
            ? block
            : $"{trimmedNotes}{Environment.NewLine}{Environment.NewLine}{block}";
    }

    private static string? GetValue(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var value) ? TrimToNull(value) : null;
    }

    private static string ValueOrBlank(string? value)
    {
        return TrimToNull(value) ?? string.Empty;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
