# Tracker (GoldenPattern)

A proof of concept for a local mechanical contractor: track parts inventory and log refrigerant for EPA reporting.

**Built:** the refrigerant log (list, filter, add, edit) and the customer, technician, and vendor data behind it.
**Not built:** parts inventory integration, dashboards, and the QuickBooks integration. The QuickBooks sync endpoints exist and return a "not implemented" failure.

## The real subject: the Golden Pattern

The contractor's domain is small. The more interesting part is the template underneath it: a .NET 10 API and Ember front end that I rebuilt as a starting point for production services. It is meant to answer the question "how do I get the same quality of code from every developer, and every AI agent, on every feature?"

The answer is to decide each cross-cutting concern once, write the decision down, and keep a working example in the repository.

### What the template standardizes

| Concern | What you get | Decision |
|---------|--------------|----------|
| Expected failures | Services return `ServiceResponse<T>` instead of throwing for "not found" or a broken business rule | [ADR 0010](docs/adr/0010-service-response.md) |
| Error responses | Every failure is an RFC 9457 `application/problem+json` body with a stable shape, error codes, and JSON Pointers to the offending field | [ADR 0011](docs/adr/0011-problem-details.md) |
| Unexpected exceptions | .NET's built-in exception handling; exception messages never reach a response body | [ADR 0012](docs/adr/0012-built-in-exception-handling.md) |
| Success responses | One `ApiResponse<T>` wrapper with metadata, plus a paged variant for lists | [ADR 0010](docs/adr/0010-service-response.md), [ADR 0014](docs/adr/0014-pagination.md) |
| Validation | FluentValidation at the boundary, reported as field-level problems | [ADR 0005](docs/adr/0005-fluent-validation.md) |
| Observability | OpenTelemetry through Azure Monitor, with a trace id that appears in logs, in failure responses, and in success metadata | [ADR 0013](docs/adr/0013-observability.md) |
| API contract | Every action declares its responses; the OpenAPI document is snapshot-tested | [ADR 0015](docs/adr/0015-openapi-responses.md) |
| Front-end types | TypeScript types are generated from the OpenAPI document, so the client cannot drift from the API | [ADR 0016](docs/adr/0016-generated-api-types.md) |
| Front-end API access | One client wraps `fetch` and turns every failure into a typed `ApiError` | [ADR 0017](docs/adr/0017-api-client.md) |
| Front-end errors | A failed user action shows one consistent error dialog, with severity taken from the HTTP status | [ADR 0019](docs/adr/0019-front-end-error-handling.md) |
| Architecture | Domain-driven design with repositories and a unit of work, RavenDB persistence, and cancellation tokens through every layer | [ADR 0003](docs/adr/0003-domain-driven-design.md), [ADR 0006](docs/adr/0006-ravendb-persistence.md), [ADR 0008](docs/adr/0008-cancellation-tokens.md) |

The full set is in the [ADR index](docs/adr/README.md).

### Why it works well with AI agents

- **Decisions are written down.** Each ADR states what to do and why, in a few short bullets. [`AGENTS.md`](AGENTS.md) tells an agent which ones to read before it touches an area.
- **Each ADR points at working code.** A reference implementation section links to the real file that demonstrates the pattern. The link is to the source, not a pasted copy, so it cannot go stale.
- **Rules are enforced, not requested.** The build treats style violations as errors ([ADR 0009](docs/adr/0009-coding-standards-enforcement.md)). Architecture tests fail when code depends on something it should not. Agents are barred from suppressing a diagnostic or loosening the rules without asking.
- **Review is part of the process.** A `standards-reviewer` agent checks C# changes against the [coding standards](docs/coding-standards/csharp/README.md) before work is marked done.
- **The refrigerant log is the worked example.** It was built on the template by following the ADRs rather than inventing new conventions.

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
