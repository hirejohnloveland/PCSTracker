# Getting Started

## Prerequisites

- .NET SDK 10.0.303 (see `global.json`; other versions are rejected)
- RavenDB running locally (the API reads `RavenDb:Urls` and `RavenDb:Database` from configuration; the API starts without it, but any request that reads or writes data fails)
- Node.js with npm (for the Ember front end in `src/web`)
- Google Chrome (for Ember tests)

## Build and run

```powershell
dotnet build src/api/PartsTrackerErp.GoldenPattern.slnx
dotnet run --project src/api/PartsTrackerErp.GoldenPattern.Api
```
