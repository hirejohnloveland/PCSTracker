# GoldenPattern

The Golden Pattern for PartsTrackerErp microservice APIs: .NET services built with Domain-Driven Design and hosted on Azure. Use it as the reference when starting a new service.

## Layout

| Path | Purpose |
|------|---------|
| `src/api/PartsTrackerErp.GoldenPattern.Api` | The API project |
| `src/api/PartsTrackerErp.GoldenPattern.Core` | Class library for the business logic, shared by all hosts |
| `src/web` | Ember.js front end |
| `tests/api/PartsTrackerErp.GoldenPattern.Tests` | Automated tests for the API and Core |
| `docs/` | ADRs, coding standards, [project structure](docs/project-structure.md), getting started |
| `.agents/` | Harness-agnostic agents, skills, and commands |
| `.cursor/`, `.claude/`, `.github/` | Harness registration for the shared `.agents/` content |
| `AGENTS.md` | Entry point for AI agents |

## Build and run

Requires .NET SDK 10.0.303 (pinned in `global.json`), a local RavenDB instance, Node.js with npm, and Google Chrome. See [getting started](docs/getting-started.md).

```powershell
dotnet build src/api/PartsTrackerErp.GoldenPattern.slnx
dotnet run --project src/api/PartsTrackerErp.GoldenPattern.Api
```
