# TestTrace Context

## Purpose

TestTrace is a governed FAT/SAT execution system for DCC Systems. It aims to replace fragile Excel FAT workflows with an auditable contract root, machine asset hierarchy, explicit applicability, governed execution, typed evidence, section approval, final release, and defensible report export.

## Active Implementation

Use:

`C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace V1`

Treat as legacy/reference only:

`C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace.UI`

## Working Rules

- Keep domain and service layers as the authority for mutations and guards.
- UI should collect intent, display guard reasons, and call services.
- Preserve JSON compatibility unless a migration is deliberately specified.
- Keep build and smoke green before handing work back.
- Do not silently switch between live and sandbox roots.
- Current GitHub repo is `https://github.com/DCCSystems/TestTrace`.

## Verification

```powershell
dotnet build "C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace V1\TestTrace V1.csproj" -o "C:\Users\Dan\AppData\Local\Temp\TestTraceVerify"
dotnet "C:\Users\Dan\AppData\Local\Temp\TestTraceVerify\TestTrace V1.dll" --smoke
```

## Current Product Direction

The app is evolving into two clear experiences:

- Builder/admin workspace: contract root, project structure, assets, authority, approval, release.
- Operator execution workspace: simple, focused FAT execution by authorised engineers.

Dark mode is the default product posture.
