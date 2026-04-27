# Current TestTrace Revision

Current open revision: `R003_governed_execution_continuity_2026-04-27`

Active source tree:

`C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace`

Active application:

`C:\Users\Dan\DCC Systems\projects\testtrace-governance\architecture\TestTrace\TestTrace V1`

## Active Intent

Close the continuity gaps between the strengthened governed backend and the WinForms operator/workspace UI.

The current backend now enforces authority assignments, typed evidence requirements, witness requirements, override-with-reason behaviour, released-project authority immutability, and structured input type validation.

## Guardrails

- Keep `TestTrace V1` as the active app.
- Treat `TestTrace.UI` as legacy/reference unless Dan explicitly reactivates it.
- Keep build and smoke green after every governed workflow change.
- Domain and services remain the source of authority; UI surfaces collect intent and display guard reasons.
- Released projects are final records. User and authority mutation screens must present them as read-only.

## Current Focus

- Execution UI must collect witness and override details when a test behaviour requires them.
- Readiness logic must match domain approval rules.
- Repo entry docs must stay current enough for future sessions to route safely.
