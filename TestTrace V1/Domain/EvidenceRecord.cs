namespace TestTrace_V1.Domain;

public sealed class EvidenceRecord
{
    public Guid EvidenceId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string StoredFileName { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public string Sha256Hash { get; init; } = string.Empty;
    public EvidenceType EvidenceType { get; init; } = EvidenceType.Other;
    public string? Description { get; init; }
    public Guid TestItemId { get; init; }
    public string AttachedBy { get; init; } = string.Empty;
    public DateTimeOffset AttachedAt { get; init; }
    public AuthorityStamp? Authority { get; init; }

    public static EvidenceRecord Create(
        Guid evidenceId,
        string originalFileName,
        string storedFileName,
        string fileExtension,
        string sha256Hash,
        EvidenceType evidenceType,
        string? description,
        Guid testItemId,
        string attachedBy,
        DateTimeOffset attachedAt,
        AuthorityStamp? authority = null)
    {
        return new EvidenceRecord
        {
            EvidenceId = evidenceId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            FileExtension = fileExtension,
            Sha256Hash = sha256Hash,
            EvidenceType = evidenceType,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            TestItemId = testItemId,
            AttachedBy = attachedBy,
            AttachedAt = attachedAt,
            Authority = authority
        };
    }
}
