# TestTrace

TestTrace is the DCC Systems governed FAT/SAT execution prototype. It is currently a WinForms/.NET application focused on building a frozen contract root, defining machine assets and test sections, executing governed FAT checks, attaching typed evidence, approving sections, releasing projects, and exporting an auditable FAT report.

## Active App

Active source:

`C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace V1`

Legacy/reference source:

`C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace.UI`

Do not use `TestTrace.UI` for current development unless Dan explicitly reactivates it.

## Run And Verify

Build:

```powershell
dotnet build "C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace V1\TestTrace V1.csproj" -o "C:\Users\Dan\AppData\Local\Temp\TestTraceVerify"
```

Smoke:

```powershell
dotnet "C:\Users\Dan\AppData\Local\Temp\TestTraceVerify\TestTrace V1.dll" --smoke
```

Run app:

```powershell
dotnet run --project "C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace V1\TestTrace V1.csproj"
```

## Data Roots

By default the app uses `Documents\TestTraceProjects` unless `TESTTRACE_PROJECTS_ROOT` is set or the sandbox launcher/debug profile supplies a sandbox path.

Sandbox launcher:

`C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace V1\Launch TestTrace Sandbox.bat`

## Current Governed Capabilities

- Local operator profiles and project user accounts.
- Project authority assignments by role and scope.
- Governed result recording with executor authority.
- Optional witness-required execution.
- Optional override-with-reason handling for failed results.
- Structured input capture with type validation.
- Typed evidence records and requirement enforcement before approval.
- Applicability separated from result state.
- Section approval and final release guards.
- Markdown FAT export with audit, authority, evidence, and execution details.
