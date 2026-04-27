using TestTrace_V1.Domain;

namespace TestTrace_V1.Contracts;

public sealed class AddSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SectionApprover { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class AddAssetRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public string AssetName { get; init; } = string.Empty;
    public string? AssetType { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? Notes { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class AddSubAssetRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid ParentAssetId { get; init; }
    public string AssetName { get; init; } = string.Empty;
    public string? AssetType { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? Notes { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class AddAssetToSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string AssetName { get; init; } = string.Empty;
    public string? AssetType { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? Notes { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class AssignAssetToSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public Guid AssetId { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class AddTestItemRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public Guid AssetId { get; init; }
    public Guid ComponentId { get; init; }
    public string TestReference { get; init; } = string.Empty;
    public string TestTitle { get; init; } = string.Empty;
    public string? TestDescription { get; init; }
    public string? ExpectedOutcome { get; init; }
    public AcceptanceCriteria AcceptanceCriteria { get; init; } = TestTrace_V1.Domain.AcceptanceCriteria.ManualConfirmation();
    public EvidenceRequirements EvidenceRequirements { get; init; } = TestTrace_V1.Domain.EvidenceRequirements.None();
    public TestBehaviourRules BehaviourRules { get; init; } = TestTrace_V1.Domain.TestBehaviourRules.Default();
    public IReadOnlyList<TestInput> TestInputs { get; init; } = [];
    public string Actor { get; init; } = string.Empty;

    public Guid EffectiveAssetId => AssetId == Guid.Empty ? ComponentId : AssetId;
}

public sealed class OpenForExecutionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
}

public sealed class RenameSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
}

public sealed class RenameAssetRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid AssetId { get; init; }
    public string AssetName { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
}

public sealed class UpdateAssetMetadataRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid AssetId { get; init; }
    public string? AssetType { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? Notes { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class UpdateProjectMetadataRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public ReportingMetadata Metadata { get; init; } = new();
    public string Actor { get; init; } = string.Empty;
}

public sealed class RenameComponentRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid ComponentId { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
}

public sealed class RenameTestItemRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid TestItemId { get; init; }
    public string TestReference { get; init; } = string.Empty;
    public string TestTitle { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
}

public sealed class SetSectionApplicabilityRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public ApplicabilityState Applicability { get; init; }
    public string? Reason { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class SetAssetApplicabilityRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public Guid AssetId { get; init; }
    public ApplicabilityState Applicability { get; init; }
    public string? Reason { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class SetTestApplicabilityRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid TestItemId { get; init; }
    public ApplicabilityState Applicability { get; init; }
    public string? Reason { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class DeleteSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class DeleteAssetRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid AssetId { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class RemoveAssetFromSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public Guid AssetId { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class AddComponentToSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string ComponentName { get; init; } = string.Empty;
    public string? ComponentDescription { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class RemoveComponentFromSectionRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public Guid ComponentId { get; init; }
    public string Actor { get; init; } = string.Empty;
}

public sealed class DeleteTestItemRequest
{
    public string ProjectFolderPath { get; init; } = string.Empty;
    public Guid TestItemId { get; init; }
    public string Actor { get; init; } = string.Empty;
}
