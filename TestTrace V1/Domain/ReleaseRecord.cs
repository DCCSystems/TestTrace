namespace TestTrace_V1.Domain;

public sealed class ReleaseRecord
{
    public Guid ReleaseId { get; init; }
    public string ReleasedBy { get; init; } = string.Empty;
    public DateTimeOffset ReleasedAt { get; init; }
    public string Declaration { get; init; } = string.Empty;
    public AuthorityStamp? Authority { get; init; }

    public static ReleaseRecord Create(
        Guid releaseId,
        string releasedBy,
        DateTimeOffset releasedAt,
        string declaration,
        AuthorityStamp? authority = null)
    {
        return new ReleaseRecord
        {
            ReleaseId = releaseId,
            ReleasedBy = releasedBy,
            ReleasedAt = releasedAt,
            Declaration = declaration,
            Authority = authority
        };
    }
}
