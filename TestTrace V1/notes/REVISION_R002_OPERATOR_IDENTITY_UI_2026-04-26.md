# Revision R002 - Operator Identity UI

Status: open
Date opened: 2026-04-26
Baseline: `R001_authority_operator_baseline_2026-04-26`

## Objective

Improve the operator profile creation and sign-in experience so it feels like a polished TestTrace product surface rather than a stock WinForms utility dialog.

This revision is a UI/interaction revision only unless a small supporting change is unavoidable.

## Product Intent

The operator profile is the user's local governed identity.

It should communicate:

- this identity is used for audit attribution
- project authority is assigned separately
- no password is required for the local profile
- actions in TestTrace are stamped with this operator identity

## Target Improvements

### First-run profile creator

Replace the plain field form feel with a more deliberate onboarding surface:

- strong title: `Create your local operator profile`
- concise subtitle explaining audit attribution
- initials badge generated from display name
- required display name
- optional email, phone, organisation
- clear primary action: `Create Profile`
- quiet trust note: `No password. Stored locally.`

### Returning-user sign-in

Move away from a table-first ListView feel:

- show operator profiles as cards or strong selectable rows
- initials badge
- display name
- organisation/email if available
- last used
- primary action: `Continue as <name>`
- secondary actions: create, edit, quit

### Project join prompt

Later in this revision if practical:

- replace the generic MessageBox with a themed join-project dialog
- explain the difference between app operator profile and project user registry
- actions: `Add to Project`, `Not now`

## Constraints

- do not change domain model unless absolutely required
- do not change authority rules
- do not change evidence, approval, release, export, or persistence behavior
- keep the work inside the backup TestTrace workspace
- preserve build/smoke health

## Relevant Files

- `UI/EditOperatorProfileForm.cs`
- `UI/OperatorSelectionForm.cs`
- `UI/OperatorProfile.cs`
- `UI/OperatorRegistry.cs`
- `UI/OperatorSession.cs`
- `UI/ProjectWorkspaceForm.cs`
- `UI/AppTheme.cs`

## Acceptance Checks

- first-run operator creation feels visually intentional
- returning operator selection feels like a product sign-in surface, not an admin list
- copy clearly distinguishes local operator identity from project authority
- dark mode remains readable
- build passes
- smoke passes

## Implementation Progress - 2026-04-26

Completed first R002 UI pass:

- Reworked `EditOperatorProfileForm` into a two-panel identity surface:
  - left-side TestTrace identity card
  - live initials badge generated from display name
  - local audit attribution explanation
  - required display name with inline validation
  - optional email, phone, and organisation metadata
  - clearer copy separating operator identity from project authority
- Reworked `OperatorSelectionForm` from a ListView table into selectable operator cards:
  - initials badge
  - display name
  - organisation/email summary
  - last-used indicator
  - `Continue as <name>` primary action
  - create/edit/quit secondary actions
- No domain, authority, evidence, approval, export, or persistence behavior changed.

Verification:

- `dotnet build` passed with 0 warnings and 0 errors.
- Smoke check passed.

## Fix - 2026-04-26 Profile Field Clipping

Dan's manual test showed the profile creator only displayed the first field and clipped the remaining fields. Cause: the new field card was docked at the default panel height inside a percent-sized layout row. Fix applied:

- field card now auto-sizes to its contents
- form layout now reserves a spacer row separately from the fields
- operator identity subtitle shortened to avoid clipping in the left card

Verification:

- `dotnet build` passed with 0 warnings and 0 errors.
- Smoke check passed.
