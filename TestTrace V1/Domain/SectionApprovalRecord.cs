namespace TestTrace_V1.Domain;

public sealed class SectionApprovalRecord
{
    public Guid ApprovalId { get; init; }
    public Guid SectionId { get; init; }
    public string ApprovedBy { get; init; } = string.Empty;
    public DateTimeOffset ApprovedAt { get; init; }
    public string? Comments { get; init; }
    public AuthorityStamp? Authority { get; init; }

    public static SectionApprovalRecord Create(
        Guid approvalId,
        Guid sectionId,
        string approvedBy,
        DateTimeOffset approvedAt,
        string? comments,
        AuthorityStamp? authority = null)
    {
        return new SectionApprovalRecord
        {
            ApprovalId = approvalId,
            SectionId = sectionId,
            ApprovedBy = approvedBy,
            ApprovedAt = approvedAt,
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments,
            Authority = authority
        };
    }
}
