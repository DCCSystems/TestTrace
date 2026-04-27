using TestTrace_V1.Domain;

namespace TestTrace_V1.Contracts;

public sealed class BuildContractRequest
{
    public string ProjectsRootDirectory { get; init; } = string.Empty;
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
    public ReportingMetadata? ReportingMetadata { get; init; }
}
