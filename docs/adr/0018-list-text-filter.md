# ADR 0018: A List's Text Filter Is a Case-Insensitive Word Search

## Context

A person narrowing a list types part of what they remember, and that is not always the start of the name. A prefix match finds `Acme Supply` for `acme` but not for `supply`. RavenDB does not run `string.Contains` in a query, so matching inside a name needs a full-text search.

## Decision

- A text filter on a list endpoint is one optional string on the request DTO, such as `Name` on `ListVendorsDto`. Null or blank applies no filter. The DTO property's description states the matching rule, because it appears in the OpenAPI document ([ADR 0015](0015-openapi-responses.md)).
- An item matches when its field contains every word of the text, ignoring case. A word matches inside the words of the field, so `sup` finds `Acme Supply`. Word order does not matter.
- The application service trims the text and passes it to the repository. The repository method names the parameter after the field and the behavior, such as `nameSearch`.
- The repository treats any character that is not a letter or digit as a separator. The text becomes a list of words, and when no words remain it applies no filter. The characters of RavenDB's search syntax, such as `*`, `?`, and quotes, never reach the query.
- The query is `Search` on the field with a `*` on each side of every word, joined by spaces, with `SearchOperator.And`.

## Consequences

- Punctuation cannot be searched. `109-` finds the same items as `109`, and `A-1` and `A 1` are the same search.
- A word matches inside one word of the field, not across two. `me sup` finds `Acme Supply` because each part matches a different word, but a single piece that spans the space does not.
- RavenDB creates an auto-index for the search on first use ([ADR 0014](0014-pagination.md)). No static index is defined.
- The match is a wildcard search and has not been measured against a large collection.
- The repository has no RavenDB integration tests, so no automated test runs this query. Application service tests check the text passed to the repository.
- The word splitting is a private method of `VendorRepository`. A second list that needs it would have to move it somewhere shared, and where is not decided.

## Reference implementation

- The query and the word splitting: [`VendorRepository.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Implementations/VendorRepository.cs): `query = query.Search(vendor => vendor.Name, terms, @operator: SearchOperator.And);`
- The repository contract: [`IVendorRepository.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Abstractions/IVendorRepository.cs)
- The request DTO and its description: [`ListVendorsDto.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests/ListVendorsDto.cs)
- Trimming before the repository call: [`VendorsApplicationService.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/VendorsApplicationService.cs)
