# TestTrace UI Direction - Mode Shell First Pass

Date: 2026-04-26
Scope: Presentation structure only

## Intent

Start incorporating the generated product-direction thinking into the real WinForms workspace without destabilising the governed backend.

This pass moves the workspace from a single centre tab cluster toward a product-mode shell:

- Overview
- Execute
- Assets
- Triage
- Report

## Implemented

### Overview

Added a command-centre style page with project health cards:

- lifecycle
- applicable tests
- passed
- failed
- not tested
- declared evidence gaps

The detail table gives the current project, machine, customer, selected focus, structure counts, authority counts, and next governed move.

### Execute

Preserved the existing execution strip surface and moved it under the `Execute` mode.

No execution behaviour changed.

### Assets

Added an asset inventory page listing:

- asset hierarchy
- asset type
- parent asset
- section usage
- linked tests
- evidence count
- metadata summary

### Triage

Added a state-led work list with:

- failed tests first
- tests needing evidence
- not tested tests
- passed tests
- not applicable tests last

This is an overview/triage surface only. It does not change execution rules.

### Report

Added a report-oriented mode containing:

- report status summary
- contract metadata
- selected result history
- selected evidence
- audit activity

Existing export remains Markdown and unchanged.

## Not Changed

- Domain model
- Services
- Persistence
- Approval rules
- Evidence rules
- Export format
- Execution strip behaviour
- Authority model

## Verification

- `dotnet build` passed with 0 warnings and 0 errors.
- Smoke check passed.

## Rendering Fix - 2026-04-26

Dan's screenshots showed three visual regressions from the mode-shell pass:

- native white TabControl header space was showing to the right of the tabs
- Overview metric cards clipped long values/captions
- execution strip observation fields were too shallow and clipped existing comments

Fixes applied:

- themed TabControl drawing now paints the unused header area
- Overview metric cards are wider and taller
- execution strips reserve more vertical space for observations

Verification:

- `dotnet build` passed with 0 warnings and 0 errors.
- Smoke check passed.

## Next Design Opportunities

- Make the mode selector look less like stock tabs and more like product navigation.
- Make Overview metric cards more visually intentional.
- Turn Assets into a more visual machine-structure surface.
- Turn Triage into true Kanban columns.
- Turn Report into a dark-mode FAT document preview rather than metadata tables.
