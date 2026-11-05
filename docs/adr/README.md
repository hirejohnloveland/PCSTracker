# Architecture Decision Records

An ADR for an implementation pattern ends with a **Reference implementation** section: links to the file(s)/method(s) that currently demonstrate it, not pasted code, so the pointer can't drift from the source it describes.

- A class with no usage nuance, such as a registration, needs only the link.
- A class that defines how to use something, such as `ServiceResponse<T>`, adds one real line of exact call syntax next to the link.
- A pattern with no implementation yet to link to carries its own exemplar code instead.

The goal is not preserving code in two places. It's making sure an agent or a person reading the ADR knows exactly what's wanted.

| ADR | Topic |
|-----|-------|
| [0001](0001-host-registration-pattern.md) | Host registration pattern |
| [0002](0002-single-core-library.md) | Single Core library with folder boundaries |
| [0003](0003-domain-driven-design.md) | Domain-Driven Design with repositories and unit of work |
| [0004](0004-colocated-interfaces.md) | Interfaces live next to their implementations |
| [0005](0005-fluent-validation.md) | FluentValidation for boundary input validation |
| [0006](0006-ravendb-persistence.md) | RavenDB for persistence |
| [0007](0007-ember-frontend.md) | Ember.js for the front end |
| [0008](0008-cancellation-tokens.md) | Cancellation tokens through every layer |
| [0009](0009-coding-standards-enforcement.md) | C# coding standards enforcement |
| [0010](0010-service-response.md) | Application services return ServiceResponse for expected failures |
| [0011](0011-problem-details.md) | Failures reach callers as RFC 9457 problem details |
| [0012](0012-built-in-exception-handling.md) | Unexpected exceptions use .NET's built-in exception handling |
| [0013](0013-observability.md) | Observability with OpenTelemetry and Azure Monitor |
| [0014](0014-pagination.md) | Pagination for list endpoints |
| [0015](0015-openapi-responses.md) | Actions declare their responses for OpenAPI |
| [0016](0016-generated-api-types.md) | The front end's API types are generated from the OpenAPI document |
| [0017](0017-api-client.md) | Front-end code calls the API through one client |
| [0018](0018-list-text-filters.md) | List text filters |
| [0019](0019-front-end-error-handling.md) | A failed user action is shown in one error dialog |
