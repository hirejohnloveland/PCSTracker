# ADR 0022: Application Settings Live in RavenDB

## Context

Some configuration must be changeable while the application runs, without a deployment. Values that bootstrap the host, such as the RavenDB connection and Azure AD settings, stay in `appsettings.json`. The host needs them before it can reach RavenDB.

## Decision

- Each area of application settings is its own document, a class that implements `ISettingsDocument` and exposes a static `DocumentId`, such as `settings-technician-roles`.
- Every settings document is stored in one `Settings` collection. The document store's `FindCollectionName` convention maps every `ISettingsDocument` type to it.
- Settings are read by their fixed id through `ISettingsRepository`, never by querying the collection as one type. Application services depend on the repository interface, like any other repository ([ADR 0003](0003-domain-driven-design.md)).
- A settings document that has not been stored is not an error. `GetAsync<T>()` returns a new instance with the class's C# default values and does not store it. The document is written only when `StoreAsync` is called and the unit of work commits.
- A settings class follows the entity style: private setters, and an update method that enforces its own rules.
- Adding a settings area means adding a class. It needs no migration and no registration.

## Consequences

- A new or empty database works without provisioning. Until a document is saved, Studio shows none, and changing a C# default changes the effective value everywhere the document has not been saved.
- Settings in one area can be saved without touching another.
- Concurrent edits to one settings document are last-write-wins, which is RavenDB's default. Optimistic concurrency is not enabled.
- Nothing yet lets a user edit settings in the application. Until that exists, settings are edited as documents in RavenDB Studio, which does not validate them.

## Reference implementation

- Marker interface: [`ISettingsDocument`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Domain/Entities/Settings/ISettingsDocument.cs)
- Example settings class: [`TechnicianRoleSettings`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Domain/Entities/Settings/TechnicianRoleSettings.cs)
- Collection convention: [`RavenDbRegistration.AddCoreRavenDb`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/RavenDbRegistration.cs)
- Repository: [`SettingsRepository`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Implementations/SettingsRepository.cs). Exact call syntax: `TechnicianRoleSettings settings = await settingsRepository.GetAsync<TechnicianRoleSettings>(cancellationToken);`
