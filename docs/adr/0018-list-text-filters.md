# ADR 0018: List Text Filters

## Context

A person narrowing a list types part of what they remember, and that is not always the start of the name. A prefix match finds `Acme Supply` for `acme` but not for `supply`. RavenDB does not run `string.Contains` in a query, so matching inside a name needs a full-text search.

A list that narrows while the user types feels immediate, but a request for every keystroke is wasteful, and every applied filter changes the URL ([ADR 0014](0014-pagination.md)). The Ember lint rule `ember/no-runloop` rejects `debounce` from `@ember/runloop` and points to `ember-lifeline` or `ember-concurrency`.

## Decision

### Backend

- A text filter on a list endpoint is one optional string on the request DTO, such as `Name` on `ListVendorsDto`. Null or blank applies no filter. The DTO property's description states the matching rule, because it appears in the OpenAPI document ([ADR 0015](0015-openapi-responses.md)).
- An item matches when its field contains every word of the text, ignoring case. A word matches inside the words of the field, so `sup` finds `Acme Supply`. Word order does not matter.
- The application service trims the text and passes it to the repository. The repository method names the parameter after the field and the behavior, such as `nameSearch`.
- The repository treats any character that is not a letter or digit as a separator. The text becomes a list of words, and when no words remain it applies no filter. The characters of RavenDB's search syntax, such as `*`, `?`, and quotes, never reach the query.
- The query is `Search` on the field with a `*` on each side of every word, joined by spaces, with `SearchOperator.And`.

### Front end

- A text filter on a list screen is a query parameter on the list's route and controller, beside `page`, with an empty default.
- Typing in the filter input applies the text 300 ms after the last keystroke. The controller does this with `debounceTask` from `ember-lifeline`, which cancels the wait if the controller is destroyed. There is no Filter button.
- Pressing Enter in the input applies the text at once and cancels the pending wait, through `cancelDebounce`.
- Applying trims the text. Text that is the same as the current filter does nothing. A change resets `page` to 1.
- The route declares the filter's query parameter with `replace: true`, so each applied value replaces the current history entry and Back leaves the list. Changes to `page` still add entries.
- `ember-lifeline` is a dev dependency. `debounceTask` takes the method's name as a string, so the method it calls is public on the controller.

## Consequences

- Punctuation cannot be searched. `109-` finds the same items as `109`, and `A-1` and `A 1` are the same search.
- A word matches inside one word of the field, not across two. `me sup` finds `Acme Supply` because each part matches a different word, but a single piece that spans the space does not.
- RavenDB creates an auto-index for the search on first use ([ADR 0014](0014-pagination.md)). No static index is defined.
- A list request is sent once the user pauses for 300 ms, not once for each keystroke.
- A filter value that was replaced in the history cannot be reached with Back.
- The delay is a constant in the controller and is the same for every list screen that copies it.

## Reference implementation

- The query and the word splitting: [`VendorRepository.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Implementations/VendorRepository.cs): `query = query.Search(vendor => vendor.Name, terms, @operator: SearchOperator.And);`
- The repository contract: [`IVendorRepository.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Abstractions/IVendorRepository.cs)
- The request DTO and its description: [`ListVendorsDto.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests/ListVendorsDto.cs)
- Trimming before the repository call: [`VendorsApplicationService.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/VendorsApplicationService.cs)
- The controller: [`vendors/index` controller](../../src/web/app/controllers/vendors/index.ts): `debounceTask(this, 'applyName', value, FILTER_DELAY_MS);`
- The route: [`vendors/index` route](../../src/web/app/routes/vendors/index.ts), where `name: { refreshModel: true, replace: true }`
- The input and form: [`vendors/index` template](../../src/web/app/templates/vendors/index.gts)
- Tests, including that typing quickly sends one request: [`vendors-list-test.ts`](../../src/web/tests/acceptance/vendors-list-test.ts)
