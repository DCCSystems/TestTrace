namespace TestTrace_V1.Domain;

public sealed class AuditEntry
{
    public Guid AuditEntryId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
    public DateTimeOffset At { get; init; }
    public string? TargetType { get; init; }
    public Guid? TargetId { get; init; }
    public string? Details { get; init; }
    public AuthorityStamp? Authority { get; init; }
}
