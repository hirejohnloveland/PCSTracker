# ADR 0014: Pagination for List Endpoints

## Context

A list endpoint can match more items than a caller wants at once, and the caller needs to know how many there are. RavenDB returns every match when a query has no `Take`, and returns them in no set order unless the query sorts.

## Decision

- A list endpoint is a `GET` that takes `page` (1-based, default 1) and `pageSize` (default 25, at most 100) as query parameters, plus its own filters. Paging is by page number and size, not by cursor, so the response can carry the total. The request DTO derives from `PagedRequestDto` and its validator from `PagedRequestDtoValidator<T>` ([ADR 0005](0005-fluent-validation.md)).
- Bad paging input is a 400 from FluentValidation. `pointer` names the query parameter, such as `#/pageSize` ([ADR 0011](0011-problem-details.md)).
- The response is a `PagedApiResponse<T>`. `data` is the array of items, `pagination` sits beside it with `page`, `pageSize`, `totalCount` and `totalPages`, and `metadata` is as in [ADR 0010](0010-service-response.md). A page past the end has no items and is still a 200.
- The application service returns `ServiceResponse<PagedResponseDto<T>>`. `PagedResponseDto<T>` is the items and the pagination, and the controller adds the metadata.
- A list action returns `ToPagedResult(response)`. `ToResult` throws if it is handed a `PagedResponseDto<T>`, because the pagination would end up inside `data`. Each list endpoint has a pipeline test that checks the top-level members of its response.
- A repository list method returns a `PagedResult<T>` of entities and the total count ([ADR 0003](0003-domain-driven-design.md)). It always sorts, ending with `Id` so equal keys keep a fixed order, applies `Skip` and `Take`, and takes the total from the query statistics (`QueryStatistics.TotalResults`).

## Consequences

- RavenDB queries are eventually consistent and loading by id is not, so a record created a moment ago can be missing from a list for a few milliseconds. The create response and a get by id are not affected. A list does not wait for non-stale results.
- RavenDB creates an auto-index the first time a list query uses a new filter or sort. No static index is defined.
- The validator rejects a page whose offset, `(page - 1) * pageSize`, does not fit an int.
- A list has no sort parameter. Its filters are the ones its request DTO defines, which for vendors is a case-insensitive name prefix.

## Front end

- A list screen keeps `page` and its filters in the URL as query parameters, so a page can be linked to and survives a refresh. The route sets `refreshModel` on each, and the controller declares them with defaults of `1` and an empty filter.
- Changing a filter resets `page` to 1.
- The screen does not send `pageSize`, so the API's default applies.
- The `pagination-controls` component takes `page` and `totalPages` from the response's `pagination`. It shows Previous and Next as links that change only `page`, and as plain disabled text on the first and last page. It shows nothing when `totalPages` is 0.

## Reference implementation

- The envelope: [`PagedApiResponse.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Responses/PagedApiResponse.cs)
- A list action, in [`VendorsController.List`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/VendorsController.cs): `return ToPagedResult(response);`. The method is [`ApiControllerBase.ToPagedResult`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/ApiControllerBase.cs).
- The request and its rules: [`PagedRequestDto.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests/PagedRequestDto.cs), [`ListVendorsDto.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests/ListVendorsDto.cs) and [`PagedRequestDtoValidator.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Validation/Validators/PagedRequestDtoValidator.cs)
- The paging query: [`VendorRepository.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Implementations/VendorRepository.cs)
- Mapping a page to its response: [`PagedResultMappingExtensions.ToPagedResponseDto`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Mapping/PagedResultMappingExtensions.cs), called in [`VendorsApplicationService.ListAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/VendorsApplicationService.cs)
- A list screen: [`vendors/index` route](../../src/web/app/routes/vendors/index.ts), [controller](../../src/web/app/controllers/vendors/index.ts), and [template](../../src/web/app/templates/vendors/index.gts)
- The paging controls: [`pagination-controls.gts`](../../src/web/app/components/pagination-controls.gts)
- A pipeline test of the envelope: [`Given_ApiFailureBehavior.Vendors_List_AnswersWithTheDataPaginationAndMetadata`](../../tests/api/PartsTrackerErp.GoldenPattern.Tests/Api/FailureBehavior/Given_ApiFailureBehavior.cs)
