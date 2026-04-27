using System.Text.Json.Serialization;

namespace TestTrace_V1.Domain;

public sealed class TestTraceProject
{
    public string SchemaVersion { get; init; } = "1.1";
    public Guid ProjectId { get; init; }
    [JsonInclude]
    public ProjectState State { get; private set; }
    public ContractRoot ContractRoot { get; init; } = new();
    [JsonInclude]
    public ReportingMetadata ReportingMetadata { get; private set; } = new();
    public List<Asset> Assets { get; init; } = [];
    public List<UserAccount> Users { get; init; } = [];
    public List<AuthorityAssignment> AuthorityAssignments { get; init; } = [];
    [JsonInclude]
    public List<Component>? Components { get; private set; }
    public List<Section> Sections { get; init; } = [];
    [JsonInclude]
    public ReleaseRecord? ReleaseRecord { get; private set; }
    public List<AuditEntry> AuditLog { get; init; } = [];

    public static TestTraceProject CreateDraftContractBuilt(
        Guid projectId,
        ContractRoot contractRoot,
        ReportingMetadata? reportingMetadata,
        string actor,
        DateTimeOffset at)
    {
        var project = new TestTraceProject
        {
            ProjectId = projectId,
            State = ProjectState.DraftContractBuilt,
            ContractRoot = contractRoot,
            ReportingMetadata = reportingMetadata?.Clone() ?? new ReportingMetadata()
        };

        project.EnsureBaselineAuthorityModel(contractRoot, actor, at);
        project.AppendAudit("BuildContract", actor, at, "Project", projectId, "Contract root frozen.");
        return project;
    }

    public void NormalizeAfterLoad()
    {
        if (Components is not null)
        {
            foreach (var component in Components)
            {
                if (Assets.All(asset => asset.AssetId != component.ComponentId))
                {
                    Assets.Add(Asset.FromLegacyComponent(component));
                }
            }
        }

        Components = null;

        foreach (var section in Sections)
        {
            section.NormalizeLegacyAssetAssignments();
        }

        NormalizeAuthorityModel();
    }

    public Section AddSection(
        string title,
        string? description,
        string? sectionApprover,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var section = Section.Create(
            Guid.NewGuid(),
            title,
            description,
            Sections.Count + 1,
            sectionApprover,
            actor,
            at);

        Sections.Add(section);
        if (!string.IsNullOrWhiteSpace(section.SectionApprover))
        {
            EnsureAuthorityAssignmentForName(
                section.SectionApprover,
                AuthorityRole.SectionApprover,
                AuthorityScopeType.Section,
                section.SectionId,
                actor,
                at,
                $"Section approver for {section.Title}.");
        }

        AppendAudit("AddSection", actor, at, "Section", section.SectionId, section.Title);
        return section;
    }

    public Asset AddAsset(
        string assetName,
        string? assetType,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        if (TopLevelAssets().Any(asset => AssetNameMatches(asset, assetName)))
        {
            throw new InvalidOperationException("Asset name must be unique at the top level of the project.");
        }

        var asset = Asset.Create(
            Guid.NewGuid(),
            assetName,
            assetType,
            null,
            manufacturer,
            model,
            serialNumber,
            notes,
            actor,
            at);

        Assets.Add(asset);
        AppendAudit("AddAsset", actor, at, "Asset", asset.AssetId, asset.Name);
        return asset;
    }

    public Asset AddSubAsset(
        Guid parentAssetId,
        string assetName,
        string? assetType,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var parent = FindAsset(parentAssetId);
        if (parent.ParentAssetId is not null)
        {
            throw new InvalidOperationException("Sub-assets cannot contain another sub-asset in this MVP.");
        }

        if (Assets.Any(asset => asset.ParentAssetId == parentAssetId && AssetNameMatches(asset, assetName)))
        {
            throw new InvalidOperationException("Sub-asset name must be unique under the selected asset.");
        }

        var asset = Asset.Create(
            Guid.NewGuid(),
            assetName,
            assetType,
            parentAssetId,
            manufacturer,
            model,
            serialNumber,
            notes,
            actor,
            at);

        Assets.Add(asset);
        AppendAudit("AddSubAsset", actor, at, "Asset", asset.AssetId, $"{parent.Name} / {asset.Name}");
        return asset;
    }

    public Asset AddAssetToSection(
        Guid sectionId,
        string assetName,
        string? assetType,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var section = FindSection(sectionId);
        var asset = TopLevelAssets().SingleOrDefault(existing => AssetNameMatches(existing, assetName));
        if (asset is null)
        {
            asset = Asset.Create(
                Guid.NewGuid(),
                assetName,
                assetType,
                null,
                manufacturer,
                model,
                serialNumber,
                notes,
                actor,
                at);
            Assets.Add(asset);
            AppendAudit("AddAsset", actor, at, "Asset", asset.AssetId, asset.Name);
        }

        section.AddAsset(asset.AssetId, actor, at);
        AppendAudit("AssignAssetToSection", actor, at, "Asset", asset.AssetId, $"{asset.Name} assigned to {section.Title}.");
        return asset;
    }

    public void AssignAssetToSection(
        Guid sectionId,
        Guid assetId,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var section = FindSection(sectionId);
        var asset = FindAsset(assetId);
        if (asset.ParentAssetId is not null)
        {
            throw new InvalidOperationException("Assign the parent asset to the section; sub-assets are included through that parent.");
        }

        section.AddAsset(assetId, actor, at);
        AppendAudit("AssignAssetToSection", actor, at, "Asset", assetId, $"{asset.Name} assigned to {section.Title}.");
    }

    public TestItem AddTestItem(
        Guid sectionId,
        Guid assetId,
        string testReference,
        string testTitle,
        string? testDescription,
        string? expectedOutcome,
        AcceptanceCriteria? acceptanceCriteria,
        EvidenceRequirements? evidenceRequirements,
        TestBehaviourRules? behaviourRules,
        IReadOnlyList<TestInput>? inputs,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var section = FindSection(sectionId);
        var asset = FindAsset(assetId);
        if (!AssetIsAvailableInSection(section, asset))
        {
            throw new InvalidOperationException("Asset is not assigned to this section.");
        }

        if (section.TestItems
            .Where(testItem => testItem.AssetId == assetId)
            .Any(testItem => string.Equals(testItem.TestReference, testReference, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Test reference must be unique within the selected asset.");
        }

        var testItem = TestItem.Create(
            Guid.NewGuid(),
            testReference,
            testTitle,
            testDescription,
            expectedOutcome,
            acceptanceCriteria,
            evidenceRequirements,
            behaviourRules,
            inputs,
            section.TestItems.Count(testItem => testItem.AssetId == assetId) + 1,
            assetId,
            actor,
            at);

        section.AddTestItem(testItem);
        AppendAudit("AddTestItem", actor, at, "TestItem", testItem.TestItemId, testItem.TestReference);
        return testItem;
    }

    public void OpenForExecution(string actor, DateTimeOffset at)
    {
        if (State != ProjectState.DraftContractBuilt)
        {
            throw new InvalidOperationException("Only draft contract projects can be opened for execution.");
        }

        if (Sections.Count == 0)
        {
            throw new InvalidOperationException("At least one section is required before opening for execution.");
        }

        var applicableSections = Sections
            .Where(section => section.Applicability == ApplicabilityState.Applicable)
            .ToList();

        if (applicableSections.Any() &&
            !applicableSections.Any(section => section.TestItems.Any(testItem => IsTestApplicable(section, testItem))))
        {
            throw new InvalidOperationException("At least one applicable test item is required before opening for execution.");
        }

        if (applicableSections.Any(section => section.Assets
                .Where(sectionAsset => sectionAsset.Applicability == ApplicabilityState.Applicable)
                .Any(sectionAsset =>
                section.TestItems.All(testItem =>
                    testItem.AssetId is null ||
                    !AssetScope(sectionAsset.AssetId).Contains(testItem.AssetId.Value) ||
                    !IsTestApplicable(section, testItem)))))
        {
            throw new InvalidOperationException("Every applicable assigned asset must contain at least one applicable test item before opening for execution.");
        }

        State = ProjectState.Executable;
        AppendAudit("OpenForExecution", actor, at, "Project", ProjectId, "Project opened for governed test execution.");
    }

    public void RenameSection(
        Guid sectionId,
        string title,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var section = FindSection(sectionId);
        if (Sections.Any(candidate =>
                candidate.SectionId != sectionId &&
                string.Equals(candidate.Title.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Section title must be unique within the project.");
        }

        var previousTitle = section.Title;
        section.Rename(title);
        AppendAudit("RenameSection", actor, at, "Section", sectionId, $"{previousTitle} -> {section.Title}");
    }

    public void RenameAsset(
        Guid assetId,
        string assetName,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var asset = FindAsset(assetId);
        var siblings = asset.ParentAssetId is null
            ? TopLevelAssets()
            : Assets.Where(candidate => candidate.ParentAssetId == asset.ParentAssetId);
        if (siblings.Any(candidate =>
                candidate.AssetId != assetId &&
                string.Equals(candidate.Name.Trim(), assetName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Asset name must be unique within its parent scope.");
        }

        var previousName = asset.Name;
        asset.Rename(assetName);
        AppendAudit("RenameAsset", actor, at, "Asset", assetId, $"{previousName} -> {asset.Name}");
    }

    public void UpdateAssetMetadata(
        Guid assetId,
        string? assetType,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var asset = FindAsset(assetId);
        asset.UpdateMetadata(assetType, manufacturer, model, serialNumber, notes);
        AppendAudit("UpdateAssetMetadata", actor, at, "Asset", assetId, asset.Name);
    }

    public void UpdateProjectMetadata(
        ReportingMetadata metadata,
        string actor,
        DateTimeOffset at)
    {
        EnsureProjectMetadataEditable(metadata);
        ReportingMetadata = metadata.Clone();
        AppendAudit("UpdateProjectMetadata", actor, at, "Project", ProjectId, "Project metadata updated.");
    }

    public void RenameTestItem(
        Guid testItemId,
        string testReference,
        string testTitle,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var (section, testItem) = FindSectionAndTestItem(testItemId);

        if (section.TestItems
            .Where(candidate => candidate.AssetId == testItem.AssetId)
            .Any(candidate =>
                candidate.TestItemId != testItemId &&
                string.Equals(candidate.TestReference.Trim(), testReference.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Test reference must be unique within the selected asset.");
        }

        var previousLabel = $"{testItem.TestReference} - {testItem.TestTitle}";
        testItem.Rename(testReference, testTitle);
        AppendAudit("RenameTestItem", actor, at, "TestItem", testItemId, $"{previousLabel} -> {testItem.TestReference} - {testItem.TestTitle}");
    }

    public void SetSectionApplicability(
        Guid sectionId,
        ApplicabilityState applicability,
        string? reason,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsApplicabilityChange();

        var section = FindSection(sectionId);
        section.SetApplicability(applicability, reason);
        AppendAudit(
            "SetSectionApplicability",
            actor,
            at,
            "Section",
            sectionId,
            ApplicabilityAuditDetails(section.Title, applicability, reason));
    }

    public void SetAssetApplicability(
        Guid sectionId,
        Guid assetId,
        ApplicabilityState applicability,
        string? reason,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsApplicabilityChange();

        var section = FindSection(sectionId);
        var selectedAsset = FindAsset(assetId);
        var topLevelAssetId = TopLevelAssetId(assetId);
        var topLevelAsset = FindAsset(topLevelAssetId);
        section.SetAssetApplicability(topLevelAssetId, applicability, reason, AssetScope(topLevelAssetId));
        AppendAudit(
            "SetAssetApplicability",
            actor,
            at,
            "Asset",
            topLevelAssetId,
            ApplicabilityAuditDetails(
                selectedAsset.AssetId == topLevelAssetId ? topLevelAsset.Name : $"{topLevelAsset.Name} / {selectedAsset.Name}",
                applicability,
                reason));
    }

    public void SetTestApplicability(
        Guid testItemId,
        ApplicabilityState applicability,
        string? reason,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsApplicabilityChange();

        var (section, testItem) = FindSectionAndTestItem(testItemId);
        if (applicability == ApplicabilityState.Applicable &&
            (section.Applicability == ApplicabilityState.NotApplicable ||
             SectionAssetForTest(section, testItem)?.Applicability == ApplicabilityState.NotApplicable))
        {
            throw new InvalidOperationException("Cannot mark a child test applicable while its parent scope is not applicable.");
        }

        section.SetTestApplicability(testItemId, applicability, reason);
        AppendAudit(
            "SetTestApplicability",
            actor,
            at,
            "TestItem",
            testItemId,
            ApplicabilityAuditDetails($"{testItem.TestReference} - {testItem.TestTitle}", applicability, reason));
    }

    public void DeleteSection(
        Guid sectionId,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var section = FindSection(sectionId);
        Sections.Remove(section);
        ReindexSections();
        AppendAudit("DeleteSection", actor, at, "Section", sectionId, section.Title);
    }

    public void RemoveAssetFromSection(
        Guid sectionId,
        Guid assetId,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var section = FindSection(sectionId);
        var asset = FindAsset(assetId);
        if (asset.ParentAssetId is not null)
        {
            throw new InvalidOperationException("Sub-assets are removed by removing their parent asset assignment or deleting the sub-asset globally.");
        }

        section.RemoveAsset(assetId, AssetScope(assetId));
        AppendAudit("RemoveAssetFromSection", actor, at, "Asset", assetId, $"{asset.Name} removed from {section.Title}.");
    }

    public void DeleteAsset(
        Guid assetId,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var asset = FindAsset(assetId);
        var scope = AssetScope(assetId);
        foreach (var section in Sections)
        {
            section.DeleteTestItemsForAssets(scope);
            section.RemoveAssetAssignments(scope);
        }

        Assets.RemoveAll(candidate => scope.Contains(candidate.AssetId));
        AppendAudit("DeleteAsset", actor, at, "Asset", assetId, asset.ParentAssetId is null
            ? $"{asset.Name} and its sub-assets deleted."
            : $"{asset.Name} deleted.");
    }

    public void DeleteTestItem(
        Guid testItemId,
        string actor,
        DateTimeOffset at)
    {
        EnsureStateAllowsWorkspaceStructure();

        var (section, testItem) = FindSectionAndTestItem(testItemId);
        section.DeleteTestItem(testItemId);
        AppendAudit("DeleteTestItem", actor, at, "TestItem", testItemId, $"{testItem.TestReference} - {testItem.TestTitle}");
    }

    public ResultEntry RecordResult(
        Guid testItemId,
        TestResult result,
        string? measuredValue,
        string? comments,
        IReadOnlyList<CapturedTestInputValue>? capturedInputValues,
        Guid? supersedesResultEntryId,
        string executedBy,
        DateTimeOffset executedAt)
    {
        EnsureExecutable();

        var (section, testItem) = FindSectionAndTestItem(testItemId);
        if (!IsTestApplicable(section, testItem))
        {
            throw new InvalidOperationException("Cannot record a result for a not applicable test.");
        }

        var authority = CreateAuthorityStamp(
            executedBy,
            AuthorityRole.TestExecutor,
            AuthorityScopeType.TestItem,
            testItemId,
            executedAt);

        var entry = testItem.RecordResult(
          result,
          measuredValue,
          comments,
          capturedInputValues,
          supersedesResultEntryId,
          executedBy,
          executedAt,
          authority);

        AppendAudit("RecordResult", executedBy, executedAt, "TestItem", testItemId, result.ToString());
        return entry;
    }

    public void AttachEvidence(EvidenceRecord evidenceRecord)
    {
        EnsureExecutable();

        var (section, testItem) = FindSectionAndTestItem(evidenceRecord.TestItemId);
        if (!IsTestApplicable(section, testItem))
        {
            throw new InvalidOperationException("Cannot attach evidence to a not applicable test.");
        }

        testItem.AttachEvidence(evidenceRecord);
        AppendAudit("AttachEvidence", evidenceRecord.AttachedBy, evidenceRecord.AttachedAt, "Evidence", evidenceRecord.EvidenceId, evidenceRecord.StoredFileName);
    }

    public SectionApprovalRecord ApproveSection(
        Guid sectionId,
        string approvedBy,
        DateTimeOffset approvedAt,
        string? comments)
    {
        EnsureExecutable();

        var section = FindSection(sectionId);
        var approvalRole = SectionApprovalRoleFor(section, approvedBy);
        var approvalScopeType = approvalRole == AuthorityRole.SectionApprover
            ? AuthorityScopeType.Section
            : AuthorityScopeType.Project;
        var approvalScopeId = approvalRole == AuthorityRole.SectionApprover
            ? sectionId
            : ProjectId;
        var authority = CreateAuthorityStamp(
            approvedBy,
            approvalRole,
            approvalScopeType,
            approvalScopeId,
            approvedAt);

        var approval = section.Approve(
            approvedBy,
            ContractRoot.ReleaseAuthority,
            approvedAt,
            comments,
            AssetScope,
            IsTestApplicable,
            authority);

        AppendAudit("ApproveSection", approvedBy, approvedAt, "Section", sectionId, section.Title);
        return approval;
    }

    public ReleaseRecord ReleaseProject(
        string releasedBy,
        DateTimeOffset releasedAt,
        string declaration)
    {
        EnsureExecutable();

        if (Sections.Count == 0)
        {
            throw new InvalidOperationException("Project must contain at least one section before release.");
        }

        if (Sections.Any(s => s.Approval is null))
        {
            throw new InvalidOperationException("All sections must be approved before release.");
        }

        if (!string.Equals(releasedBy.Trim(), ContractRoot.ReleaseAuthority.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Releaser must match the project release authority.");
        }

        if (string.IsNullOrWhiteSpace(declaration))
        {
            throw new InvalidOperationException("Release declaration is required.");
        }

        var authority = CreateAuthorityStamp(
            releasedBy,
            AuthorityRole.ReleaseAuthority,
            AuthorityScopeType.Project,
            ProjectId,
            releasedAt);

        ReleaseRecord = ReleaseRecord.Create(
            Guid.NewGuid(),
            releasedBy,
            releasedAt,
            declaration,
            authority);

        State = ProjectState.Released;
        AppendAudit("ReleaseProject", releasedBy, releasedAt, "Project", ProjectId, "Project released.");
        return ReleaseRecord;
    }

    public void AppendAudit(
        string action,
        string actor,
        DateTimeOffset at,
        string? targetType = null,
        Guid? targetId = null,
        string? details = null)
    {
        AuditLog.Add(new AuditEntry
        {
            AuditEntryId = Guid.NewGuid(),
            Action = action,
            Actor = actor,
            At = at,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            Authority = CreateAuthorityStamp(actor, null, null, null, at)
        });
    }

    public IReadOnlyList<Asset> ChildAssets(Guid parentAssetId)
    {
        return Assets
            .Where(asset => asset.ParentAssetId == parentAssetId)
            .OrderBy(asset => asset.Name)
            .ToList();
    }

    public IReadOnlySet<Guid> AssetScope(Guid assetId)
    {
        return Assets
            .Where(asset => asset.AssetId == assetId || asset.ParentAssetId == assetId)
            .Select(asset => asset.AssetId)
            .ToHashSet();
    }

    public Guid TopLevelAssetId(Guid assetId)
    {
        var asset = FindAsset(assetId);
        return asset.ParentAssetId ?? asset.AssetId;
    }

    public ApplicabilityState EffectiveTestApplicability(Section section, TestItem testItem)
    {
        return IsTestApplicable(section, testItem)
            ? ApplicabilityState.Applicable
            : ApplicabilityState.NotApplicable;
    }

    public string? EffectiveTestApplicabilityReason(Section section, TestItem testItem)
    {
        if (section.Applicability == ApplicabilityState.NotApplicable)
        {
            return section.ApplicabilityReason;
        }

        var sectionAsset = SectionAssetForTest(section, testItem);
        if (sectionAsset?.Applicability == ApplicabilityState.NotApplicable)
        {
            return sectionAsset.ApplicabilityReason;
        }

        return testItem.Applicability == ApplicabilityState.NotApplicable
            ? testItem.ApplicabilityReason
            : null;
    }

    public AuthorityStamp CreateAuthorityStamp(
        string actor,
        AuthorityRole? role,
        AuthorityScopeType? scopeType,
        Guid? scopeId,
        DateTimeOffset at)
    {
        var user = EnsureUserAccount(
            actor,
            email: null,
            phone: null,
            organisation: null,
            createdBy: actor,
            createdAt: at);

        return AuthorityStamp.Create(user, role, scopeType, scopeId);
    }

    public UserAccount AddUser(
        string displayName,
        string? email,
        string? phone,
        string? organisation,
        string actor,
        DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new InvalidOperationException("User display name is required.");
        }

        var trimmed = displayName.Trim();
        if (Users.Any(user => string.Equals(user.DisplayName, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A user with this display name already exists in the project.");
        }

        var user = UserAccount.Create(
            Guid.NewGuid(),
            trimmed,
            email,
            phone,
            organisation,
            actor,
            at);
        Users.Add(user);
        AppendAudit("AddUser", actor, at, "User", user.UserId, $"{user.DisplayName} added to the project user registry.");
        return user;
    }

    public UserAccount UpdateUser(
        Guid userId,
        string displayName,
        string? email,
        string? phone,
        string? organisation,
        string actor,
        DateTimeOffset at)
    {
        var user = FindUser(userId);
        var trimmed = (displayName ?? string.Empty).Trim();

        if (Users.Any(candidate =>
                candidate.UserId != userId &&
                string.Equals(candidate.DisplayName, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Another user already uses this display name in the project.");
        }

        var previous = user.DisplayName;
        user.UpdateProfile(trimmed, email, phone, organisation);
        AppendAudit(
            "UpdateUser",
            actor,
            at,
            "User",
            user.UserId,
            string.Equals(previous, user.DisplayName, StringComparison.Ordinal)
                ? $"{user.DisplayName} profile updated."
                : $"{previous} -> {user.DisplayName} (profile updated).");
        return user;
    }

    public void DeactivateUser(
        Guid userId,
        string reason,
        string actor,
        DateTimeOffset at)
    {
        var user = FindUser(userId);
        if (!user.IsActive)
        {
            return;
        }

        var blockingAssignments = AuthorityAssignments
            .Where(assignment => assignment.IsActive && assignment.UserId == userId)
            .ToList();

        foreach (var assignment in blockingAssignments)
        {
            if (IsCriticalRole(assignment.Role) &&
                IsSoleActiveBearer(assignment.UserId, assignment.Role, assignment.ScopeType, assignment.ScopeId))
            {
                throw new InvalidOperationException(
                    $"Cannot deactivate {user.DisplayName}: they are the only active {assignment.Role}. Assign another user first.");
            }
        }

        foreach (var assignment in blockingAssignments)
        {
            assignment.Revoke(actor, $"User deactivated: {reason}", at);
        }

        user.Deactivate(reason, at);
        AppendAudit("DeactivateUser", actor, at, "User", user.UserId, $"{user.DisplayName} deactivated. Reason: {reason.Trim()}");
    }

    public void ReactivateUser(Guid userId, string actor, DateTimeOffset at)
    {
        var user = FindUser(userId);
        if (user.IsActive)
        {
            return;
        }

        user.Reactivate(at);
        AppendAudit("ReactivateUser", actor, at, "User", user.UserId, $"{user.DisplayName} reactivated.");
    }

    public AuthorityAssignment AssignAuthority(
        Guid userId,
        AuthorityRole role,
        AuthorityScopeType scopeType,
        Guid scopeId,
        string actor,
        DateTimeOffset at,
        string? reason)
    {
        var user = FindUser(userId);
        if (!user.IsActive)
        {
            throw new InvalidOperationException($"Cannot assign authority to deactivated user {user.DisplayName}.");
        }

        if (scopeId == Guid.Empty)
        {
            throw new InvalidOperationException("Authority assignment scope id is required.");
        }

        EnsureScopeExists(scopeType, scopeId);

        var assignment = EnsureAuthorityAssignment(user, role, scopeType, scopeId, actor, at, reason);
        AppendAudit(
            "AssignAuthority",
            actor,
            at,
            "Authority",
            assignment.AssignmentId,
            $"{user.DisplayName} -> {role} on {ScopeLabel(scopeType, scopeId)}.");
        return assignment;
    }

    public void RevokeAuthority(
        Guid assignmentId,
        string reason,
        string actor,
        DateTimeOffset at)
    {
        var assignment = AuthorityAssignments.SingleOrDefault(candidate => candidate.AssignmentId == assignmentId)
            ?? throw new InvalidOperationException("Authority assignment was not found.");

        if (!assignment.IsActive)
        {
            return;
        }

        if (IsCriticalRole(assignment.Role) &&
            IsSoleActiveBearer(assignment.UserId, assignment.Role, assignment.ScopeType, assignment.ScopeId))
        {
            throw new InvalidOperationException(
                $"Cannot revoke the only active {assignment.Role} for {ScopeLabel(assignment.ScopeType, assignment.ScopeId)}. Assign another user first.");
        }

        assignment.Revoke(actor, reason, at);
        AppendAudit(
            "RevokeAuthority",
            actor,
            at,
            "Authority",
            assignment.AssignmentId,
            $"{assignment.DisplayNameSnapshot} revoked from {assignment.Role} on {ScopeLabel(assignment.ScopeType, assignment.ScopeId)}. Reason: {reason.Trim()}");
    }

    public string ScopeLabel(AuthorityScopeType scopeType, Guid scopeId)
    {
        return scopeType switch
        {
            AuthorityScopeType.Project => $"Project ({ContractRoot.ProjectCode})",
            AuthorityScopeType.Section => Sections.SingleOrDefault(section => section.SectionId == scopeId) is { } section
                ? $"Section: {section.Title}"
                : $"Section: {scopeId}",
            AuthorityScopeType.TestItem => Sections.SelectMany(s => s.TestItems).SingleOrDefault(testItem => testItem.TestItemId == scopeId) is { } testItem
                ? $"Test: {testItem.TestReference} - {testItem.TestTitle}"
                : $"Test: {scopeId}",
            AuthorityScopeType.Evidence => $"Evidence: {scopeId}",
            _ => scopeId.ToString()
        };
    }

    private void EnsureScopeExists(AuthorityScopeType scopeType, Guid scopeId)
    {
        switch (scopeType)
        {
            case AuthorityScopeType.Project:
                if (scopeId != ProjectId)
                {
                    throw new InvalidOperationException("Project-scoped authority must use the current project id.");
                }
                break;
            case AuthorityScopeType.Section:
                if (Sections.All(section => section.SectionId != scopeId))
                {
                    throw new InvalidOperationException("Section scope id does not match any section in the project.");
                }
                break;
            case AuthorityScopeType.TestItem:
                if (Sections.SelectMany(s => s.TestItems).All(testItem => testItem.TestItemId != scopeId))
                {
                    throw new InvalidOperationException("Test item scope id does not match any test item in the project.");
                }
                break;
            case AuthorityScopeType.Evidence:
                if (Sections.SelectMany(s => s.TestItems).SelectMany(testItem => testItem.EvidenceRecords).All(record => record.EvidenceId != scopeId))
                {
                    throw new InvalidOperationException("Evidence scope id does not match any evidence in the project.");
                }
                break;
        }
    }

    private static bool IsCriticalRole(AuthorityRole role)
    {
        return role is AuthorityRole.ContractAuthor
            or AuthorityRole.LeadTestEngineer
            or AuthorityRole.ReleaseAuthority
            or AuthorityRole.FinalApprovalAuthority;
    }

    private bool IsSoleActiveBearer(
        Guid userId,
        AuthorityRole role,
        AuthorityScopeType scopeType,
        Guid scopeId)
    {
        return AuthorityAssignments.Count(assignment =>
            assignment.IsActive &&
            assignment.UserId != userId &&
            assignment.Role == role &&
            assignment.ScopeType == scopeType &&
            assignment.ScopeId == scopeId) == 0;
    }

    private UserAccount FindUser(Guid userId)
    {
        return Users.SingleOrDefault(user => user.UserId == userId)
            ?? throw new InvalidOperationException("User was not found in the project.");
    }

    private void NormalizeAuthorityModel()
    {
        var actor = string.IsNullOrWhiteSpace(ContractRoot.CreatedBy)
            ? "System Migration"
            : ContractRoot.CreatedBy;
        var at = ContractRoot.CreatedAt == default
            ? DateTimeOffset.UtcNow
            : ContractRoot.CreatedAt;

        EnsureBaselineAuthorityModel(ContractRoot, actor, at);

        foreach (var section in Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.SectionApprover))
            {
                EnsureAuthorityAssignmentForName(
                    section.SectionApprover,
                    AuthorityRole.SectionApprover,
                    AuthorityScopeType.Section,
                    section.SectionId,
                    actor,
                    at,
                    $"Section approver for {section.Title}.");
            }

            EnsureUserForActor(section.CreatedBy, actor, at);

            if (section.Approval is not null)
            {
                EnsureUserForActor(section.Approval.ApprovedBy, actor, at);
            }

            foreach (var asset in section.Assets)
            {
                EnsureUserForActor(asset.AddedBy, actor, at);
            }

            foreach (var testItem in section.TestItems)
            {
                EnsureUserForActor(testItem.CreatedBy, actor, at);
                foreach (var result in testItem.ResultHistory)
                {
                    EnsureUserForActor(result.ExecutedBy, actor, at);
                }

                foreach (var evidence in testItem.EvidenceRecords)
                {
                    EnsureUserForActor(evidence.AttachedBy, actor, at);
                }
            }
        }

        foreach (var asset in Assets)
        {
            EnsureUserForActor(asset.CreatedBy, actor, at);
        }

        if (ReleaseRecord is not null)
        {
            EnsureUserForActor(ReleaseRecord.ReleasedBy, actor, at);
        }

        foreach (var auditEntry in AuditLog)
        {
            EnsureUserForActor(auditEntry.Actor, actor, at);
        }
    }

    private void EnsureBaselineAuthorityModel(ContractRoot contractRoot, string actor, DateTimeOffset at)
    {
        EnsureAuthorityAssignmentForName(
            contractRoot.CreatedBy,
            AuthorityRole.ContractAuthor,
            AuthorityScopeType.Project,
            ProjectId,
            actor,
            at,
            "Contract root author.");

        EnsureAuthorityAssignmentForName(
            contractRoot.LeadTestEngineer,
            AuthorityRole.LeadTestEngineer,
            AuthorityScopeType.Project,
            ProjectId,
            actor,
            at,
            "Accountable FAT lead.");

        EnsureAuthorityAssignmentForName(
            contractRoot.LeadTestEngineer,
            AuthorityRole.TestExecutor,
            AuthorityScopeType.Project,
            ProjectId,
            actor,
            at,
            "Lead Test Engineer can execute tests.");

        EnsureAuthorityAssignmentForName(
            contractRoot.ReleaseAuthority,
            AuthorityRole.ReleaseAuthority,
            AuthorityScopeType.Project,
            ProjectId,
            actor,
            at,
            "Project release authority.");

        EnsureAuthorityAssignmentForName(
            contractRoot.FinalApprovalAuthority,
            AuthorityRole.FinalApprovalAuthority,
            AuthorityScopeType.Project,
            ProjectId,
            actor,
            at,
            "Final approval authority.");
    }

    private AuthorityAssignment? EnsureAuthorityAssignmentForName(
        string? displayName,
        AuthorityRole role,
        AuthorityScopeType scopeType,
        Guid scopeId,
        string actor,
        DateTimeOffset at,
        string? reason)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        var user = EnsureUserAccount(
            displayName,
            email: null,
            phone: null,
            organisation: null,
            createdBy: actor,
            createdAt: at);

        return EnsureAuthorityAssignment(user, role, scopeType, scopeId, actor, at, reason);
    }

    private AuthorityAssignment EnsureAuthorityAssignment(
        UserAccount user,
        AuthorityRole role,
        AuthorityScopeType scopeType,
        Guid scopeId,
        string actor,
        DateTimeOffset at,
        string? reason)
    {
        var existing = AuthorityAssignments.FirstOrDefault(assignment =>
            assignment.IsActive &&
            assignment.UserId == user.UserId &&
            assignment.Role == role &&
            assignment.ScopeType == scopeType &&
            assignment.ScopeId == scopeId);

        if (existing is not null)
        {
            return existing;
        }

        var assignment = AuthorityAssignment.Create(
            Guid.NewGuid(),
            user,
            role,
            scopeType,
            scopeId,
            actor,
            at,
            reason);
        AuthorityAssignments.Add(assignment);
        return assignment;
    }

    private UserAccount EnsureUserAccount(
        string displayName,
        string? email,
        string? phone,
        string? organisation,
        string createdBy,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new InvalidOperationException("User display name is required.");
        }

        var normalizedName = displayName.Trim();
        var existing = Users.FirstOrDefault(user =>
            string.Equals(user.DisplayName.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return existing;
        }

        var user = UserAccount.Create(
            Guid.NewGuid(),
            normalizedName,
            email,
            phone,
            organisation,
            createdBy,
            createdAt);
        Users.Add(user);
        return user;
    }

    private void EnsureUserForActor(string? actor, string createdBy, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return;
        }

        EnsureUserAccount(actor, null, null, null, createdBy, createdAt);
    }

    private void EnsureStateAllowsWorkspaceStructure()
    {
        if (State != ProjectState.DraftContractBuilt)
        {
            throw new InvalidOperationException("Structure can only be changed while the project is in draft contract.");
        }
    }

    private void EnsureStateAllowsApplicabilityChange()
    {
        if (State is not ProjectState.DraftContractBuilt and not ProjectState.Executable)
        {
            throw new InvalidOperationException("Applicability can only be changed before project release.");
        }
    }

    private void EnsureExecutable()
    {
        if (State != ProjectState.Executable)
        {
            throw new InvalidOperationException("Test execution operations require the project to be ready for execution.");
        }
    }

    private void EnsureProjectMetadataEditable(ReportingMetadata requestedMetadata)
    {
        if (State == ProjectState.Released)
        {
            throw new InvalidOperationException("Project metadata cannot be changed after release.");
        }

        if (State == ProjectState.Executable && !ReportingMetadata.ExecutionContextMatches(requestedMetadata))
        {
            throw new InvalidOperationException(
                "Machine context is frozen once the project is opened for execution. Only reporting and contact details can still be edited until release.");
        }
    }

    private Section FindSection(Guid sectionId)
    {
        return Sections.SingleOrDefault(s => s.SectionId == sectionId)
            ?? throw new InvalidOperationException("Section was not found.");
    }

    private Asset FindAsset(Guid assetId)
    {
        return Assets.SingleOrDefault(asset => asset.AssetId == assetId)
            ?? throw new InvalidOperationException("Asset was not found.");
    }

    private TestItem FindTestItem(Guid testItemId)
    {
        return Sections
            .SelectMany(s => s.TestItems)
            .SingleOrDefault(t => t.TestItemId == testItemId)
            ?? throw new InvalidOperationException("Test item was not found.");
    }

    private (Section Section, TestItem TestItem) FindSectionAndTestItem(Guid testItemId)
    {
        foreach (var section in Sections)
        {
            var testItem = section.TestItems.SingleOrDefault(testItem => testItem.TestItemId == testItemId);
            if (testItem is not null)
            {
                return (section, testItem);
            }
        }

        throw new InvalidOperationException("Test item was not found.");
    }

    private bool AssetIsAvailableInSection(Section section, Asset asset)
    {
        var topLevelAssetId = asset.ParentAssetId ?? asset.AssetId;
        return section.Assets.Any(sectionAsset => sectionAsset.AssetId == topLevelAssetId);
    }

    private bool IsTestApplicable(Section section, TestItem testItem)
    {
        if (section.Applicability == ApplicabilityState.NotApplicable)
        {
            return false;
        }

        if (SectionAssetForTest(section, testItem)?.Applicability == ApplicabilityState.NotApplicable)
        {
            return false;
        }

        return testItem.Applicability == ApplicabilityState.Applicable;
    }

    private SectionAsset? SectionAssetForTest(Section section, TestItem testItem)
    {
        if (testItem.AssetId is null)
        {
            return null;
        }

        var topLevelAssetId = TopLevelAssetId(testItem.AssetId.Value);
        return section.Assets.SingleOrDefault(asset => asset.AssetId == topLevelAssetId);
    }

    private AuthorityRole SectionApprovalRoleFor(Section section, string approvedBy)
    {
        return !string.IsNullOrWhiteSpace(section.SectionApprover) &&
            AuthorityMatches(approvedBy, section.SectionApprover)
            ? AuthorityRole.SectionApprover
            : AuthorityRole.ReleaseAuthority;
    }

    private string ApplicabilityAuditDetails(string label, ApplicabilityState applicability, string? reason)
    {
        var scopePrefix = State == ProjectState.Executable
            ? "Executable scope decision: "
            : string.Empty;
        return applicability == ApplicabilityState.NotApplicable
            ? $"{scopePrefix}{label} -> NotApplicable. Reason: {reason?.Trim()}"
            : $"{scopePrefix}{label} -> Applicable.";
    }

    private IEnumerable<Asset> TopLevelAssets()
    {
        return Assets.Where(asset => asset.ParentAssetId is null);
    }

    private static bool AssetNameMatches(Asset asset, string candidateName)
    {
        return string.Equals(asset.Name.Trim(), candidateName.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool AuthorityMatches(string actual, string expected)
    {
        return string.Equals(actual.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private void ReindexSections()
    {
        var order = 1;
        foreach (var section in Sections.OrderBy(section => section.DisplayOrder))
        {
            section.SetDisplayOrder(order++);
        }
    }
}
