using System.Text.Json;
using System.Text.Json.Serialization;
using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;

namespace TestTrace_V1.Persistence;

public sealed class JsonProjectRepository : IProjectRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public TestTraceProject Load(ProjectLocation location)
    {
        if (!File.Exists(location.ProjectFile))
        {
            throw new FileNotFoundException("Project file was not found.", location.ProjectFile);
        }

        var json = File.ReadAllText(location.ProjectFile);
        var project = JsonSerializer.Deserialize<TestTraceProject>(json, JsonOptions)
            ?? throw new InvalidDataException("Project JSON could not be deserialized.");
        project.NormalizeAfterLoad();
        return project;
    }

    public SaveResult Save(TestTraceProject project, ProjectLocation location)
    {
        try
        {
            Directory.CreateDirectory(location.ProjectFolder);
            Directory.CreateDirectory(location.EvidenceFolder);
            Directory.CreateDirectory(location.ExportsFolder);

            var tempFile = location.ProjectFile + ".tmp";
            var backupFile = location.ProjectFile + ".bak";
            var json = JsonSerializer.Serialize(project, JsonOptions);

            using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            if (File.Exists(location.ProjectFile))
            {
                File.Replace(tempFile, location.ProjectFile, backupFile, ignoreMetadataErrors: true);
                if (File.Exists(backupFile))
                {
                    File.Delete(backupFile);
                }
            }
            else
            {
                File.Move(tempFile, location.ProjectFile);
            }

            return SaveResult.Success();
        }
        catch (Exception ex)
        {
            return SaveResult.Failure(ex.Message);
        }
    }
}
