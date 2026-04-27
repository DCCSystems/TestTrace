using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;
using TestTrace_V1.Persistence;
using TestTrace_V1.UI;
using TestTrace_V1.Workspace;

namespace TestTrace_V1;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Contains("--smoke", StringComparer.OrdinalIgnoreCase))
        {
            RunSmokeCheck();
            return;
        }

        ApplicationConfiguration.Initialize();

        var registry = OperatorRegistry.Load();

        if (registry.Operators.Count == 0)
        {
            using var onboarding = new EditOperatorProfileForm(existing: null, isFirstRun: true);
            if (onboarding.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                var profile = OperatorProfile.Create(
                    onboarding.DisplayName,
                    onboarding.JobRole,
                    onboarding.Email,
                    onboarding.Phone,
                    onboarding.Organisation,
                    DateTimeOffset.UtcNow);
                registry.Add(profile);
                registry.Save();
                OperatorSession.SignIn(profile, registry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TestTrace", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        else
        {
            using var picker = new OperatorSelectionForm(registry);
            if (picker.ShowDialog() != DialogResult.OK || picker.SelectedOperator is null)
            {
                return;
            }
            OperatorSession.SignIn(picker.SelectedOperator, registry);
        }

        Application.Run(new MainForm());
    }

    private static void RunSmokeCheck()
    {
        var root = Path.Combine(Path.GetTempPath(), "TestTraceSmoke_" + Guid.NewGuid().ToString("N"));
        try
        {
            var repository = new JsonProjectRepository();
            var service = new BuildContractService(repository);
            var buildResult = service.Build(new BuildContractRequest
            {
                ProjectsRootDirectory = root,
                ProjectType = "FAT",
                ProjectName = "Xtrutech Pilot FAT",
                ProjectCode = "FAT-2026-014",
                MachineModel = "Pilot Machine",
                MachineSerialNumber = "SN-001",
                CustomerName = "Xtrutech",
                ScopeNarrative = "Factory Acceptance Test pilot for governed TestTrace execution.",
                LeadTestEngineer = "Dan Chetwyn",
                ReleaseAuthority = "Dan Chetwyn",
                FinalApprovalAuthority = "Dan Chetwyn",
                CreatedBy = "Smoke Test",
                ReportingMetadata = new ReportingMetadata
                {
                    ProjectStartDate = new DateOnly(2026, 4, 23),
                    CustomerAddress = "5 The Road",
                    CustomerCountry = "United Kingdom",
                    CustomerProjectReference = "XT-REF-001",
                    SiteContactName = "Andy Davis",
                    SiteContactEmail = "andy@example.com",
                    SiteContactPhone = "01234 567890",
                    MachineConfigurationSpecification = "Pilot extrusion line with encoder feedback and guard interlock verification.",
                    ControlPlatform = "Siemens",
                    MachineRoleApplication = "Extruder",
                    SoftwareVersion = "v1.0",
                    LeadTestEngineerEmail = "dan@example.com",
                    LeadTestEngineerPhone = "07123 456789"
                }
            });

            if (!buildResult.Succeeded)
            {
                FailSmoke(buildResult.ErrorMessage, buildResult.Validation.Issues);
                return;
            }

            if (buildResult.Project is null || buildResult.Project.State != ProjectState.DraftContractBuilt)
            {
                FailSmoke("project was not created in DraftContractBuilt state.");
                return;
            }

            if (string.IsNullOrWhiteSpace(buildResult.ProjectFolderPath) || string.IsNullOrWhiteSpace(buildResult.ProjectFilePath) || !File.Exists(buildResult.ProjectFilePath))
            {
                FailSmoke("project.testtrace.json was not written.");
                return;
            }

            if (buildResult.Project.SchemaVersion != "1.1" ||
                !buildResult.Project.Users.Any(user => user.DisplayName == "Smoke Test") ||
                !buildResult.Project.Users.Any(user => user.DisplayName == "Dan Chetwyn") ||
                !buildResult.Project.AuthorityAssignments.Any(assignment =>
                    assignment.Role == AuthorityRole.ContractAuthor &&
                    assignment.DisplayNameSnapshot == "Smoke Test") ||
                !buildResult.Project.AuthorityAssignments.Any(assignment =>
                    assignment.Role == AuthorityRole.LeadTestEngineer &&
                    assignment.DisplayNameSnapshot == "Dan Chetwyn") ||
                !buildResult.Project.AuthorityAssignments.Any(assignment =>
                    assignment.Role == AuthorityRole.TestExecutor &&
                    assignment.DisplayNameSnapshot == "Dan Chetwyn") ||
                !buildResult.Project.AuthorityAssignments.Any(assignment =>
                    assignment.Role == AuthorityRole.ReleaseAuthority &&
                    assignment.DisplayNameSnapshot == "Dan Chetwyn") ||
                !buildResult.Project.AuthorityAssignments.Any(assignment =>
                    assignment.Role == AuthorityRole.FinalApprovalAuthority &&
                    assignment.DisplayNameSnapshot == "Dan Chetwyn"))
            {
                FailSmoke("baseline authority users or project assignments were not created.");
                return;
            }

            var workspace = new WorkspaceService(repository);

            var usersAuthority = new UsersAuthorityService(repository);
            var smokeTestUserId = buildResult.Project.Users.Single(user => user.DisplayName == "Smoke Test").UserId;
            var assignSmokeExecutor = usersAuthority.AssignAuthority(new AssignAuthorityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                UserId = smokeTestUserId,
                Role = AuthorityRole.TestExecutor,
                ScopeType = AuthorityScopeType.Project,
                ScopeId = buildResult.Project.ProjectId,
                Reason = "Smoke harness executes test results.",
                Actor = "Smoke Test"
            });
            if (!assignSmokeExecutor.Succeeded || assignSmokeExecutor.TargetId is null)
            {
                FailSmoke(assignSmokeExecutor.ErrorMessage, assignSmokeExecutor.Validation.Issues);
                return;
            }

            var addedUser = usersAuthority.AddUser(new AddUserRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                DisplayName = "Andy Davis",
                Email = "andy@example.com",
                Phone = "01234 567890",
                Organisation = "Xtrutech",
                Actor = "Smoke Test"
            });
            if (!addedUser.Succeeded || addedUser.TargetId is null)
            {
                FailSmoke(addedUser.ErrorMessage, addedUser.Validation.Issues);
                return;
            }
            var andyId = addedUser.TargetId.Value;

            var updatedUser = usersAuthority.UpdateUser(new UpdateUserRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                UserId = andyId,
                DisplayName = "Andy Davis",
                Email = "andy.davis@example.com",
                Phone = "01234 567890",
                Organisation = "Xtrutech Ltd",
                Actor = "Smoke Test"
            });
            if (!updatedUser.Succeeded ||
                updatedUser.Project!.Users.Single(user => user.UserId == andyId).Organisation != "Xtrutech Ltd")
            {
                FailSmoke("update user did not persist new organisation.");
                return;
            }

            var deactivateAuthor = usersAuthority.DeactivateUser(new DeactivateUserRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                UserId = updatedUser.Project!.Users.Single(user => user.DisplayName == "Smoke Test").UserId,
                Reason = "Trying to deactivate sole contract author for smoke guard.",
                Actor = "Dan Chetwyn"
            });
            if (deactivateAuthor.Succeeded)
            {
                FailSmoke("guard failed: deactivating the sole ContractAuthor should be rejected.");
                return;
            }

            var deactivateAndy = usersAuthority.DeactivateUser(new DeactivateUserRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                UserId = andyId,
                Reason = "Smoke deactivation - Andy is no longer on the project.",
                Actor = "Smoke Test"
            });
            if (!deactivateAndy.Succeeded)
            {
                FailSmoke(deactivateAndy.ErrorMessage, deactivateAndy.Validation.Issues);
                return;
            }
            if (deactivateAndy.Project!.Users.Single(user => user.UserId == andyId).IsActive)
            {
                FailSmoke("Andy was not actually deactivated.");
                return;
            }

            var reactivateAndy = usersAuthority.ReactivateUser(new ReactivateUserRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                UserId = andyId,
                Actor = "Smoke Test"
            });
            if (!reactivateAndy.Succeeded || !reactivateAndy.Project!.Users.Single(user => user.UserId == andyId).IsActive)
            {
                FailSmoke("Andy was not reactivated.");
                return;
            }

            var assignAndyWitness = usersAuthority.AssignAuthority(new AssignAuthorityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                UserId = andyId,
                Role = AuthorityRole.Witness,
                ScopeType = AuthorityScopeType.Project,
                ScopeId = buildResult.Project.ProjectId,
                Reason = "Andy witnesses smoke execution results.",
                Actor = "Smoke Test"
            });
            if (!assignAndyWitness.Succeeded || assignAndyWitness.TargetId is null)
            {
                FailSmoke(assignAndyWitness.ErrorMessage, assignAndyWitness.Validation.Issues);
                return;
            }

            var updateMetadata = workspace.UpdateProjectMetadata(new UpdateProjectMetadataRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Actor = "Smoke Test",
                Metadata = new ReportingMetadata
                {
                    ProjectStartDate = new DateOnly(2026, 4, 24),
                    CustomerAddress = "5 The Road, Here, There, Everywhere",
                    CustomerCountry = "United Kingdom",
                    CustomerProjectReference = "XT-REF-001-A",
                    SiteContactName = "Andy Davis",
                    SiteContactEmail = "andy.davis@example.com",
                    SiteContactPhone = "01234 567890",
                    MachineConfigurationSpecification = "Pilot extrusion line with updated machine description for the smoke path.",
                    ControlPlatform = "Siemens",
                    MachineRoleApplication = "Extruder with Ram Feeder",
                    SoftwareVersion = "v1.0.1",
                    LeadTestEngineerEmail = "dan.chetwyn@example.com",
                    LeadTestEngineerPhone = "07123 456789"
                }
            });

            if (!updateMetadata.Succeeded)
            {
                FailSmoke(updateMetadata.ErrorMessage, updateMetadata.Validation.Issues);
                return;
            }

            var earlyOpen = workspace.OpenForExecution(new OpenForExecutionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Actor = "Smoke Test"
            });

            if (earlyOpen.Succeeded)
            {
                FailSmoke("project opened for execution without section/test guards.");
                return;
            }

            var sectionResult = workspace.AddSection(new AddSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Title = "Mechanical Safety",
                Description = "Core mechanical acceptance checks.",
                SectionApprover = "Dan Chetwyn",
                Actor = "Smoke Test"
            });

            if (!sectionResult.Succeeded || sectionResult.TargetId is null)
            {
                FailSmoke(sectionResult.ErrorMessage, sectionResult.Validation.Issues);
                return;
            }

            var assignSectionApprover = usersAuthority.AssignAuthority(new AssignAuthorityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                UserId = andyId,
                Role = AuthorityRole.SectionApprover,
                ScopeType = AuthorityScopeType.Section,
                ScopeId = sectionResult.TargetId.Value,
                Reason = "Andy will sign off mechanical safety section.",
                Actor = "Smoke Test"
            });
            if (!assignSectionApprover.Succeeded || assignSectionApprover.TargetId is null)
            {
                FailSmoke(assignSectionApprover.ErrorMessage, assignSectionApprover.Validation.Issues);
                return;
            }
            var sectionApproverAssignmentId = assignSectionApprover.TargetId.Value;

            var revokeAuthor = usersAuthority.RevokeAuthority(new RevokeAuthorityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                AssignmentId = assignSectionApprover.Project!.AuthorityAssignments
                    .Single(assignment => assignment.Role == AuthorityRole.ContractAuthor &&
                                          assignment.IsActive).AssignmentId,
                Reason = "Trying to revoke sole ContractAuthor for smoke guard.",
                Actor = "Dan Chetwyn"
            });
            if (revokeAuthor.Succeeded)
            {
                FailSmoke("guard failed: revoking the sole ContractAuthor should be rejected.");
                return;
            }

            var revokeSectionApprover = usersAuthority.RevokeAuthority(new RevokeAuthorityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                AssignmentId = sectionApproverAssignmentId,
                Reason = "Smoke revocation - reassigning section sign-off.",
                Actor = "Smoke Test"
            });
            if (!revokeSectionApprover.Succeeded ||
                revokeSectionApprover.Project!.AuthorityAssignments
                    .Single(assignment => assignment.AssignmentId == sectionApproverAssignmentId).IsActive)
            {
                FailSmoke("section approver authority was not revoked.");
                return;
            }

            var componentResult = workspace.AddComponentToSection(new AddComponentToSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ComponentName = "Extruder Motor",
                ComponentDescription = "Reusable machine component across disciplines.",
                Actor = "Smoke Test"
            });

            if (!componentResult.Succeeded || componentResult.TargetId is null)
            {
                FailSmoke(componentResult.ErrorMessage, componentResult.Validation.Issues);
                return;
            }

            var subAssetResult = workspace.AddSubAsset(new AddSubAssetRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                ParentAssetId = componentResult.TargetId.Value,
                AssetName = "Encoder",
                AssetType = "Sensor",
                Manufacturer = "Baumer",
                Model = "EIL580",
                SerialNumber = "ENC-001",
                Notes = "Mounted to extruder drive motor.",
                Actor = "Smoke Test"
            });

            if (!subAssetResult.Succeeded || subAssetResult.TargetId is null)
            {
                FailSmoke(subAssetResult.ErrorMessage, subAssetResult.Validation.Issues);
                return;
            }

            var temporarySubAsset = workspace.AddSubAsset(new AddSubAssetRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                ParentAssetId = componentResult.TargetId.Value,
                AssetName = "Temporary Sensor",
                AssetType = "Sensor",
                Actor = "Smoke Test"
            });

            if (!temporarySubAsset.Succeeded || temporarySubAsset.TargetId is null)
            {
                FailSmoke(temporarySubAsset.ErrorMessage, temporarySubAsset.Validation.Issues);
                return;
            }

            var deleteTemporarySubAsset = workspace.DeleteAsset(new DeleteAssetRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                AssetId = temporarySubAsset.TargetId.Value,
                Actor = "Smoke Test"
            });

            if (!deleteTemporarySubAsset.Succeeded)
            {
                FailSmoke(deleteTemporarySubAsset.ErrorMessage, deleteTemporarySubAsset.Validation.Issues);
                return;
            }

            var l1VoltageInputId = Guid.NewGuid();
            var l2VoltageInputId = Guid.NewGuid();
            var l3VoltageInputId = Guid.NewGuid();
            var rotationDirectionInputId = Guid.NewGuid();
            var pulseCountInputId = Guid.NewGuid();

            var testResult = workspace.AddTestItem(new AddTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                AssetId = componentResult.TargetId.Value,
                TestReference = "T-001",
                TestTitle = "Guard interlock check",
                TestDescription = "Confirm guard interlock stops machine motion.",
                ExpectedOutcome = "Machine motion stops when guard opens.",
                AcceptanceCriteria = AcceptanceCriteria.NumericRange(1450, 50, "RPM"),
                EvidenceRequirements = EvidenceRequirements.Create(
                    photoRequired: true,
                    measurementRequired: true,
                    signatureRequired: false,
                    fileUploadRequired: true,
                    commentRequiredOnFail: true,
                    commentAlwaysRequired: false),
                BehaviourRules = TestBehaviourRules.Create(
                    blockProgressionIfFailed: true,
                    allowOverrideWithReason: false,
                    requiresWitness: true),
                TestInputs =
                [
                    TestInput.Create(l1VoltageInputId, "L1 Voltage", TestInputType.Numeric, 400, 10, "V", true, 1),
                    TestInput.Create(l2VoltageInputId, "L2 Voltage", TestInputType.Numeric, 400, 10, "V", true, 2),
                    TestInput.Create(l3VoltageInputId, "L3 Voltage", TestInputType.Numeric, 400, 10, "V", true, 3),
                    TestInput.Create(rotationDirectionInputId, "Rotation Direction", TestInputType.Boolean, null, null, null, true, 4)
                ],
                Actor = "Smoke Test"
            });

            if (!testResult.Succeeded || testResult.TargetId is null)
            {
                FailSmoke(testResult.ErrorMessage, testResult.Validation.Issues);
                return;
            }

            var subAssetTestResult = workspace.AddTestItem(new AddTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                AssetId = subAssetResult.TargetId.Value,
                TestReference = "T-002",
                TestTitle = "Encoder signal check",
                TestDescription = "Confirm encoder produces a stable pulse train at low speed.",
                ExpectedOutcome = "Signal remains stable without dropout.",
                AcceptanceCriteria = AcceptanceCriteria.ManualConfirmation(),
                EvidenceRequirements = EvidenceRequirements.Create(
                    photoRequired: false,
                    measurementRequired: true,
                    signatureRequired: false,
                    fileUploadRequired: false,
                    commentRequiredOnFail: true,
                    commentAlwaysRequired: false),
                BehaviourRules = TestBehaviourRules.Default(),
                TestInputs =
                [
                    TestInput.Create(pulseCountInputId, "Pulse Count", TestInputType.Numeric, 1024, 2, "ppr", true, 1)
                ],
                Actor = "Smoke Test"
            });

            if (!subAssetTestResult.Succeeded || subAssetTestResult.TargetId is null)
            {
                FailSmoke(subAssetTestResult.ErrorMessage, subAssetTestResult.Validation.Issues);
                return;
            }

            var notApplicableTestResult = workspace.AddTestItem(new AddTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                AssetId = componentResult.TargetId.Value,
                TestReference = "T-003",
                TestTitle = "Optional inverter supplied check",
                TestDescription = "Check optional inverter package when supplied.",
                ExpectedOutcome = "Optional inverter package is present when in scope.",
                Actor = "Smoke Test"
            });

            if (!notApplicableTestResult.Succeeded || notApplicableTestResult.TargetId is null)
            {
                FailSmoke(notApplicableTestResult.ErrorMessage, notApplicableTestResult.Validation.Issues);
                return;
            }

            var simpleTestResult = workspace.AddTestItem(new AddTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                AssetId = componentResult.TargetId.Value,
                TestReference = "VIS-NP-001",
                TestTitle = "Nameplate check",
                TestDescription = "Confirm the motor nameplate is present and legible.",
                ExpectedOutcome = "Nameplate details can be read.",
                Actor = "Smoke Test"
            });

            if (!simpleTestResult.Succeeded || simpleTestResult.TargetId is null)
            {
                FailSmoke(simpleTestResult.ErrorMessage, simpleTestResult.Validation.Issues);
                return;
            }

            var overrideFailTestResult = workspace.AddTestItem(new AddTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                AssetId = componentResult.TargetId.Value,
                TestReference = "OVR-001",
                TestTitle = "Documented exception check",
                TestDescription = "Confirm failed condition can be accepted only by an explicit override reason.",
                ExpectedOutcome = "Failure is either resolved or accepted by controlled exception.",
                AcceptanceCriteria = AcceptanceCriteria.ManualConfirmation(),
                EvidenceRequirements = EvidenceRequirements.None(),
                BehaviourRules = TestBehaviourRules.Create(
                    blockProgressionIfFailed: true,
                    allowOverrideWithReason: true,
                    requiresWitness: false),
                Actor = "Smoke Test"
            });

            if (!overrideFailTestResult.Succeeded || overrideFailTestResult.TargetId is null)
            {
                FailSmoke(overrideFailTestResult.ErrorMessage, overrideFailTestResult.Validation.Issues);
                return;
            }

            var missingReasonApplicability = workspace.SetTestApplicability(new SetTestApplicabilityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = notApplicableTestResult.TargetId.Value,
                Applicability = ApplicabilityState.NotApplicable,
                Actor = "Smoke Test"
            });

            if (missingReasonApplicability.Succeeded)
            {
                FailSmoke("test was marked not applicable without a reason.");
                return;
            }

            var markTestNotApplicable = workspace.SetTestApplicability(new SetTestApplicabilityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = notApplicableTestResult.TargetId.Value,
                Applicability = ApplicabilityState.NotApplicable,
                Reason = "Optional inverter package is not supplied on this machine.",
                Actor = "Smoke Test"
            });

            if (!markTestNotApplicable.Succeeded)
            {
                FailSmoke(markTestNotApplicable.ErrorMessage, markTestNotApplicable.Validation.Issues);
                return;
            }

            var notApplicableSection = workspace.AddSection(new AddSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Title = "Machine Options Not Supplied",
                Description = "Scope placeholder for options excluded from this FAT.",
                SectionApprover = "Dan Chetwyn",
                Actor = "Smoke Test"
            });

            if (!notApplicableSection.Succeeded || notApplicableSection.TargetId is null)
            {
                FailSmoke(notApplicableSection.ErrorMessage, notApplicableSection.Validation.Issues);
                return;
            }

            var markSectionNotApplicable = workspace.SetSectionApplicability(new SetSectionApplicabilityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = notApplicableSection.TargetId.Value,
                Applicability = ApplicabilityState.NotApplicable,
                Reason = "These options are not supplied on this machine.",
                Actor = "Smoke Test"
            });

            if (!markSectionNotApplicable.Succeeded)
            {
                FailSmoke(markSectionNotApplicable.ErrorMessage, markSectionNotApplicable.Validation.Issues);
                return;
            }

            var assetNotApplicableSection = workspace.AddSection(new AddSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Title = "Optional Electrical Package",
                Description = "Electrical tests for optional equipment.",
                SectionApprover = "Dan Chetwyn",
                Actor = "Smoke Test"
            });

            if (!assetNotApplicableSection.Succeeded || assetNotApplicableSection.TargetId is null)
            {
                FailSmoke(assetNotApplicableSection.ErrorMessage, assetNotApplicableSection.Validation.Issues);
                return;
            }

            var assignAssetToOptionalSection = workspace.AssignAssetToSection(new AssignAssetToSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = assetNotApplicableSection.TargetId.Value,
                AssetId = componentResult.TargetId.Value,
                Actor = "Smoke Test"
            });

            if (!assignAssetToOptionalSection.Succeeded)
            {
                FailSmoke(assignAssetToOptionalSection.ErrorMessage, assignAssetToOptionalSection.Validation.Issues);
                return;
            }

            var optionalElectricalTest = workspace.AddTestItem(new AddTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = assetNotApplicableSection.TargetId.Value,
                AssetId = componentResult.TargetId.Value,
                TestReference = "E-001",
                TestTitle = "Optional inverter supply voltage",
                TestDescription = "Record supply voltage for optional inverter package.",
                ExpectedOutcome = "Supply is within the configured inverter tolerance.",
                Actor = "Smoke Test"
            });

            if (!optionalElectricalTest.Succeeded || optionalElectricalTest.TargetId is null)
            {
                FailSmoke(optionalElectricalTest.ErrorMessage, optionalElectricalTest.Validation.Issues);
                return;
            }

            var markAssetNotApplicable = workspace.SetAssetApplicability(new SetAssetApplicabilityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = assetNotApplicableSection.TargetId.Value,
                AssetId = componentResult.TargetId.Value,
                Applicability = ApplicabilityState.NotApplicable,
                Reason = "Optional inverter package is excluded from this FAT.",
                Actor = "Smoke Test"
            });

            if (!markAssetNotApplicable.Succeeded)
            {
                FailSmoke(markAssetNotApplicable.ErrorMessage, markAssetNotApplicable.Validation.Issues);
                return;
            }

            var renameSection = workspace.RenameSection(new RenameSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                Title = "Mechanical Safety Checks",
                Actor = "Smoke Test"
            });

            if (!renameSection.Succeeded)
            {
                FailSmoke(renameSection.ErrorMessage, renameSection.Validation.Issues);
                return;
            }

            var renameComponent = workspace.RenameAsset(new RenameAssetRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                AssetId = componentResult.TargetId.Value,
                AssetName = "Extruder Drive Motor",
                Actor = "Smoke Test"
            });

            if (!renameComponent.Succeeded)
            {
                FailSmoke(renameComponent.ErrorMessage, renameComponent.Validation.Issues);
                return;
            }

            var renameTest = workspace.RenameTestItem(new RenameTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                TestReference = "T-001A",
                TestTitle = "Guard interlock stop check",
                Actor = "Smoke Test"
            });

            if (!renameTest.Succeeded)
            {
                FailSmoke(renameTest.ErrorMessage, renameTest.Validation.Issues);
                return;
            }

            var temporaryTest = workspace.AddTestItem(new AddTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ComponentId = componentResult.TargetId.Value,
                TestReference = "T-999",
                TestTitle = "Temporary draft check",
                Actor = "Smoke Test"
            });

            if (!temporaryTest.Succeeded || temporaryTest.TargetId is null)
            {
                FailSmoke(temporaryTest.ErrorMessage, temporaryTest.Validation.Issues);
                return;
            }

            var deleteTemporaryTest = workspace.DeleteTestItem(new DeleteTestItemRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = temporaryTest.TargetId.Value,
                Actor = "Smoke Test"
            });

            if (!deleteTemporaryTest.Succeeded)
            {
                FailSmoke(deleteTemporaryTest.ErrorMessage, deleteTemporaryTest.Validation.Issues);
                return;
            }

            var temporaryComponent = workspace.AddComponentToSection(new AddComponentToSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ComponentName = "Temporary Draft Component",
                Actor = "Smoke Test"
            });

            if (!temporaryComponent.Succeeded || temporaryComponent.TargetId is null)
            {
                FailSmoke(temporaryComponent.ErrorMessage, temporaryComponent.Validation.Issues);
                return;
            }

            var removeTemporaryComponent = workspace.RemoveComponentFromSection(new RemoveComponentFromSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ComponentId = temporaryComponent.TargetId.Value,
                Actor = "Smoke Test"
            });

            if (!removeTemporaryComponent.Succeeded)
            {
                FailSmoke(removeTemporaryComponent.ErrorMessage, removeTemporaryComponent.Validation.Issues);
                return;
            }

            var temporarySection = workspace.AddSection(new AddSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Title = "Temporary Draft Section",
                Actor = "Smoke Test"
            });

            if (!temporarySection.Succeeded || temporarySection.TargetId is null)
            {
                FailSmoke(temporarySection.ErrorMessage, temporarySection.Validation.Issues);
                return;
            }

            var deleteTemporarySection = workspace.DeleteSection(new DeleteSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = temporarySection.TargetId.Value,
                Actor = "Smoke Test"
            });

            if (!deleteTemporarySection.Succeeded)
            {
                FailSmoke(deleteTemporarySection.ErrorMessage, deleteTemporarySection.Validation.Issues);
                return;
            }

            var openResult = workspace.OpenForExecution(new OpenForExecutionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Actor = "Smoke Test"
            });

            if (!openResult.Succeeded)
            {
                FailSmoke(openResult.ErrorMessage, openResult.Validation.Issues);
                return;
            }

            var machineContextUpdateAfterOpen = workspace.UpdateProjectMetadata(new UpdateProjectMetadataRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Actor = "Smoke Test",
                Metadata = new ReportingMetadata
                {
                    ProjectStartDate = new DateOnly(2026, 4, 24),
                    CustomerAddress = "5 The Road, Here, There, Everywhere",
                    CustomerCountry = "United Kingdom",
                    CustomerProjectReference = "XT-REF-001-A",
                    SiteContactName = "Andy Davis",
                    SiteContactEmail = "andy.davis@example.com",
                    SiteContactPhone = "01234 567890",
                    MachineConfigurationSpecification = "Changed after execution opened and should be blocked.",
                    ControlPlatform = "Beckhoff",
                    MachineRoleApplication = "Extruder with Secondary Feeder",
                    SoftwareVersion = "v2.0.0",
                    LeadTestEngineerEmail = "dan.chetwyn@example.com",
                    LeadTestEngineerPhone = "07123 456789"
                }
            });

            if (machineContextUpdateAfterOpen.Succeeded)
            {
                FailSmoke("machine context was editable after opening for execution.");
                return;
            }

            var reportingMetadataUpdateAfterOpen = workspace.UpdateProjectMetadata(new UpdateProjectMetadataRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                Actor = "Smoke Test",
                Metadata = new ReportingMetadata
                {
                    ProjectStartDate = new DateOnly(2026, 4, 25),
                    CustomerAddress = "7 Updated Road, Here, There, Everywhere",
                    CustomerCountry = "United Kingdom",
                    CustomerProjectReference = "XT-REF-001-B",
                    SiteContactName = "Andrew Davis",
                    SiteContactEmail = "andrew.davis@example.com",
                    SiteContactPhone = "01987 654321",
                    MachineConfigurationSpecification = "Pilot extrusion line with updated machine description for the smoke path.",
                    ControlPlatform = "Siemens",
                    MachineRoleApplication = "Extruder with Ram Feeder",
                    SoftwareVersion = "v1.0.1",
                    LeadTestEngineerEmail = "dan.updated@example.com",
                    LeadTestEngineerPhone = "07999 888777"
                }
            });

            if (!reportingMetadataUpdateAfterOpen.Succeeded)
            {
                FailSmoke(reportingMetadataUpdateAfterOpen.ErrorMessage, reportingMetadataUpdateAfterOpen.Validation.Issues);
                return;
            }

            var renameAfterOpen = workspace.RenameSection(new RenameSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                Title = "Late Rename",
                Actor = "Smoke Test"
            });

            if (renameAfterOpen.Succeeded)
            {
                FailSmoke("structure rename was allowed after opening for execution.");
                return;
            }

            var execution = new ExecutionService(repository);
            var approval = new ApprovalService(repository);
            var prematureApproval = approval.ApproveSection(new ApproveSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ApprovedBy = "Dan Chetwyn",
                Comments = "This should be blocked because applicable tests are untested."
            });

            if (prematureApproval.Succeeded)
            {
                FailSmoke("section approval was allowed while applicable tests were untested.");
                return;
            }

            var notApplicableResult = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = notApplicableTestResult.TargetId.Value,
                Result = TestResult.Pass,
                Comments = "This should be blocked because the test is not applicable.",
                ExecutedBy = "Smoke Test"
            });

            if (notApplicableResult.Succeeded)
            {
                FailSmoke("result recording was allowed for a not applicable test.");
                return;
            }

            var sourceEvidence = Path.Combine(root, "source-evidence.txt");
            File.WriteAllText(sourceEvidence, "Evidence captured during smoke test.");
            var notApplicableEvidence = execution.AttachEvidence(new AttachEvidenceRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = notApplicableTestResult.TargetId.Value,
                SourceFilePath = sourceEvidence,
                EvidenceType = EvidenceType.Other,
                Description = "This should be blocked because the test is not applicable.",
                AttachedBy = "Smoke Test"
            });

            if (notApplicableEvidence.Succeeded)
            {
                FailSmoke("evidence attachment was allowed for a not applicable test.");
                return;
            }

            var failWithoutComments = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Result = TestResult.Fail,
                ExecutedBy = "Smoke Test"
            });

            if (failWithoutComments.Succeeded)
            {
                FailSmoke("Fail result was allowed without comments when the test requires comment on fail.");
                return;
            }

            var missingStructuredInputs = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Result = TestResult.Pass,
                Comments = "This should be blocked because required structured inputs are missing.",
                WitnessedBy = "Andy Davis",
                ExecutedBy = "Smoke Test"
            });

            if (missingStructuredInputs.Succeeded)
            {
                FailSmoke("structured-input result was allowed without required captured values.");
                return;
            }

            var unauthorizedExecution = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Result = TestResult.Pass,
                Comments = "This should be blocked because Andy has no test executor authority.",
                CapturedInputValues =
                [
                    CapturedTestInputValue.Create(l1VoltageInputId, "400"),
                    CapturedTestInputValue.Create(l2VoltageInputId, "401"),
                    CapturedTestInputValue.Create(l3VoltageInputId, "399"),
                    CapturedTestInputValue.Create(rotationDirectionInputId, "true")
                ],
                ExecutedBy = "Andy Davis"
            });

            if (unauthorizedExecution.Succeeded)
            {
                FailSmoke("result recording was allowed without active TestExecutor authority.");
                return;
            }

            var invalidStructuredInputTypes = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Result = TestResult.Pass,
                Comments = "This should be blocked because captured input types are invalid.",
                CapturedInputValues =
                [
                    CapturedTestInputValue.Create(l1VoltageInputId, "not-a-number"),
                    CapturedTestInputValue.Create(l2VoltageInputId, "401"),
                    CapturedTestInputValue.Create(l3VoltageInputId, "399"),
                    CapturedTestInputValue.Create(rotationDirectionInputId, "maybe")
                ],
                WitnessedBy = "Andy Davis",
                ExecutedBy = "Smoke Test"
            });

            if (invalidStructuredInputTypes.Succeeded)
            {
                FailSmoke("structured-input result was allowed with invalid numeric/boolean captured values.");
                return;
            }

            var resultEntry = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Result = TestResult.Pass,
                MeasuredValue = "Stopped within expected time",
                Comments = "Smoke execution result.",
                CapturedInputValues =
                [
                    CapturedTestInputValue.Create(l1VoltageInputId, "400"),
                    CapturedTestInputValue.Create(l2VoltageInputId, "401"),
                    CapturedTestInputValue.Create(l3VoltageInputId, "399"),
                    CapturedTestInputValue.Create(rotationDirectionInputId, "true")
                ],
                WitnessedBy = "Andy Davis",
                ExecutedBy = "Smoke Test"
            });

            if (!resultEntry.Succeeded || resultEntry.TargetId is null)
            {
                FailSmoke(resultEntry.ErrorMessage, resultEntry.Validation.Issues);
                return;
            }

            var retestEntry = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Result = TestResult.Pass,
                MeasuredValue = "Retested with stable readings",
                Comments = "Structured inputs confirmed on re-test.",
                CapturedInputValues =
                [
                    CapturedTestInputValue.Create(l1VoltageInputId, "401"),
                    CapturedTestInputValue.Create(l2VoltageInputId, "400"),
                    CapturedTestInputValue.Create(l3VoltageInputId, "400"),
                    CapturedTestInputValue.Create(rotationDirectionInputId, "true")
                ],
                SupersedesResultEntryId = resultEntry.TargetId.Value,
                WitnessedBy = "Andy Davis",
                ExecutedBy = "Smoke Test"
            });

            if (!retestEntry.Succeeded || retestEntry.TargetId is null)
            {
                FailSmoke(retestEntry.ErrorMessage, retestEntry.Validation.Issues);
                return;
            }

            var subAssetResultEntry = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = subAssetTestResult.TargetId.Value,
                Result = TestResult.Pass,
                MeasuredValue = "1024 ppr stable",
                Comments = "Encoder signal stable.",
                CapturedInputValues =
                [
                    CapturedTestInputValue.Create(pulseCountInputId, "1024")
                ],
                ExecutedBy = "Smoke Test"
            });

            if (!subAssetResultEntry.Succeeded || subAssetResultEntry.TargetId is null)
            {
                FailSmoke(subAssetResultEntry.ErrorMessage, subAssetResultEntry.Validation.Issues);
                return;
            }

            var simpleResultEntry = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = simpleTestResult.TargetId.Value,
                Result = TestResult.Pass,
                Comments = "Nameplate present and legible.",
                ExecutedBy = "Smoke Test"
            });

            if (!simpleResultEntry.Succeeded || simpleResultEntry.TargetId is null)
            {
                FailSmoke(simpleResultEntry.ErrorMessage, simpleResultEntry.Validation.Issues);
                return;
            }

            var overrideFailWithoutReason = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = overrideFailTestResult.TargetId.Value,
                Result = TestResult.Fail,
                Comments = "This should be blocked at approval because no override reason exists.",
                ExecutedBy = "Smoke Test"
            });

            if (!overrideFailWithoutReason.Succeeded || overrideFailWithoutReason.TargetId is null)
            {
                FailSmoke(overrideFailWithoutReason.ErrorMessage, overrideFailWithoutReason.Validation.Issues);
                return;
            }

            var approvalWithBlockingFailure = approval.ApproveSection(new ApproveSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ApprovedBy = "Dan Chetwyn",
                Comments = "This should be blocked because the failed test has no override reason."
            });

            if (approvalWithBlockingFailure.Succeeded)
            {
                FailSmoke("section approval was allowed while a failed test still blocked progression.");
                return;
            }

            var overrideFailWithReason = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = overrideFailTestResult.TargetId.Value,
                Result = TestResult.Fail,
                Comments = "Failure accepted as a controlled exception for smoke coverage.",
                OverrideReason = "Customer accepted documented deviation for smoke validation.",
                SupersedesResultEntryId = overrideFailWithoutReason.TargetId.Value,
                ExecutedBy = "Smoke Test"
            });

            if (!overrideFailWithReason.Succeeded || overrideFailWithReason.TargetId is null)
            {
                FailSmoke(overrideFailWithReason.ErrorMessage, overrideFailWithReason.Validation.Issues);
                return;
            }

            var notApplicableWithoutComments = execution.RecordResult(new RecordResultRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Result = TestResult.NotApplicable,
                ExecutedBy = "Smoke Test"
            });

            if (notApplicableWithoutComments.Succeeded)
            {
                FailSmoke("NotApplicable result was allowed as a new execution result.");
                return;
            }

            if (!notApplicableWithoutComments.Validation.Issues.Any(issue =>
                    string.Equals(issue.Message, "Use applicability instead.", StringComparison.Ordinal)))
            {
                FailSmoke("NotApplicable result was rejected without the expected applicability guidance.");
                return;
            }

            var wrongEvidenceTypeResult = execution.AttachEvidence(new AttachEvidenceRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                SourceFilePath = sourceEvidence,
                EvidenceType = EvidenceType.Other,
                Description = "Smoke evidence file with the wrong requirement type.",
                AttachedBy = "Smoke Test"
            });

            if (!wrongEvidenceTypeResult.Succeeded || wrongEvidenceTypeResult.TargetId is null)
            {
                FailSmoke(wrongEvidenceTypeResult.ErrorMessage, wrongEvidenceTypeResult.Validation.Issues);
                return;
            }

            var approvalWithMissingEvidence = approval.ApproveSection(new ApproveSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ApprovedBy = "Dan Chetwyn",
                Comments = "This should be blocked because required evidence types are missing."
            });

            if (approvalWithMissingEvidence.Succeeded)
            {
                FailSmoke("section approval was allowed while required evidence types were missing.");
                return;
            }

            var photoEvidenceResult = execution.AttachEvidence(new AttachEvidenceRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                SourceFilePath = sourceEvidence,
                EvidenceType = EvidenceType.Photo,
                Description = "Smoke photo evidence file.",
                AttachedBy = "Smoke Test"
            });

            if (!photoEvidenceResult.Succeeded || photoEvidenceResult.TargetId is null)
            {
                FailSmoke(photoEvidenceResult.ErrorMessage, photoEvidenceResult.Validation.Issues);
                return;
            }

            var measurementEvidenceResult = execution.AttachEvidence(new AttachEvidenceRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                SourceFilePath = sourceEvidence,
                EvidenceType = EvidenceType.Measurement,
                Description = "Smoke measurement evidence file.",
                AttachedBy = "Smoke Test"
            });

            if (!measurementEvidenceResult.Succeeded || measurementEvidenceResult.TargetId is null)
            {
                FailSmoke(measurementEvidenceResult.ErrorMessage, measurementEvidenceResult.Validation.Issues);
                return;
            }

            var fileUploadEvidenceResult = execution.AttachEvidence(new AttachEvidenceRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                SourceFilePath = sourceEvidence,
                EvidenceType = EvidenceType.FileUpload,
                Description = "Smoke file upload evidence file.",
                AttachedBy = "Smoke Test"
            });

            if (!fileUploadEvidenceResult.Succeeded || fileUploadEvidenceResult.TargetId is null)
            {
                FailSmoke(fileUploadEvidenceResult.ErrorMessage, fileUploadEvidenceResult.Validation.Issues);
                return;
            }

            var subAssetMeasurementEvidenceResult = execution.AttachEvidence(new AttachEvidenceRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = subAssetTestResult.TargetId.Value,
                SourceFilePath = sourceEvidence,
                EvidenceType = EvidenceType.Measurement,
                Description = "Smoke encoder measurement evidence file.",
                AttachedBy = "Smoke Test"
            });

            if (!subAssetMeasurementEvidenceResult.Succeeded || subAssetMeasurementEvidenceResult.TargetId is null)
            {
                FailSmoke(subAssetMeasurementEvidenceResult.ErrorMessage, subAssetMeasurementEvidenceResult.Validation.Issues);
                return;
            }

            var markResultedTestNotApplicable = workspace.SetTestApplicability(new SetTestApplicabilityRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                TestItemId = testResult.TargetId.Value,
                Applicability = ApplicabilityState.NotApplicable,
                Reason = "This should be blocked because the test now has result/evidence history.",
                Actor = "Smoke Test"
            });

            if (markResultedTestNotApplicable.Succeeded)
            {
                FailSmoke("test with result/evidence history was marked not applicable.");
                return;
            }

            var loaded = repository.Load(ProjectLocation.FromProjectFolder(buildResult.ProjectFolderPath));
            if (loaded.ProjectId != buildResult.Project.ProjectId || loaded.State != ProjectState.Executable)
            {
                FailSmoke("reloaded project did not preserve ProjectId and Executable state.");
                return;
            }

            if (loaded.ContractRoot.ProjectCode != "FAT-2026-014" || loaded.AuditLog.Count == 0)
            {
                FailSmoke("reloaded contract root or audit log was not preserved.");
                return;
            }

            if (loaded.ReportingMetadata.ProjectStartDate != new DateOnly(2026, 4, 25) ||
                loaded.ReportingMetadata.CustomerAddress != "7 Updated Road, Here, There, Everywhere" ||
                loaded.ReportingMetadata.CustomerProjectReference != "XT-REF-001-B" ||
                loaded.ReportingMetadata.SiteContactName != "Andrew Davis" ||
                loaded.ReportingMetadata.SiteContactEmail != "andrew.davis@example.com" ||
                loaded.ReportingMetadata.SiteContactPhone != "01987 654321" ||
                loaded.ReportingMetadata.LeadTestEngineerEmail != "dan.updated@example.com" ||
                loaded.ReportingMetadata.LeadTestEngineerPhone != "07999 888777")
            {
                FailSmoke("reporting/contact metadata updates made after opening for execution were not persisted.");
                return;
            }

            if (loaded.ReportingMetadata.MachineConfigurationSpecification != "Pilot extrusion line with updated machine description for the smoke path." ||
                loaded.ReportingMetadata.ControlPlatform != "Siemens" ||
                loaded.ReportingMetadata.MachineRoleApplication != "Extruder with Ram Feeder" ||
                loaded.ReportingMetadata.SoftwareVersion != "v1.0.1")
            {
                FailSmoke("machine context changed after execution opened when it should have stayed frozen.");
                return;
            }

            if (loaded.Assets.Count != 3 || loaded.Sections.Count != 3 || loaded.Sections[0].Assets.Count != 1 || loaded.Sections[0].TestItems.Count != 5)
            {
                FailSmoke("asset/section/test structure was not persisted.");
                return;
            }

            if (!loaded.AuditLog.Any(entry => entry.Action == "SetTestApplicability") ||
                !loaded.AuditLog.Any(entry => entry.Action == "SetSectionApplicability") ||
                !loaded.AuditLog.Any(entry => entry.Action == "SetAssetApplicability"))
            {
                FailSmoke("applicability audit entries were not persisted.");
                return;
            }

            if (loaded.SchemaVersion != "1.1" ||
                !loaded.Users.Any(user => user.DisplayName == "Smoke Test") ||
                !loaded.Users.Any(user => user.DisplayName == "Dan Chetwyn") ||
                !loaded.AuthorityAssignments.Any(assignment =>
                    assignment.Role == AuthorityRole.ContractAuthor &&
                    assignment.DisplayNameSnapshot == "Smoke Test") ||
                !loaded.AuthorityAssignments.Any(assignment =>
                    assignment.Role == AuthorityRole.SectionApprover &&
                    assignment.DisplayNameSnapshot == "Dan Chetwyn" &&
                    assignment.ScopeType == AuthorityScopeType.Section &&
                    assignment.ScopeId == sectionResult.TargetId.Value) ||
                loaded.AuditLog.Any(entry => entry.Authority is null || string.IsNullOrWhiteSpace(entry.Authority.DisplayName)))
            {
                FailSmoke("authority users, assignments, or audit stamps were not persisted.");
                return;
            }

            if (loaded.Sections[0].Title != "Mechanical Safety Checks" ||
                loaded.Assets.Single(asset => asset.AssetId == componentResult.TargetId.Value).Name != "Extruder Drive Motor" ||
                loaded.Assets.Single(asset => asset.AssetId == subAssetResult.TargetId.Value).ParentAssetId != componentResult.TargetId.Value ||
                loaded.Assets.Single(asset => asset.AssetId == subAssetResult.TargetId.Value).SerialNumber != "ENC-001" ||
                loaded.Sections[0].TestItems[0].TestReference != "T-001A" ||
                loaded.Sections[0].TestItems[0].TestTitle != "Guard interlock stop check")
            {
                FailSmoke("renamed structure labels were not persisted.");
                return;
            }

            var loadedTestItem = loaded.Sections[0].TestItems[0];
            if (loadedTestItem.AcceptanceCriteria.ConditionType != PassConditionType.NumericRange ||
                loadedTestItem.AcceptanceCriteria.TargetValue != 1450 ||
                loadedTestItem.AcceptanceCriteria.Tolerance != 50 ||
                loadedTestItem.AcceptanceCriteria.Unit != "RPM" ||
                !loadedTestItem.EvidenceRequirements.PhotoRequired ||
                !loadedTestItem.EvidenceRequirements.MeasurementRequired ||
                !loadedTestItem.EvidenceRequirements.FileUploadRequired ||
                !loadedTestItem.EvidenceRequirements.CommentRequiredOnFail ||
                !loadedTestItem.BehaviourRules.BlockProgressionIfFailed ||
                !loadedTestItem.BehaviourRules.RequiresWitness ||
                loadedTestItem.Inputs.Count != 4 ||
                loadedTestItem.Inputs[0].Label != "L1 Voltage" ||
                loadedTestItem.Inputs[0].InputType != TestInputType.Numeric ||
                loadedTestItem.Inputs[0].TargetValue != 400 ||
                loadedTestItem.Inputs[0].Tolerance != 10 ||
                loadedTestItem.Inputs[0].Unit != "V" ||
                loadedTestItem.Inputs[3].Label != "Rotation Direction" ||
                loadedTestItem.Inputs[3].InputType != TestInputType.Boolean)
            {
                FailSmoke("test acceptance, evidence, behaviour, or structured inputs were not persisted.");
                return;
            }

            if (loadedTestItem.ResultHistory.Count != 2 || loadedTestItem.LatestResult != TestResult.Pass)
            {
                FailSmoke("result history was not persisted.");
                return;
            }

            var loadedLatestResult = loadedTestItem.ResultHistory[^1];
            if (loadedLatestResult.CapturedInputValues.Count != 4 ||
                loadedLatestResult.CapturedInputValues.All(value => value.TestInputId != l1VoltageInputId || value.Value != "401") ||
                loadedLatestResult.CapturedInputValues.All(value => value.TestInputId != rotationDirectionInputId || value.Value != "true"))
            {
                FailSmoke("captured structured input values were not persisted on the latest result.");
                return;
            }

            if (loadedLatestResult.Authority is null ||
                loadedLatestResult.Authority.Role != AuthorityRole.TestExecutor ||
                loadedLatestResult.Authority.ScopeType != AuthorityScopeType.TestItem ||
                loadedLatestResult.Authority.ScopeId != testResult.TargetId.Value ||
                loadedLatestResult.Authority.DisplayName != "Smoke Test")
            {
                FailSmoke("result authority stamp was not persisted.");
                return;
            }

            if (loadedLatestResult.WitnessAuthority is null ||
                loadedLatestResult.WitnessAuthority.Role != AuthorityRole.Witness ||
                loadedLatestResult.WitnessAuthority.ScopeType != AuthorityScopeType.TestItem ||
                loadedLatestResult.WitnessAuthority.ScopeId != testResult.TargetId.Value ||
                loadedLatestResult.WitnessedBy != "Andy Davis")
            {
                FailSmoke("witness authority stamp was not persisted on the latest result.");
                return;
            }

            var loadedOverrideFailTest = loaded.Sections[0].TestItems.Single(testItem => testItem.TestItemId == overrideFailTestResult.TargetId.Value);
            if (loadedOverrideFailTest.LatestResult != TestResult.Fail ||
                string.IsNullOrWhiteSpace(loadedOverrideFailTest.ResultHistory[^1].OverrideReason) ||
                loadedOverrideFailTest.LatestFailureBlocksProgression())
            {
                FailSmoke("override-with-reason failure state was not persisted or still blocked progression.");
                return;
            }

            var loadedNotApplicableTest = loaded.Sections[0].TestItems.Single(testItem => testItem.TestItemId == notApplicableTestResult.TargetId.Value);
            if (loadedNotApplicableTest.Applicability != ApplicabilityState.NotApplicable ||
                !string.Equals(loadedNotApplicableTest.ApplicabilityReason, "Optional inverter package is not supplied on this machine.", StringComparison.Ordinal))
            {
                FailSmoke("test applicability state or reason was not persisted.");
                return;
            }

            var loadedSubAssetTestItem = loaded.Sections[0].TestItems.Single(testItem => testItem.AssetId == subAssetResult.TargetId.Value);
            if (loadedSubAssetTestItem.Inputs.Count != 1 ||
                loadedSubAssetTestItem.Inputs[0].Label != "Pulse Count" ||
                loadedSubAssetTestItem.ResultHistory.Count != 1 ||
                loadedSubAssetTestItem.LatestResult != TestResult.Pass ||
                loadedSubAssetTestItem.ResultHistory[0].CapturedInputValues.Count != 1 ||
                loadedSubAssetTestItem.ResultHistory[0].CapturedInputValues[0].Value != "1024")
            {
                FailSmoke("sub-asset test structure, captured input values, or result history was not persisted.");
                return;
            }

            var loadedSimpleTestItem = loaded.Sections[0].TestItems.Single(testItem => testItem.TestItemId == simpleTestResult.TargetId.Value);
            if (loadedSimpleTestItem.Inputs.Count != 0 ||
                loadedSimpleTestItem.ResultHistory.Count != 1 ||
                loadedSimpleTestItem.ResultHistory[0].CapturedInputValues.Count != 0 ||
                loadedSimpleTestItem.LatestResult != TestResult.Pass)
            {
                FailSmoke("simple test execution without structured inputs did not remain compatible.");
                return;
            }

            if (loadedTestItem.EvidenceRecords.Count != 4 ||
                loadedTestItem.EvidenceRecords.All(evidence => evidence.EvidenceType != EvidenceType.Photo) ||
                loadedTestItem.EvidenceRecords.All(evidence => evidence.EvidenceType != EvidenceType.Measurement) ||
                loadedTestItem.EvidenceRecords.All(evidence => evidence.EvidenceType != EvidenceType.FileUpload) ||
                loadedTestItem.EvidenceRecords.All(evidence => evidence.EvidenceType != EvidenceType.Other))
            {
                FailSmoke("typed evidence records were not persisted.");
                return;
            }

            if (loadedSubAssetTestItem.EvidenceRecords.Count != 1 ||
                loadedSubAssetTestItem.EvidenceRecords[0].EvidenceType != EvidenceType.Measurement)
            {
                FailSmoke("sub-asset typed measurement evidence was not persisted.");
                return;
            }

            var loadedEvidenceAuthority = loadedTestItem.EvidenceRecords[0].Authority;
            if (loadedEvidenceAuthority is null ||
                loadedEvidenceAuthority.Role != AuthorityRole.TestExecutor ||
                loadedEvidenceAuthority.ScopeType != AuthorityScopeType.TestItem ||
                loadedEvidenceAuthority.ScopeId != testResult.TargetId.Value ||
                loadedEvidenceAuthority.DisplayName != "Smoke Test")
            {
                FailSmoke("evidence authority stamp was not persisted.");
                return;
            }

            var loadedNotApplicableSection = loaded.Sections.Single(section => section.SectionId == notApplicableSection.TargetId.Value);
            if (loadedNotApplicableSection.Applicability != ApplicabilityState.NotApplicable ||
                !string.Equals(loadedNotApplicableSection.ApplicabilityReason, "These options are not supplied on this machine.", StringComparison.Ordinal))
            {
                FailSmoke("section applicability state or reason was not persisted.");
                return;
            }

            var loadedAssetNotApplicableSection = loaded.Sections.Single(section => section.SectionId == assetNotApplicableSection.TargetId.Value);
            var loadedAssetAssignment = loadedAssetNotApplicableSection.Assets.Single(asset => asset.AssetId == componentResult.TargetId.Value);
            if (loadedAssetAssignment.Applicability != ApplicabilityState.NotApplicable ||
                !string.Equals(loadedAssetAssignment.ApplicabilityReason, "Optional inverter package is excluded from this FAT.", StringComparison.Ordinal))
            {
                FailSmoke("asset applicability state or reason was not persisted.");
                return;
            }

            var storedEvidence = Path.Combine(buildResult.ProjectFolderPath, "evidence", loadedTestItem.EvidenceRecords[0].StoredFileName);
            if (!File.Exists(storedEvidence) || string.IsNullOrWhiteSpace(loadedTestItem.EvidenceRecords[0].Sha256Hash))
            {
                FailSmoke("stored evidence file or hash was missing.");
                return;
            }

            var sectionApproval = approval.ApproveSection(new ApproveSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = sectionResult.TargetId.Value,
                ApprovedBy = "Dan Chetwyn",
                Comments = "Smoke approval."
            });

            if (!sectionApproval.Succeeded || sectionApproval.TargetId is null)
            {
                FailSmoke(sectionApproval.ErrorMessage, sectionApproval.Validation.Issues);
                return;
            }

            var notApplicableSectionApproval = approval.ApproveSection(new ApproveSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = notApplicableSection.TargetId.Value,
                ApprovedBy = "Dan Chetwyn",
                Comments = "Smoke approval for out-of-scope section."
            });

            if (!notApplicableSectionApproval.Succeeded || notApplicableSectionApproval.TargetId is null)
            {
                FailSmoke(notApplicableSectionApproval.ErrorMessage, notApplicableSectionApproval.Validation.Issues);
                return;
            }

            var assetNotApplicableSectionApproval = approval.ApproveSection(new ApproveSectionRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                SectionId = assetNotApplicableSection.TargetId.Value,
                ApprovedBy = "Dan Chetwyn",
                Comments = "Smoke approval for out-of-scope asset assignment."
            });

            if (!assetNotApplicableSectionApproval.Succeeded || assetNotApplicableSectionApproval.TargetId is null)
            {
                FailSmoke(assetNotApplicableSectionApproval.ErrorMessage, assetNotApplicableSectionApproval.Validation.Issues);
                return;
            }

            var wrongReleaseAuthority = approval.ReleaseProject(new ReleaseProjectRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                ReleasedBy = "Someone Else",
                Declaration = "I confirm this project is ready for release."
            });

            if (wrongReleaseAuthority.Succeeded)
            {
                FailSmoke("wrong release authority was allowed to release the project.");
                return;
            }

            var release = approval.ReleaseProject(new ReleaseProjectRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                ReleasedBy = "Dan Chetwyn",
                Declaration = "I confirm this project is ready for release."
            });

            if (!release.Succeeded || release.TargetId is null)
            {
                FailSmoke(release.ErrorMessage, release.Validation.Issues);
                return;
            }

            var released = repository.Load(ProjectLocation.FromProjectFolder(buildResult.ProjectFolderPath));
            if (released.State != ProjectState.Released || released.ReleaseRecord is null)
            {
                FailSmoke("released project state or release record was not persisted.");
                return;
            }

            if (released.Sections.Any(section => section.Approval is null) ||
                !released.AuditLog.Any(entry => entry.Action == "ReleaseProject"))
            {
                FailSmoke("section approvals or final release audit entry were not persisted.");
                return;
            }

            if (released.Sections.Any(section =>
                    section.Approval?.Authority is null ||
                    section.Approval.Authority.Role != AuthorityRole.SectionApprover ||
                    section.Approval.Authority.ScopeType != AuthorityScopeType.Section ||
                    section.Approval.Authority.ScopeId != section.SectionId ||
                    section.Approval.Authority.DisplayName != "Dan Chetwyn") ||
                released.ReleaseRecord.Authority is null ||
                released.ReleaseRecord.Authority.Role != AuthorityRole.ReleaseAuthority ||
                released.ReleaseRecord.Authority.ScopeType != AuthorityScopeType.Project ||
                released.ReleaseRecord.Authority.ScopeId != released.ProjectId ||
                released.ReleaseRecord.Authority.DisplayName != "Dan Chetwyn")
            {
                FailSmoke("approval or release authority stamps were not persisted.");
                return;
            }

            var postReleaseUserMutation = usersAuthority.AddUser(new AddUserRequest
            {
                ProjectFolderPath = buildResult.ProjectFolderPath,
                DisplayName = "Late User",
                Actor = "Dan Chetwyn"
            });
            if (postReleaseUserMutation.Succeeded)
            {
                FailSmoke("released project allowed user or authority model mutation.");
                return;
            }

            var releasedReadiness = ProjectReadinessService.Evaluate(released, released.Sections[0], released.Assets[0], released.Sections[0].TestItems[0]);
            if (!releasedReadiness.IsReleased ||
                releasedReadiness.CanAddSection ||
                releasedReadiness.CanAddComponent ||
                releasedReadiness.CanAddTestItem ||
                releasedReadiness.CanRenameSelection ||
                releasedReadiness.CanDeleteSelection ||
                releasedReadiness.CanRecordResults ||
                releasedReadiness.CanAttachEvidence ||
                releasedReadiness.CanApproveSection ||
                releasedReadiness.CanRelease)
            {
                FailSmoke("released project readiness did not report a read-only state.");
                return;
            }

            var export = new ExportService(repository).ExportFatReport(buildResult.ProjectFolderPath);
            if (!export.Succeeded || string.IsNullOrWhiteSpace(export.FilePath) || !File.Exists(export.FilePath))
            {
                FailSmoke(export.ErrorMessage ?? "FAT report export file was not created.");
                return;
            }

            var exportText = File.ReadAllText(export.FilePath);
            if (!exportText.Contains("Acceptance criteria", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Evidence requirements", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Evidence type", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Photo", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Measurement", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("FileUpload", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Behaviour", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Test inputs", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("L1 Voltage", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Captured inputs", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("L1 Voltage: 401 V", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Rotation Direction: Yes", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Rotation Direction", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Sub-asset", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Encoder", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Pulse Count", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Pulse Count: 1024 ppr", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Applicability", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Applicability reason", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Optional inverter package is not supplied on this machine.", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Optional inverter package is excluded from this FAT.", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Authority Model", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("ContractAuthor", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("SectionApprover", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Authority:", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("TestExecutor", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Witnessed by", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Witness authority", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Override reason", StringComparison.OrdinalIgnoreCase) ||
                !exportText.Contains("Release authority stamp", StringComparison.OrdinalIgnoreCase))
            {
                FailSmoke("FAT report did not include authority, asset hierarchy, applicability, or test acceptance/evidence/behaviour/input fields.");
                return;
            }

            Console.WriteLine("Smoke check passed: project built, structured, executed, evidence-bound, approved, released, exported, and reloaded.");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static void FailSmoke(string? message, IEnumerable<ValidationIssue>? issues = null)
    {
        Console.Error.WriteLine("Smoke check failed: " + (message ?? "unknown error"));
        if (issues is not null)
        {
            foreach (var issue in issues)
            {
                Console.Error.WriteLine($"{issue.TargetField}: {issue.Message}");
            }
        }

        Environment.ExitCode = 1;
    }
}
