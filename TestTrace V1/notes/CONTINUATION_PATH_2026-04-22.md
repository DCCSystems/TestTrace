# TestTrace Continuation Path

## Resume Target

Continue in the CODEX sandbox only:

`C:\Users\Dan\Desktop\DCC Systems CODEX\projects\testtrace-governance\architecture\TestTrace\TestTrace V1`

Do not touch the live tree:

`C:\Users\Dan\DCC Systems`

## Current Verified Baseline

Fresh verification completed today.

### Build

```powershell
dotnet build "C:\Users\Dan\Desktop\DCC Systems CODEX\projects\testtrace-governance\architecture\TestTrace\TestTrace V1\TestTrace V1.csproj" -o "C:\Users\Dan\AppData\Local\Temp\TestTraceCodexVerify"
```

Result:
- success
- 0 warnings
- 0 errors

### Smoke

```powershell
dotnet "C:\Users\Dan\AppData\Local\Temp\TestTraceCodexVerify\TestTrace V1.dll" --smoke
```

Result:
- passed
- verified path: build, structure, execution, evidence, approval, release, export, reload

## Where We Actually Landed

The project is in a healthy state to resume.

What is solid:
- governed workflow core still intact
- theme stabilization is holding
- browser shell is materially improved
- workspace shell is materially improved
- PASS / FAIL readability issue is fixed
- app launch regression from splitter sizing is fixed

What is still unfinished:
- browser shell still has too much empty mass and not enough compositional confidence
- workspace still feels slightly assembled rather than fully designed
- context panel is useful but still too property-grid-adjacent
- execution centre surface still wants stronger visual anchoring

## Files Most Likely To Matter Next

Primary files:
- `UI\MainForm.cs`
- `UI\ProjectWorkspaceForm.cs`
- `UI\AppTheme.cs`

Secondary files if the shell pass spills outward:
- `UI\NewProjectForm.cs`
- `UI\AddTestItemForm.cs`
- `UI\RecordResultForm.cs`

## Important Design Truths To Carry Forward

1. Applicability is separate from result.
2. Governance rules stay in services/domain, not UI.
3. Do not jump to Electron yet.
4. Do not add another major feature before the shell feels coherent.
5. The best near-term gains are presentation and composition, not architecture churn.
6. Use the sandbox as the only write target unless the user explicitly changes that instruction.

## Recommended Next Pass

### Name
**UI Composition And Layout Maturity Pass**

### Intent
Take the current dark-mode product shell and make it feel calmer, more intentional, and more balanced without changing any underlying behaviour.

### Scope
Focus on:
- browser composition
- workspace composition
- pane balance
- spacing rhythm
- typography hierarchy
- reducing visible border noise
- improving the context panel's readability
- improving how the execution surface sits within the centre pane

### Explicit non-goals
- no domain changes
- no service changes
- no export changes
- no workflow changes
- no approval changes
- no persistence changes
- no new architecture layers

## Practical Work Order

### 1. Refine the browser shell (`UI\MainForm.cs`)

Suggested focus:
- reduce the feeling of a table floating in a large empty plane
- make the selected project preview feel more composed and less boxy
- improve spacing and proportion between list and preview
- make the first screen feel like a product home, not just a launcher

### 2. Refine the workspace shell (`UI\ProjectWorkspaceForm.cs`)

Suggested focus:
- rebalance the structure / centre / context widths
- reduce heavy nested framing
- make the centre surface feel visually anchored
- improve context panel section hierarchy
- soften the remaining stock-WinForms harshness

### 3. Use theme semantics, not local one-off colours (`UI\AppTheme.cs`)

Suggested focus:
- only if needed to support the composition pass
- do not start a new theme-system expansion unless the current palette truly blocks the layout polish

## Specific Problems To Target Next

### Browser
- right side still feels overly empty when only one or two projects exist
- list and preview are siblings, but they do not yet feel like one curated browser experience
- the big project preview panel could use stronger grouping and cleaner rhythm

### Workspace
- left tree pane is still cramped and visually noisy
- the right context panel still feels too much like a dump of properties
- the execution strip sits inside too many rectangular boundaries
- the readiness panel and centre tabs are usable, but not yet elegant

## What To Avoid On Resume

Avoid these traps:
- adding a new major FAT feature because the shell still feels slightly unfinished
- trying to solve this with a framework jump
- overcomplicating the theme system when the bigger issue is layout balance
- chasing an AI-generated mockup that is too close to the current layout to be directionally useful

## Image Generation Note

A UI mockup generation pass was attempted, but the result was too close to the current app to serve as a strong design target.

That means the next pass should stay grounded in direct code refinement and screenshot-led critique.

If image generation is revisited later, the prompt should force a more ambitious redesign direction rather than a polished replica.

## Suggested Resume Prompt

```text
Continue the TestTrace UI Composition And Layout Maturity pass in the CODEX sandbox only.

Focus on product-shell polish rather than new features.

Goals:
1. Improve browser composition in MainForm so the first screen feels intentional and calm.
2. Improve workspace composition in ProjectWorkspaceForm so the left tree, centre execution surface, and right context panel feel like one designed shell.
3. Reduce border heaviness and nested panel noise.
4. Improve spacing, typography hierarchy, and pane balance.
5. Do not change domain, services, export, persistence, approval flow, or execution behaviour.

After changes:
- build
- run smoke
- report UI-only changes
```

## Environment / Tooling Notes

Important practical note from this session:
- the CODEX sandbox project is outside the default writable roots for this thread
- reading is fine, but writing to the sandbox project required escalated PowerShell commands
- use `workdir: C:\Users\Dan\Desktop` when running those commands; using the backup path as the working directory caused invalid-directory issues earlier

## Confidence Note

This is a safe resume point.

The app is not in a fragile or broken halfway state.
It is in the good kind of unfinished state: stable behaviour, improving shell, clear next polish target.
