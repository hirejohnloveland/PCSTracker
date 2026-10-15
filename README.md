# GoldenPattern

The Golden Pattern for Ziosk microservice APIs: .NET services built with Domain-Driven Design and hosted on Azure. Use it as the reference when starting a new service.

## Layout

| Path | Purpose |
|------|---------|
| `src/api/Ziosk.GoldenPattern.Api` | The API project |
| `src/api/Ziosk.GoldenPattern.Core` | Class library for the business logic, shared by all hosts |
| `docs/` | ADRs, coding standards, project structure, getting started |
| `.agents/` | Harness-agnostic agents, skills, and commands |
| `.cursor/`, `.claude/`, `.github/` | Harness registration for the shared `.agents/` content |
| `AGENTS.md` | Entry point for AI agents |

## Build and run

Requires the .NET 10 SDK.

```powershell
dotnet build src/api/Ziosk.GoldenPattern.slnx
dotnet run --project src/api/Ziosk.GoldenPattern.Api
```
