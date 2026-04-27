namespace TestTrace_V1.Domain;

public sealed class ContractRoot
{
    public string ProjectType { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string ProjectCode { get; init; } = string.Empty;
    public string MachineModel { get; init; } = string.Empty;
    public string MachineSerialNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string ScopeNarrative { get; init; } = string.Empty;
    public string LeadTestEngineer { get; init; } = string.Empty;
    public string ReleaseAuthority { get; init; } = string.Empty;
    public string FinalApprovalAuthority { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
