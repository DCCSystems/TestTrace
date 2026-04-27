using System.Text.Json;

namespace TestTrace_V1.UI;

public sealed class OperatorRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public List<OperatorProfile> Operators { get; init; } = [];
    public Guid? LastActiveOperatorId { get; set; }

    public IEnumerable<OperatorProfile> OrderedForPicker()
    {
        return Operators
            .OrderByDescending(op => op.OperatorId == LastActiveOperatorId)
            .ThenByDescending(op => op.LastActiveAt ?? op.CreatedAt)
            .ThenBy(op => op.DisplayName);
    }

    public OperatorProfile? FindById(Guid id)
    {
        return Operators.FirstOrDefault(op => op.OperatorId == id);
    }

    public OperatorProfile? FindByDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        var trimmed = displayName.Trim();
        return Operators.FirstOrDefault(op => string.Equals(op.DisplayName, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    public OperatorProfile Add(OperatorProfile profile)
    {
        if (Operators.Any(op => string.Equals(op.DisplayName, profile.DisplayName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"An operator profile named '{profile.DisplayName}' already exists.");
        }

        Operators.Add(profile);
        return profile;
    }

    public OperatorProfile Update(Guid id, OperatorProfile updated)
    {
        var existing = FindById(id) ?? throw new InvalidOperationException("Operator profile was not found.");
        if (Operators.Any(op =>
                op.OperatorId != id &&
                string.Equals(op.DisplayName, updated.DisplayName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Another operator profile named '{updated.DisplayName}' already exists.");
        }

        var index = Operators.IndexOf(existing);
        Operators[index] = updated;
        return updated;
    }

    public void MarkActive(Guid id, DateTimeOffset at)
    {
        var existing = FindById(id);
        if (existing is null)
        {
            return;
        }

        var index = Operators.IndexOf(existing);
        Operators[index] = existing.WithLastActive(at);
        LastActiveOperatorId = id;
    }

    public static string DefaultRegistryPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "TestTrace", "operators.json");
    }

    public static OperatorRegistry Load(string? path = null)
    {
        path ??= DefaultRegistryPath();
        if (!File.Exists(path))
        {
            return new OperatorRegistry();
        }

        try
        {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new OperatorRegistry();
            }

            var loaded = JsonSerializer.Deserialize<OperatorRegistry>(json, JsonOptions);
            return loaded ?? new OperatorRegistry();
        }
        catch
        {
            return new OperatorRegistry();
        }
    }

    public void Save(string? path = null)
    {
        path ??= DefaultRegistryPath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(this, JsonOptions));
        if (File.Exists(path))
        {
            File.Replace(tempPath, path, destinationBackupFileName: null);
        }
        else
        {
            File.Move(tempPath, path);
        }
    }
}
