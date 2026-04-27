# TestTrace Session Log

Date: 2026-04-25
Project: TestTrace V1
Workspace: `C:\Users\Dan\Desktop\DCC Systems Backup 17.04.26\projects\TestTrace-governance\architecture\TestTrace\TestTrace V1`
Mode: Backup clone (active TestTrace workspace as of 2026-04-24)
Live tree to avoid: `C:\Users\Dan\DCC Systems`

## Session Focus

The session began as a continuation of yesterday's Authority Model v1 work (which had been left mid-flight by a previous tool when its rate limit hit) and then expanded into building the application-level identity layer the system was missing.

Practical themes:
- close out Authority Model v1 verification
- ship the active-user UI on top of it (per-project user picker + management)
- fix the rough edges that surfaced when the model met the real UI
- introduce app-level operator profiles so the app actually has a sign-in step
- tighten path/launch configuration so the sandbox sample becomes discoverable again

## What Was Completed

### 1. Authority Model v1 verified end-to-end

The previous tool had landed the domain types, services, persistence, and export
changes for the Authority Model but was cut off before running the smoke. This
session picked that up and finished it.

- rebuilt against the existing source; build was 0/0 first try
- ran `--smoke` and identified one mismatch: the smoke assertion expected
  authority stamps to render as `"<Name> as <Role>"` but the export was
  producing `"<Role>: <Name>"`
- aligned `ExportService.FormatAuthority` to `"{Name} as {Role}{scope}"`
  (more natural for FAT report reading)
- final verification: build 0/0, smoke passed including all authority
  assertions on baseline users, project assignments, audit/result/evidence/
  approval/release stamps, persistence reload, and export coverage

### 2. Active-User UI layered on top of the model

The Authority Model existed in the data, but the UI had no concept of "who is
acting right now." This session built that layer.

Domain additions (extending what was there):
- `UserAccount` — relaxed init-only fields to private setters + `UpdateProfile`,
  `Deactivate`, `Reactivate` methods
- `AuthorityAssignment` — added `Revoke(by, reason, at)` with revocation
  metadata
- `TestTraceProject` — public methods: `AddUser`, `UpdateUser`,
  `DeactivateUser`, `ReactivateUser`, `AssignAuthority`, `RevokeAuthority`,
  `ScopeLabel`, plus critical-role guards (`ContractAuthor`,
  `LeadTestEngineer`, `ReleaseAuthority`, `FinalApprovalAuthority` cannot be
  the sole bearer left undeactivated/unrevoked)

Workspace service:
- `UsersAuthorityService` — wraps domain ops with validation + persistence,
  mirrors the `WorkspaceService` pattern
- `Contracts/UsersAuthorityRequests.cs` — six request DTOs

UI:
- `ActiveUserContext` — per-form active user with smart default picker
  (matches Windows username → falls back to LeadTestEngineer → first active)
- `EditUserForm` — add/edit user profile
- `AssignAuthorityForm` — user × role × scope picker (Project / Section /
  TestItem)
- `SwitchUserForm` — quick "who am I" picker with role badges
- `UsersAuthorityForm` — two-tab modal (Users / Authority) with full CRUD
  + revocation with reason prompt
- `ProjectWorkspaceForm` — "Acting as Dan (DA) [Switch...]" pill in the
  header; `Users & Authority...` button in the action bar; `CurrentActor()`
  switched from static (Windows username) to instance method backed by
  the active user

Smoke coverage extended to verify add user, update user, deactivate sole
ContractAuthor (rejected — guard works), deactivate Andy, reactivate Andy,
assign SectionApprover, revoke sole ContractAuthor (rejected — guard works),
revoke section approver — all clean.

### 3. UI rendering bugs hunted and fixed

Once the new screens met real screens, several real visual bugs surfaced.

**Header clipping in `ProjectWorkspaceForm`**: the title and the new
"Acting as" pill were both being clipped at the top of the row, looking like
they were hidden behind the summary card. Root cause: I had set `Dock = Fill`
on a nested `TableLayoutPanel` whose outer cell was `AutoSize`. With Dock=Fill,
the inner panel reports no preferred size, so the autosize cell collapses to
zero and the children render anchored but overflow upward.

Fix: restored `Dock = Top` (matching `MainForm.CreateHeader`'s working
pattern), explicit `RowCount = 1 + RowStyles.Add(AutoSize)`, anchored title
and pill to `Bottom` so they share a baseline, set `WrapContents = false` on
the FlowLayoutPanel.

**Ampersand mnemonic**: WinForms was eating `&` in label/button text as the
keyboard-shortcut prefix. "Local Users & Authority Assignments" was rendering
as "Local Users  Authority Assignments". Fix: `UseMnemonic = false` on
affected labels and buttons (and consistent use of single `&` rather than
`&&` once the mnemonic is disabled).

**ListView rows blanking on hover (the most insidious one)**: in
`UsersAuthorityForm`, list rows were losing their text as the cursor passed
over them. Root cause was in shared `AppTheme.cs`: `DrawListViewItem` was
filling the entire row background, then `DrawListViewSubItem` was supposed
to repaint each cell. On a partial repaint (mouse-hover invalidation),
`DrawItem` wiped the row pixels but Windows did not re-fire `DrawSubItem`
for every cell — so cells went blank.

Fix: two changes to `AppTheme.ConfigureListView`:
- removed the row-background fill from `DrawListViewItem` (each cell is now
  fully self-painted by `DrawListViewSubItem` — bg + border + text)
- enabled `LVS_EX_DOUBLEBUFFER` via P/Invoke `SendMessage` (the canonical
  comctl32 fix for partial-paint artifacts), hooked to `HandleCreated` so
  it survives handle recycling

Both fixes apply to **every** ListView in TestTrace, not just the new dialogs.

### 4. UX clunkiness pass

User feedback was that the active-user surface felt "clunky." Concrete fixes:

- removed the auto-prompt that opened the Switch User dialog every time a
  project loaded (it was intrusive when a sensible default already existed)
- moved the heavy "Users & Authority..." button out of the header pill into
  the action bar at the bottom, alongside Refresh and Open Folder
- slimmed the header pill to just `Acting as <Name (Initials)> [Switch...]`
  with no background block

### 5. Path alignment fix (P1 from Codex's earlier review)

The sandbox launchers had been pointing at the old `DCC Systems CODEX`
workspace, so the sample project Codex created (`10064-DEMO_5060f455`) was
invisible when launching from F5.

- `Properties/launchSettings.json` updated to
  `C:\Users\Dan\Desktop\DCC Systems Backup 17.04.26\sandbox-data\TestTraceProjects`
- `Launch TestTrace Sandbox.bat` updated to the same path

### 6. Legacy project loading made tolerant

After the Authority Model migration shipped, both pre-existing projects
(`10064` and `Twin-Screw-with-Ram-Feeder`) were loading as INVALID with the
error "User display name is required." Cause: `LeadTestEngineer = null` in
both, and `NormalizeAuthorityModel` → `EnsureBaselineAuthorityModel` →
`EnsureAuthorityAssignmentForName` was throwing when the field was missing.

Fix: `EnsureAuthorityAssignmentForName` now returns `null` when the legacy
display name is null/whitespace, instead of throwing. Migration is tolerant
of incomplete legacy records rather than rejecting them.

Plus a UX guard in `MainForm.OpenSelectedProject`: invalid rows can no
longer be opened in the workspace. The browser explains the error and
offers "open the folder" instead of dumping the user into a broken workspace
with "Project load failed."

### 7. Operator Profile v1 — application-level identity

The Authority Model v1 had been entirely **project-scoped**: users lived
inside each project's JSON. There was no app-level identity, no sign-in,
and the app dropped straight into the project browser.

Built operator profiles as the missing layer. Deliberately calibrated to
the earlier scope guidance: no passwords, no enterprise auth, no cloud sync,
no permission matrix.

Files added:
- `UI/OperatorProfile.cs` — immutable profile record with helpers to derive
  initials, update profile, mark last-active
- `UI/OperatorRegistry.cs` — JSON-backed list at
  `%APPDATA%\TestTrace\operators.json`, with atomic save
  (`.tmp` → `File.Replace`) and tolerant load (missing/malformed file →
  empty registry)
- `UI/OperatorSession.cs` — app-wide static session holder
- `UI/EditOperatorProfileForm.cs` — create/edit form, doubles as first-run
  welcome screen
- `UI/OperatorSelectionForm.cs` — sign-in picker with last-used default,
  "Continue as <Name>", Create / Edit / Quit

Wiring:
- `Program.cs` — picker / onboarding now runs before `MainForm`. Cancel
  exits cleanly. Selected operator becomes app-wide active user.
- `MainForm.cs` — "Signed in as <Name (Initials)> [Switch...]" pill in the
  top-right header
- `ProjectWorkspaceForm.cs` — operator → project user matching by display
  name; one-time prompt to add the operator to the project's user registry
  if absent ("You are signed in as 'Dan Chetwyn', but this project does
  not have you in its user registry yet. Add yourself...")

Sizing fix afterwards: both new login forms were initially `FixedDialog`
with `MinimumSize` set against total form size (including title bar and
borders). On smaller displays / higher DPI scaling the action row was
pushed off-screen with no way to resize. Fixed by switching to `Sizable`,
using `ClientSize` for preferred initial size, and dropping `MinimumSize`
to a true floor that fits any reasonable display.

## Verification

Final state:
- build: 0 warnings / 0 errors
- smoke: passed end-to-end (project built, structured, executed,
  evidence-bound, approved, released, exported, reloaded — including all
  authority management assertions and persistence reload)

## Open Threads / Recommended Next Pass

- **Evidence enforcement** is the next logical move. The earlier Codex review
  flagged it as P1, and now that authority + operator identity exist, the
  enforcement rule can be expressed properly: *"photo evidence required,
  attached by an authorised user, before approval."* The enforcement rule
  has the bite it was missing before.
- **Witness role** was added to the enum and is assignable, but the result
  record has no witness identity field yet. Decide whether MVP needs a real
  witness capture flow or whether the assignment is enough for v1.
- **Project integrity validator** still doesn't exist — `JsonProjectRepository`
  normalises legacy data on load but does not validate orphan IDs, duplicate
  IDs, invalid parent relationships, or impossible lifecycle states. Worth
  adding before evidence enforcement so guard violations don't compound.
- **`ProjectWorkspaceForm` extraction** — still very large. Not urgent now
  that backend gaps are sealing, but it's the next likely friction point.
  Recommended order when the time comes: ExecutionSheetControl,
  ExecutionStripControl, ContextPanelControl, StructureTreePresenter.
- **Operator → project linkage**: currently matched by display name only.
  When evidence enforcement lands, it may be worth strengthening the link
  (operator id stored in the project user record) so renames don't break
  attribution.
