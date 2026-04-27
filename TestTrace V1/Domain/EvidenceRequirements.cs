using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class EvidenceRequirements
{
    [JsonInclude]
    public bool PhotoRequired { get; private set; }

    [JsonInclude]
    public bool MeasurementRequired { get; private set; }

    [JsonInclude]
    public bool SignatureRequired { get; private set; }

    [JsonInclude]
    public bool FileUploadRequired { get; private set; }

    [JsonInclude]
    public bool CommentRequiredOnFail { get; private set; }

    [JsonInclude]
    public bool CommentAlwaysRequired { get; private set; }

    public static EvidenceRequirements None()
    {
        return new EvidenceRequirements();
    }

    public static EvidenceRequirements Create(
        bool photoRequired,
        bool measurementRequired,
        bool signatureRequired,
        bool fileUploadRequired,
        bool commentRequiredOnFail,
        bool commentAlwaysRequired)
    {
        return new EvidenceRequirements
        {
            PhotoRequired = photoRequired,
            MeasurementRequired = measurementRequired,
            SignatureRequired = signatureRequired,
            FileUploadRequired = fileUploadRequired,
            CommentRequiredOnFail = commentRequiredOnFail,
            CommentAlwaysRequired = commentAlwaysRequired
        };
    }

    public string Describe()
    {
        var requirements = new List<string>();
        if (PhotoRequired)
        {
            requirements.Add("Photo");
        }

        if (MeasurementRequired)
        {
            requirements.Add("Measurement");
        }

        if (SignatureRequired)
        {
            requirements.Add("Signature");
        }

        if (FileUploadRequired)
        {
            requirements.Add("File upload");
        }

        if (CommentRequiredOnFail)
        {
            requirements.Add("Observation required on fail");
        }

        if (CommentAlwaysRequired)
        {
            requirements.Add("Observation always required");
        }

        return requirements.Count == 0 ? "No explicit evidence requirements" : string.Join(", ", requirements);
    }
}
