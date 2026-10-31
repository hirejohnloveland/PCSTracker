# Project Structure and Architectural Responsibilities

**Status:** Repository guidance  
**Applies to:** `PartsTrackerErp.GoldenPattern` solution and the `src/web` front end

## Purpose

This document defines where code belongs in the Golden Pattern and the responsibilities of each project and folder. Its purpose is to make the template predictable for developers and coding agents, without splitting every architectural boundary into another .NET project.

`PartsTrackerErp.GoldenPattern.Core` is a single class library; folders and namespaces mark its boundaries. See [ADR 0002](adr/0002-single-core-library.md).

## Domain-Driven Design

Core applies these DDD concepts. See [ADR 0003](adr/0003-domain-driven-design.md) for why.

- **Entities** carry behavior. Methods on the entity change its state and enforce its rules.
- **Value objects** model concepts defined by their values, such as an email address or an amount, and validate themselves.
- **Domain services** hold business rules that don't belong to a single entity or value object.
- **Application services** coordinate a use case: load entities through repositories, call domain behavior, and commit through the unit of work. They contain no business rules.
- **Repositories** load, add, and remove entities using RavenDB sessions. They don't call `SaveChangesAsync`.
- **Unit of work** commits the changes from a use case in one transaction.

## Solution projects

Solution: `src/api/PartsTrackerErp.GoldenPattern.slnx`

| Project | Responsibility |
| --- | --- |
| `PartsTrackerErp.GoldenPattern.Api` | HTTP host and composition root. Defines endpoints/controllers, transport concerns, middleware, authentication/authorization setup, and application startup. Delegates use cases to application services. |
| `PartsTrackerErp.GoldenPattern.Core` | Application orchestration, domain behavior, repository and integration implementations, contracts, mapping, validation, and related abstractions. Organized by responsibility inside a single assembly. |
| `PartsTrackerErp.GoldenPattern.Tests` | xUnit project at `tests/api/PartsTrackerErp.GoldenPattern.Tests`. Automated tests for application behavior, domain rules, data/integration boundaries, and architecture conventions. |

## Core folder reference

| Path under `Core` | What belongs here |
| --- | --- |
| `ApplicationServices/Abstractions` | Interfaces describing application use cases exposed to the API or other entry points. |
| `ApplicationServices/Implementations` | Use-case orchestration: coordinate repositories, external integrations, domain behavior, transactions, and result construction. Keep HTTP details and low-level persistence code out. |
| `Contracts/Requests` | Request DTOs accepted at application/API boundaries. These describe data coming in, not necessarily domain entities. |
| `Contracts/Responses` | Response DTOs returned to callers. Deliberately shape public or internal API responses without leaking persistence entities. |
| `Contracts/Integrations` | DTOs representing messages and payloads from/to external APIs and services. These model the external system's contract, not our domain. |
| `DependencyInjection` | Dependency-registration extension methods for Core services, repositories, clients, validators, and mapping. Invoked by the API composition root. |
| `Domain/Entities` | Objects with domain identity, meaningful lifecycle/state, and rules or invariants that belong to the business model. |
| `Domain/ValueObjects` | Domain concepts defined by their values rather than identity; put their validation and invariants here. |
| `Domain/Services/Abstractions` | Interfaces for domain services. |
| `Domain/Services/Implementations` | Business rules and calculations that do not naturally belong on one entity/value object. Prefer domain logic that is deterministic and independent of I/O. |
| `Domain/Exceptions` | Exceptions representing violations of domain rules or domain-specific failures. |
| `Infrastructure/Persistence` | Database infrastructure: RavenDB connection/session handling and other persistence plumbing. |
| `Repositories/Abstractions` | Interfaces for accessing data owned by this service. Define useful persistence operations, not necessarily generic CRUD for every entity. |
| `Repositories/Implementations` | Database access implementations: queries, commands, and persistence mappings using the infrastructure layer. |
| `Integrations/Abstractions` | Interfaces for clients/adapters that communicate with systems outside this service, such as another API or an external provider. |
| `Integrations/Implementations` | Concrete HTTP/SDK/message clients, external service calls, and serialization/transport handling. |
| `Mapping` | Mapping between request/response DTOs, integration DTOs, domain models, and persistence representations where needed. |
| `Validation/Validators` | Boundary/input validation, typically for request contracts (required fields, ranges, formats). Domain invariants remain in the domain. |
| `Utilities` | Small, reusable, broadly applicable helpers with no natural home elsewhere. Not a home for business logic. |

## Responsibility and dependency rules

1. **API endpoints invoke application services.** Endpoints handle HTTP concerns; they do not contain business workflows, database queries, or calls to external clients.
2. **Application services orchestrate; domain objects and services enforce business meaning.** An application service may load data via a repository, call an integration, invoke domain behavior, and construct a response. Domain code should not need to know how HTTP, RavenDB, or remote clients work.
3. **Repositories are for persistence owned by this service.** Repository interfaces and implementations live together in `Repositories`. See [ADR 0004](adr/0004-colocated-interfaces.md).
4. **Integrations are for external systems.** An HTTP client for the Portal Users API belongs under `Integrations`, not `Repositories`, even when its methods look like `GetUser` or `ListUsers`. Its wire DTOs belong in `Contracts/Integrations`.
5. **Contracts are transport shapes, not automatically domain models.** A DTO becomes a domain entity or value object only when the business model needs domain identity, meaning, or invariants—not merely because it is a JSON object.
6. **Depend on abstractions at consumption sites.** Application services should receive repository and integration interfaces through dependency injection, not instantiate their concrete implementations. Concrete types are wired at the composition root.
7. **Folders are logical boundaries, not separate assemblies.** See [ADR 0002](adr/0002-single-core-library.md). Architecture tests and analyzers may enforce selected conventions, with narrowly scoped exceptions when justified.

## Example: consuming a Users API

Suppose this service fetches user information from the Portal Users API but does not own or store users.

| Concern | Location |
| --- | --- |
| Portal Users API response DTO | `Contracts/Integrations/PortalUsers/PortalUserDto.cs` |
| Client interface | `Integrations/Abstractions/IPortalUsersClient.cs` |
| HTTP client implementation | `Integrations/Implementations/PortalUsersClient.cs` |
| Use case that fetches/uses users | `ApplicationServices/Implementations/...` |
| Response returned by our API | `Contracts/Responses/...` |

The external `PortalUserDto` is **not automatically** a `Domain/Entities/User`. Create a domain model only if this service actually needs one to express its business rules.

## Placement checklist

When adding code, ask:

- Is this HTTP hosting/transport behavior? **API.**
- Is it coordinating a use case? **ApplicationServices.**
- Is it a business rule or domain concept? **Domain.**
- Is it reading/writing our database? **Repositories** (with plumbing in **Infrastructure/Persistence**).
- Is it communicating with another service? **Integrations** (with payload DTOs in **Contracts/Integrations**).
- Is it a request, response, or other boundary payload? **Contracts.**
- Is it input validation or object transformation? **Validation** or **Mapping**, respectively.

## Web application

The front end is an Ember.js application in `src/web`. It is not part of the .NET solution. See [ADR 0007](adr/0007-ember-frontend.md).

It keeps the layout generated by `ember new`. Folder names under `app/` are Ember's conventions, and Ember finds routes, route templates, controllers, and services by file name and location. Add files with Ember's generators, for example `npm exec ember generate route <name>`, so they land in the standard places.

## Web folder reference

| Path under `src/web` | What belongs here |
| --- | --- |
| `app/router.js` | Maps URLs to routes. Every page a user can navigate to is declared here. |
| `app/routes/` | One file per route. Loads the data a page needs and handles redirects. Contains no markup. |
| `app/templates/` | One template per route, as a `.gjs` file. `application.gjs` is the root template and is always on screen. |
| `app/components/` | Reusable UI. A component's markup and behavior live together in one `.gjs` file. |
| `app/controllers/` | Query parameters and state derived from a route's model. A controller has the same name as its route. |
| `app/services/` | Long-lived objects shared across the app, such as session state or API calls. |
| `app/helpers/` | Small functions used inside templates. |
| `app/styles/` | Global CSS. |
| `config/` | Build and runtime settings, such as `environment.js` and `targets.js`. |
| `public/` | Static files copied to the build output unchanged. |
| `tests/unit/` | Unit tests for routes, controllers, and services. |
| `tests/integration/` | Tests that render a component or helper. |

**Guiding principle:** Keep the template small and the placement rules clear. Add complexity only when it solves a demonstrated problem.
