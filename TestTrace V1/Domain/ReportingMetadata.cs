namespace TestTrace_V1.Domain;

public sealed class ReportingMetadata
{
    public DateOnly? ProjectStartDate { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerCountry { get; set; }
    public string? CustomerProjectReference { get; set; }
    public string? SiteContactName { get; set; }
    public string? SiteContactEmail { get; set; }
    public string? SiteContactPhone { get; set; }
    public string? MachineConfigurationSpecification { get; set; }
    public string? ControlPlatform { get; set; }
    public string? MachineRoleApplication { get; set; }
    public string? SoftwareVersion { get; set; }
    public string? LeadTestEngineerEmail { get; set; }
    public string? LeadTestEngineerPhone { get; set; }

    public ReportingMetadata Clone()
    {
        return new ReportingMetadata
        {
            ProjectStartDate = ProjectStartDate,
            CustomerAddress = CustomerAddress,
            CustomerCountry = CustomerCountry,
            CustomerProjectReference = CustomerProjectReference,
            SiteContactName = SiteContactName,
            SiteContactEmail = SiteContactEmail,
            SiteContactPhone = SiteContactPhone,
            MachineConfigurationSpecification = MachineConfigurationSpecification,
            ControlPlatform = ControlPlatform,
            MachineRoleApplication = MachineRoleApplication,
            SoftwareVersion = SoftwareVersion,
            LeadTestEngineerEmail = LeadTestEngineerEmail,
            LeadTestEngineerPhone = LeadTestEngineerPhone
        };
    }

    public bool ExecutionContextMatches(ReportingMetadata? other)
    {
        return other is not null &&
               Same(MachineConfigurationSpecification, other.MachineConfigurationSpecification) &&
               Same(ControlPlatform, other.ControlPlatform) &&
               Same(MachineRoleApplication, other.MachineRoleApplication) &&
               Same(SoftwareVersion, other.SoftwareVersion);
    }

    private static bool Same(string? left, string? right)
    {
        return string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
