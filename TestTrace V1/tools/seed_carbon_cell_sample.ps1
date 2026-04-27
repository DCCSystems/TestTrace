param(
    [string]$ProjectsRoot = "C:\Users\Dan\Desktop\DCC Systems Backup 17.04.26\sandbox-data\TestTraceProjects"
)

$ErrorActionPreference = 'Stop'

function New-IsoTimestamp {
    param([DateTimeOffset]$Value)
    return $Value.ToString("o")
}

function New-AuditEntry {
    param(
        [string]$Action,
        [string]$Actor,
        [DateTimeOffset]$At,
        [string]$TargetType,
        [Guid]$TargetId,
        [string]$Details
    )

    return [ordered]@{
        AuditEntryId = [guid]::NewGuid().ToString()
        Action       = $Action
        Actor        = $Actor
        At           = New-IsoTimestamp $At
        TargetType   = $TargetType
        TargetId     = $TargetId.ToString()
        Details      = $Details
    }
}

function New-TestInput {
    param(
        [Guid]$TestInputId,
        [string]$Label,
        [int]$InputType,
        [decimal]$TargetValue,
        [decimal]$Tolerance,
        [string]$Unit,
        [bool]$Required,
        [int]$DisplayOrder
    )

    return [ordered]@{
        TestInputId  = $TestInputId.ToString()
        Label        = $Label
        InputType    = $InputType
        TargetValue  = $TargetValue
        Tolerance    = $Tolerance
        Unit         = $Unit
        Required     = $Required
        DisplayOrder = $DisplayOrder
    }
}

function New-BooleanInput {
    param(
        [Guid]$TestInputId,
        [string]$Label,
        [bool]$Required,
        [int]$DisplayOrder
    )

    return [ordered]@{
        TestInputId  = $TestInputId.ToString()
        Label        = $Label
        InputType    = 1
        Required     = $Required
        DisplayOrder = $DisplayOrder
    }
}

function New-CapturedInputValue {
    param(
        [Guid]$TestInputId,
        [string]$Value
    )

    return [ordered]@{
        TestInputId = $TestInputId.ToString()
        Value       = $Value
    }
}

function New-ResultEntry {
    param(
        [Guid]$ResultEntryId,
        [int]$Result,
        [string]$MeasuredValue,
        [string]$Comments,
        [array]$CapturedInputValues,
        [Nullable[Guid]]$SupersedesResultEntryId,
        [string]$ExecutedBy,
        [DateTimeOffset]$ExecutedAt
    )

    $resultEntry = [ordered]@{
        ResultEntryId        = $ResultEntryId.ToString()
        Result               = $Result
        MeasuredValue        = $MeasuredValue
        Comments             = $Comments
        CapturedInputValues  = $CapturedInputValues
        SupersedesResultEntryId = $null
        ExecutedBy           = $ExecutedBy
        ExecutedAt           = New-IsoTimestamp $ExecutedAt
    }

    if ($SupersedesResultEntryId.HasValue) {
        $resultEntry.SupersedesResultEntryId = $SupersedesResultEntryId.Value.ToString()
    }

    return $resultEntry
}

function New-EvidenceRecord {
    param(
        [Guid]$EvidenceId,
        [string]$OriginalFileName,
        [string]$StoredFileName,
        [string]$FileExtension,
        [string]$Sha256Hash,
        [string]$Description,
        [Guid]$TestItemId,
        [string]$AttachedBy,
        [DateTimeOffset]$AttachedAt
    )

    return [ordered]@{
        EvidenceId       = $EvidenceId.ToString()
        OriginalFileName = $OriginalFileName
        StoredFileName   = $StoredFileName
        FileExtension    = $FileExtension
        Sha256Hash       = $Sha256Hash.ToLowerInvariant()
        Description      = $Description
        TestItemId       = $TestItemId.ToString()
        AttachedBy       = $AttachedBy
        AttachedAt       = New-IsoTimestamp $AttachedAt
    }
}

function New-MotorNotes {
    param(
        [string]$Summary,
        [string]$RatedVoltage,
        [string]$RatedCurrent,
        [string]$PowerRating,
        [string]$Frequency,
        [string]$SpeedRpm,
        [string]$Phase
    )

    return @(
        $Summary.Trim()
        "[MotorDataPlate]"
        "RatedVoltage: $RatedVoltage"
        "RatedCurrent: $RatedCurrent"
        "PowerRating: $PowerRating"
        "Frequency: $Frequency"
        "SpeedRpm: $SpeedRpm"
        "Phase: $Phase"
        "[/MotorDataPlate]"
    ) -join "`r`n"
}

$baseTime = [DateTimeOffset]::Parse("2026-04-25T09:00:00+01:00")
function T([int]$Minutes) { return $baseTime.AddMinutes($Minutes) }

$projectId = [guid]::NewGuid()
$projectFolder = Join-Path $ProjectsRoot ("10064-DEMO_" + $projectId.ToString("N").Substring(0, 8))
$projectFile = Join-Path $projectFolder "project.testtrace.json"
$evidenceFolder = Join-Path $projectFolder "evidence"
$exportsFolder = Join-Path $projectFolder "exports"

New-Item -ItemType Directory -Path $projectFolder -Force | Out-Null
New-Item -ItemType Directory -Path $evidenceFolder -Force | Out-Null
New-Item -ItemType Directory -Path $exportsFolder -Force | Out-Null

$mechanicalSectionId = [guid]::NewGuid()
$electricalSectionId = [guid]::NewGuid()
$softwareSectionId = [guid]::NewGuid()
$optionalSectionId = [guid]::NewGuid()

$extruderAssetId = [guid]::NewGuid()
$drumCoolerAssetId = [guid]::NewGuid()
$extruderMotorAssetId = [guid]::NewGuid()
$drumMotorAssetId = [guid]::NewGuid()
$kibblerMotorAssetId = [guid]::NewGuid()

$overallConditionTestId = [guid]::NewGuid()
$nameplateTestId = [guid]::NewGuid()
$voltageTestId = [guid]::NewGuid()
$rotationTestId = [guid]::NewGuid()
$remoteDiagnosticsTestId = [guid]::NewGuid()

$l1VoltageInputId = [guid]::NewGuid()
$l2VoltageInputId = [guid]::NewGuid()
$l3VoltageInputId = [guid]::NewGuid()
$rotationDirectionInputId = [guid]::NewGuid()

$mechanicalPassResultId = [guid]::NewGuid()
$voltageFailResultId = [guid]::NewGuid()
$voltagePassResultId = [guid]::NewGuid()

$mechanicalEvidenceId = [guid]::NewGuid()
$voltageEvidenceId = [guid]::NewGuid()

$mechanicalStoredEvidence = "EV-000001_{0}_motor-condition-note.txt" -f $mechanicalEvidenceId.ToString("N").Substring(0, 8)
$voltageStoredEvidence = "EV-000002_{0}_voltage-bop-capture.txt" -f $voltageEvidenceId.ToString("N").Substring(0, 8)

$mechanicalEvidencePath = Join-Path $evidenceFolder $mechanicalStoredEvidence
$voltageEvidencePath = Join-Path $evidenceFolder $voltageStoredEvidence

Set-Content -LiteralPath $mechanicalEvidencePath -Encoding UTF8 -Value @"
Carbon Cell XTS19XP sample evidence
Mechanical overall condition note

Inspection summary:
- Paint finish acceptable
- Terminal box secure
- No visible transport damage
"@

Set-Content -LiteralPath $voltageEvidencePath -Encoding UTF8 -Value @"
Carbon Cell XTS19XP sample evidence
Electrical drive BOP capture note

Initial run:
- L1 401 V
- L2 365 V
- L3 398 V

Retest after terminal tightening:
- L1 399 V
- L2 401 V
- L3 400 V
"@

$mechanicalEvidenceHash = (Get-FileHash -LiteralPath $mechanicalEvidencePath -Algorithm SHA256).Hash.ToLowerInvariant()
$voltageEvidenceHash = (Get-FileHash -LiteralPath $voltageEvidencePath -Algorithm SHA256).Hash.ToLowerInvariant()

$project = [ordered]@{
    SchemaVersion = "1.0"
    ProjectId     = $projectId.ToString()
    State         = 2
    ContractRoot  = [ordered]@{
        ProjectType            = "FAT"
        ProjectName            = "Carbon Cell XTS19XP Demo"
        ProjectCode            = "10064-DEMO"
        MachineModel           = "XTS19XP"
        MachineSerialNumber    = "10065"
        CustomerName           = "Carbon Cell"
        ScopeNarrative         = "Sample FAT project based on the saved Carbon Cell XTS19XP content. Covers mechanical, electrical and software checks for the extruder, drum cooler and associated motors."
        LeadTestEngineer       = "Dan Chetwyn"
        ReleaseAuthority       = "Dan Chetwyn"
        FinalApprovalAuthority = "Andy Davis"
        CreatedBy              = "Codex Seed"
        CreatedAt              = New-IsoTimestamp (T 0)
    }
    ReportingMetadata = [ordered]@{
        ProjectStartDate                    = "2026-04-25"
        CustomerAddress                     = "Carbon Cell, Pilot Factory, Sample Industrial Estate"
        CustomerCountry                     = "United Kingdom"
        CustomerProjectReference            = "CC-XTS19XP-10064"
        SiteContactName                     = "Andy Davis"
        SiteContactEmail                    = "andy.davis@carboncell.example"
        SiteContactPhone                    = "01234 567890"
        MachineConfigurationSpecification   = "XTS19XP extrusion line with XTDC1 drum cooler, extruder main motor, drum motor and kibbler motor."
        ControlPlatform                     = "Siemens"
        MachineRoleApplication              = "Carbon Cell lab extrusion line"
        SoftwareVersion                     = "XTS19XP V1.2"
        LeadTestEngineerEmail               = "dan.chetwyn@example.com"
        LeadTestEngineerPhone               = "07123 456789"
    }
    Assets = @(
        [ordered]@{
            AssetId      = $extruderAssetId.ToString()
            Name         = "XTS 19 Extruder"
            Type         = "Component"
            Notes        = "Mechanical, software and control checks related to the extruder."
            CreatedBy    = "Codex Seed"
            CreatedAt    = New-IsoTimestamp (T 3)
        },
        [ordered]@{
            AssetId      = $drumCoolerAssetId.ToString()
            Name         = "XTDC1 Drum Cooler"
            Type         = "Component"
            Notes        = "Drum cooler assembly with drum motor and kibbler motor."
            CreatedBy    = "Codex Seed"
            CreatedAt    = New-IsoTimestamp (T 4)
        },
        [ordered]@{
            AssetId       = $extruderMotorAssetId.ToString()
            Name          = "02M1 Extruder Main Motor"
            Type          = "Motor"
            ParentAssetId = $extruderAssetId.ToString()
            Manufacturer  = "WEG"
            Model         = "W22"
            SerialNumber  = "CC-02M1-10065"
            Notes         = New-MotorNotes "Main extruder drive motor." "400 V" "8.2 A" "4.0 kW" "50 Hz" "1450 RPM" "3"
            CreatedBy     = "Codex Seed"
            CreatedAt     = New-IsoTimestamp (T 5)
        },
        [ordered]@{
            AssetId       = $drumMotorAssetId.ToString()
            Name          = "02M1 Drum Motor"
            Type          = "Motor"
            ParentAssetId = $drumCoolerAssetId.ToString()
            Manufacturer  = "TECO"
            Model         = "AEHH"
            SerialNumber  = "CC-DRM-2041"
            Notes         = New-MotorNotes "Drive motor for the XTDC1 drum cooler." "400 V" "3.1 A" "1.5 kW" "50 Hz" "1440 RPM" "3"
            CreatedBy     = "Codex Seed"
            CreatedAt     = New-IsoTimestamp (T 6)
        },
        [ordered]@{
            AssetId       = $kibblerMotorAssetId.ToString()
            Name          = "10M1 Kibbler Motor"
            Type          = "Motor"
            ParentAssetId = $drumCoolerAssetId.ToString()
            Manufacturer  = "WEG"
            Model         = "W21"
            SerialNumber  = "CC-KIB-1099"
            Notes         = New-MotorNotes "Motor driving the kibbler assembly." "400 V" "2.6 A" "1.1 kW" "50 Hz" "930 RPM" "3"
            CreatedBy     = "Codex Seed"
            CreatedAt     = New-IsoTimestamp (T 7)
        }
    )
    Sections = @(
        [ordered]@{
            SectionId        = $mechanicalSectionId.ToString()
            Title            = "Mechanical Checks"
            Description      = "Mechanical checks on the contract equipment and visible condition."
            DisplayOrder     = 1
            SectionApprover  = "Dan Chetwyn"
            Applicability    = 0
            Assets           = @(
                [ordered]@{
                    AssetId       = $extruderAssetId.ToString()
                    DisplayOrder  = 1
                    Applicability = 0
                    AddedBy       = "Codex Seed"
                    AddedAt       = New-IsoTimestamp (T 8)
                },
                [ordered]@{
                    AssetId       = $drumCoolerAssetId.ToString()
                    DisplayOrder  = 2
                    Applicability = 0
                    AddedBy       = "Codex Seed"
                    AddedAt       = New-IsoTimestamp (T 9)
                }
            )
            TestItems = @(
                [ordered]@{
                    TestItemId           = $overallConditionTestId.ToString()
                    TestReference        = "MEC-EXT-001"
                    TestTitle            = "Overall condition"
                    TestDescription      = "Check the overall condition of the extruder frame, guards, paint finish and visible fixings."
                    ExpectedOutcome      = "The extruder should be in new condition with no visible transport damage, chips, scratches or loose hardware."
                    AcceptanceCriteria   = [ordered]@{
                        ConditionType = 0
                    }
                    EvidenceRequirements = [ordered]@{
                        PhotoRequired         = $true
                        MeasurementRequired   = $false
                        SignatureRequired     = $false
                        FileUploadRequired    = $false
                        CommentRequiredOnFail = $true
                        CommentAlwaysRequired = $false
                    }
                    BehaviourRules = [ordered]@{
                        BlockProgressionIfFailed = $true
                        AllowOverrideWithReason  = $false
                        RequiresWitness          = $false
                    }
                    DisplayOrder   = 1
                    AssetId        = $extruderAssetId.ToString()
                    Applicability  = 0
                    Inputs         = @()
                    ResultHistory  = @(
                        (New-ResultEntry -ResultEntryId $mechanicalPassResultId -Result 1 -MeasuredValue $null -Comments "Extruder frame, paint finish and visible guards acceptable." -CapturedInputValues @() -SupersedesResultEntryId $null -ExecutedBy "Dan Chetwyn" -ExecutedAt (T 40))
                    )
                    EvidenceRecords = @(
                        (New-EvidenceRecord -EvidenceId $mechanicalEvidenceId -OriginalFileName "motor-condition-note.txt" -StoredFileName $mechanicalStoredEvidence -FileExtension ".txt" -Sha256Hash $mechanicalEvidenceHash -Description "Mechanical condition note for the extruder." -TestItemId $overallConditionTestId -AttachedBy "Dan Chetwyn" -AttachedAt (T 41))
                    )
                    CreatedBy      = "Codex Seed"
                    CreatedAt      = New-IsoTimestamp (T 15)
                    LatestResult   = 1
                },
                [ordered]@{
                    TestItemId           = $nameplateTestId.ToString()
                    TestReference        = "MEC-MOT-002"
                    TestTitle            = "Motor nameplate check"
                    TestDescription      = "Confirm the extruder main motor nameplate is fitted, legible and matches the data plate details."
                    ExpectedOutcome      = "Nameplate is present and legible with the expected voltage, current, power and speed details."
                    AcceptanceCriteria   = [ordered]@{
                        ConditionType = 0
                    }
                    EvidenceRequirements = [ordered]@{
                        PhotoRequired         = $true
                        MeasurementRequired   = $false
                        SignatureRequired     = $false
                        FileUploadRequired    = $false
                        CommentRequiredOnFail = $true
                        CommentAlwaysRequired = $false
                    }
                    BehaviourRules = [ordered]@{
                        BlockProgressionIfFailed = $true
                        AllowOverrideWithReason  = $false
                        RequiresWitness          = $false
                    }
                    DisplayOrder   = 2
                    AssetId        = $extruderMotorAssetId.ToString()
                    Applicability  = 0
                    Inputs         = @()
                    ResultHistory  = @()
                    EvidenceRecords = @()
                    CreatedBy      = "Codex Seed"
                    CreatedAt      = New-IsoTimestamp (T 16)
                    LatestResult   = 0
                }
            )
            CreatedBy        = "Codex Seed"
            CreatedAt        = New-IsoTimestamp (T 1)
        },
        [ordered]@{
            SectionId        = $electricalSectionId.ToString()
            Title            = "Electrical Checks"
            Description      = "Electrical checks on motors and associated drive readings."
            DisplayOrder     = 2
            SectionApprover  = "Dan Chetwyn"
            Applicability    = 0
            Assets           = @(
                [ordered]@{
                    AssetId       = $drumCoolerAssetId.ToString()
                    DisplayOrder  = 1
                    Applicability = 0
                    AddedBy       = "Codex Seed"
                    AddedAt       = New-IsoTimestamp (T 10)
                }
            )
            TestItems = @(
                [ordered]@{
                    TestItemId           = $voltageTestId.ToString()
                    TestReference        = "ELE-DRM-001"
                    TestTitle            = "Drive BOP voltage check"
                    TestDescription      = "Run the drum motor at 100% speed and record the line voltages shown on the drive BOP."
                    ExpectedOutcome      = "The displayed phase voltages should remain around 400 V within the accepted tolerance."
                    AcceptanceCriteria   = [ordered]@{
                        ConditionType = 1
                        TargetValue   = 400
                        Tolerance     = 20
                        Unit          = "V"
                    }
                    EvidenceRequirements = [ordered]@{
                        PhotoRequired         = $true
                        MeasurementRequired   = $true
                        SignatureRequired     = $true
                        FileUploadRequired    = $true
                        CommentRequiredOnFail = $true
                        CommentAlwaysRequired = $true
                    }
                    BehaviourRules = [ordered]@{
                        BlockProgressionIfFailed = $true
                        AllowOverrideWithReason  = $false
                        RequiresWitness          = $true
                    }
                    DisplayOrder   = 1
                    AssetId        = $drumMotorAssetId.ToString()
                    Applicability  = 0
                    Inputs         = @(
                        (New-TestInput -TestInputId $l1VoltageInputId -Label "L1 Voltage" -InputType 0 -TargetValue 400 -Tolerance 20 -Unit "V" -Required $true -DisplayOrder 1),
                        (New-TestInput -TestInputId $l2VoltageInputId -Label "L2 Voltage" -InputType 0 -TargetValue 400 -Tolerance 20 -Unit "V" -Required $true -DisplayOrder 2),
                        (New-TestInput -TestInputId $l3VoltageInputId -Label "L3 Voltage" -InputType 0 -TargetValue 400 -Tolerance 20 -Unit "V" -Required $true -DisplayOrder 3)
                    )
                    ResultHistory  = @(
                        (New-ResultEntry -ResultEntryId $voltageFailResultId -Result 2 -MeasuredValue "365 V on L2" -Comments "Initial run showed a low L2 reading. Retest required." -CapturedInputValues @(
                            (New-CapturedInputValue -TestInputId $l1VoltageInputId -Value "401"),
                            (New-CapturedInputValue -TestInputId $l2VoltageInputId -Value "365"),
                            (New-CapturedInputValue -TestInputId $l3VoltageInputId -Value "398")
                        ) -SupersedesResultEntryId $null -ExecutedBy "Dan Chetwyn" -ExecutedAt (T 43)),
                        (New-ResultEntry -ResultEntryId $voltagePassResultId -Result 1 -MeasuredValue "Stable around 400 V" -Comments "Retest after terminal tightening. Voltages stabilised." -CapturedInputValues @(
                            (New-CapturedInputValue -TestInputId $l1VoltageInputId -Value "399"),
                            (New-CapturedInputValue -TestInputId $l2VoltageInputId -Value "401"),
                            (New-CapturedInputValue -TestInputId $l3VoltageInputId -Value "400")
                        ) -SupersedesResultEntryId $voltageFailResultId -ExecutedBy "Dan Chetwyn" -ExecutedAt (T 47))
                    )
                    EvidenceRecords = @(
                        (New-EvidenceRecord -EvidenceId $voltageEvidenceId -OriginalFileName "voltage-bop-capture.txt" -StoredFileName $voltageStoredEvidence -FileExtension ".txt" -Sha256Hash $voltageEvidenceHash -Description "Voltage readings captured during fail and retest runs." -TestItemId $voltageTestId -AttachedBy "Dan Chetwyn" -AttachedAt (T 48))
                    )
                    CreatedBy      = "Codex Seed"
                    CreatedAt      = New-IsoTimestamp (T 17)
                    LatestResult   = 1
                },
                [ordered]@{
                    TestItemId           = $rotationTestId.ToString()
                    TestReference        = "ELE-KIB-001"
                    TestTitle            = "Kibbler motor rotation direction"
                    TestDescription      = "Jog the kibbler motor and confirm the shaft rotation is correct for normal operation."
                    ExpectedOutcome      = "Rotation direction is correct when the motor is jogged from the drive."
                    AcceptanceCriteria   = [ordered]@{
                        ConditionType = 2
                    }
                    EvidenceRequirements = [ordered]@{
                        PhotoRequired         = $false
                        MeasurementRequired   = $false
                        SignatureRequired     = $false
                        FileUploadRequired    = $false
                        CommentRequiredOnFail = $true
                        CommentAlwaysRequired = $false
                    }
                    BehaviourRules = [ordered]@{
                        BlockProgressionIfFailed = $true
                        AllowOverrideWithReason  = $false
                        RequiresWitness          = $true
                    }
                    DisplayOrder   = 2
                    AssetId        = $kibblerMotorAssetId.ToString()
                    Applicability  = 0
                    Inputs         = @(
                        (New-BooleanInput -TestInputId $rotationDirectionInputId -Label "Rotation direction correct" -Required $true -DisplayOrder 1)
                    )
                    ResultHistory  = @()
                    EvidenceRecords = @()
                    CreatedBy      = "Codex Seed"
                    CreatedAt      = New-IsoTimestamp (T 18)
                    LatestResult   = 0
                }
            )
            CreatedBy        = "Codex Seed"
            CreatedAt        = New-IsoTimestamp (T 2)
        },
        [ordered]@{
            SectionId        = $softwareSectionId.ToString()
            Title            = "Software Checks"
            Description      = "Software and HMI checks relevant to the sample FAT."
            DisplayOrder     = 3
            SectionApprover  = "Dan Chetwyn"
            Applicability    = 0
            Assets           = @(
                [ordered]@{
                    AssetId       = $extruderAssetId.ToString()
                    DisplayOrder  = 1
                    Applicability = 0
                    AddedBy       = "Codex Seed"
                    AddedAt       = New-IsoTimestamp (T 11)
                }
            )
            TestItems = @(
                [ordered]@{
                    TestItemId            = $remoteDiagnosticsTestId.ToString()
                    TestReference         = "SFT-EXT-001"
                    TestTitle             = "Remote diagnostics page"
                    TestDescription       = "Confirm the remote diagnostics page is available when that package is supplied."
                    ExpectedOutcome       = "Remote diagnostics page loads and shows controller communications."
                    AcceptanceCriteria    = [ordered]@{
                        ConditionType = 0
                    }
                    EvidenceRequirements  = [ordered]@{
                        PhotoRequired         = $false
                        MeasurementRequired   = $false
                        SignatureRequired     = $false
                        FileUploadRequired    = $false
                        CommentRequiredOnFail = $true
                        CommentAlwaysRequired = $false
                    }
                    BehaviourRules = [ordered]@{
                        BlockProgressionIfFailed = $false
                        AllowOverrideWithReason  = $false
                        RequiresWitness          = $false
                    }
                    DisplayOrder         = 1
                    AssetId              = $extruderAssetId.ToString()
                    Applicability        = 1
                    ApplicabilityReason  = "Remote diagnostics package is not included in this sample FAT scope."
                    Inputs               = @()
                    ResultHistory        = @()
                    EvidenceRecords      = @()
                    CreatedBy            = "Codex Seed"
                    CreatedAt            = New-IsoTimestamp (T 19)
                    LatestResult         = 0
                }
            )
            CreatedBy        = "Codex Seed"
            CreatedAt        = New-IsoTimestamp (T 11)
        },
        [ordered]@{
            SectionId           = $optionalSectionId.ToString()
            Title               = "Optional Packages"
            Description         = "Placeholder section for optional packages excluded from this sample machine."
            DisplayOrder        = 4
            SectionApprover     = "Dan Chetwyn"
            Applicability       = 1
            ApplicabilityReason = "No optional remote diagnostics or inverter add-on packages are supplied on this machine."
            Assets              = @()
            TestItems           = @()
            CreatedBy           = "Codex Seed"
            CreatedAt           = New-IsoTimestamp (T 13)
        }
    )
    AuditLog = @(
        (New-AuditEntry -Action "BuildContract" -Actor "Codex Seed" -At (T 0) -TargetType "Project" -TargetId $projectId -Details "Contract root frozen for the Carbon Cell sample project."),
        (New-AuditEntry -Action "AddSection" -Actor "Codex Seed" -At (T 1) -TargetType "Section" -TargetId $mechanicalSectionId -Details "Mechanical Checks"),
        (New-AuditEntry -Action "AddSection" -Actor "Codex Seed" -At (T 2) -TargetType "Section" -TargetId $electricalSectionId -Details "Electrical Checks"),
        (New-AuditEntry -Action "AddAsset" -Actor "Codex Seed" -At (T 3) -TargetType "Asset" -TargetId $extruderAssetId -Details "XTS 19 Extruder"),
        (New-AuditEntry -Action "AddAsset" -Actor "Codex Seed" -At (T 4) -TargetType "Asset" -TargetId $drumCoolerAssetId -Details "XTDC1 Drum Cooler"),
        (New-AuditEntry -Action "AddSubAsset" -Actor "Codex Seed" -At (T 5) -TargetType "Asset" -TargetId $extruderMotorAssetId -Details "02M1 Extruder Main Motor"),
        (New-AuditEntry -Action "AddSubAsset" -Actor "Codex Seed" -At (T 6) -TargetType "Asset" -TargetId $drumMotorAssetId -Details "02M1 Drum Motor"),
        (New-AuditEntry -Action "AddSubAsset" -Actor "Codex Seed" -At (T 7) -TargetType "Asset" -TargetId $kibblerMotorAssetId -Details "10M1 Kibbler Motor"),
        (New-AuditEntry -Action "AssignAssetToSection" -Actor "Codex Seed" -At (T 8) -TargetType "Asset" -TargetId $extruderAssetId -Details "Assigned to Mechanical Checks"),
        (New-AuditEntry -Action "AssignAssetToSection" -Actor "Codex Seed" -At (T 9) -TargetType "Asset" -TargetId $drumCoolerAssetId -Details "Assigned to Mechanical Checks"),
        (New-AuditEntry -Action "AssignAssetToSection" -Actor "Codex Seed" -At (T 10) -TargetType "Asset" -TargetId $drumCoolerAssetId -Details "Assigned to Electrical Checks"),
        (New-AuditEntry -Action "AddSection" -Actor "Codex Seed" -At (T 11) -TargetType "Section" -TargetId $softwareSectionId -Details "Software Checks"),
        (New-AuditEntry -Action "AssignAssetToSection" -Actor "Codex Seed" -At (T 12) -TargetType "Asset" -TargetId $extruderAssetId -Details "Assigned to Software Checks"),
        (New-AuditEntry -Action "AddSection" -Actor "Codex Seed" -At (T 13) -TargetType "Section" -TargetId $optionalSectionId -Details "Optional Packages"),
        (New-AuditEntry -Action "SetSectionApplicability" -Actor "Codex Seed" -At (T 14) -TargetType "Section" -TargetId $optionalSectionId -Details "Marked Optional Packages as not applicable for this machine."),
        (New-AuditEntry -Action "AddTestItem" -Actor "Codex Seed" -At (T 15) -TargetType "TestItem" -TargetId $overallConditionTestId -Details "MEC-EXT-001 - Overall condition"),
        (New-AuditEntry -Action "AddTestItem" -Actor "Codex Seed" -At (T 16) -TargetType "TestItem" -TargetId $nameplateTestId -Details "MEC-MOT-002 - Motor nameplate check"),
        (New-AuditEntry -Action "AddTestItem" -Actor "Codex Seed" -At (T 17) -TargetType "TestItem" -TargetId $voltageTestId -Details "ELE-DRM-001 - Drive BOP voltage check"),
        (New-AuditEntry -Action "AddTestItem" -Actor "Codex Seed" -At (T 18) -TargetType "TestItem" -TargetId $rotationTestId -Details "ELE-KIB-001 - Kibbler motor rotation direction"),
        (New-AuditEntry -Action "AddTestItem" -Actor "Codex Seed" -At (T 19) -TargetType "TestItem" -TargetId $remoteDiagnosticsTestId -Details "SFT-EXT-001 - Remote diagnostics page"),
        (New-AuditEntry -Action "SetTestApplicability" -Actor "Codex Seed" -At (T 20) -TargetType "TestItem" -TargetId $remoteDiagnosticsTestId -Details "Marked SFT-EXT-001 as not applicable."),
        (New-AuditEntry -Action "OpenForExecution" -Actor "Dan Chetwyn" -At (T 30) -TargetType "Project" -TargetId $projectId -Details "Project opened for FAT execution."),
        (New-AuditEntry -Action "RecordResult" -Actor "Dan Chetwyn" -At (T 40) -TargetType "TestItem" -TargetId $overallConditionTestId -Details "Recorded Pass for MEC-EXT-001."),
        (New-AuditEntry -Action "AttachEvidence" -Actor "Dan Chetwyn" -At (T 41) -TargetType "TestItem" -TargetId $overallConditionTestId -Details "Attached mechanical condition note."),
        (New-AuditEntry -Action "RecordResult" -Actor "Dan Chetwyn" -At (T 43) -TargetType "TestItem" -TargetId $voltageTestId -Details "Recorded initial Fail for ELE-DRM-001."),
        (New-AuditEntry -Action "RecordResult" -Actor "Dan Chetwyn" -At (T 47) -TargetType "TestItem" -TargetId $voltageTestId -Details "Recorded retest Pass for ELE-DRM-001."),
        (New-AuditEntry -Action "AttachEvidence" -Actor "Dan Chetwyn" -At (T 48) -TargetType "TestItem" -TargetId $voltageTestId -Details "Attached voltage capture note.")
    )
}

$json = $project | ConvertTo-Json -Depth 100
Set-Content -LiteralPath $projectFile -Encoding UTF8 -Value $json

Write-Host ""
Write-Host "Created TestTrace sample project:" -ForegroundColor Green
Write-Host $projectFolder
Write-Host ""
Write-Host "Project file:" -ForegroundColor Green
Write-Host $projectFile
Write-Host ""
Write-Host "You should see:" -ForegroundColor Green
Write-Host "- Carbon Cell XTS19XP Demo"
Write-Host "- Executable state"
Write-Host "- Mechanical, Electrical, Software and Optional Packages sections"
Write-Host "- Motor data plate metadata on motor assets"
Write-Host "- A passed mechanical test with evidence"
Write-Host "- A failed then retested electrical voltage check with structured inputs"
Write-Host "- An untested rotation-direction check"
Write-Host "- A not applicable software test"
