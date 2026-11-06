# ADR 0017: Front-End Code Calls the API Through One Client

## Context

Every API response has the same shape: a success carries `data` and `metadata` ([ADR 0010](0010-service-response.md)), a list adds `pagination` ([ADR 0014](0014-pagination.md)), and a failure is `application/problem+json` with an `errors` array and a `traceId` ([ADR 0011](0011-problem-details.md)). [ADR 0007](0007-ember-frontend.md) has the front end call the API with `fetch` and installs no data layer. If each screen called `fetch` itself, each would repeat the request setup and the failure handling.

During development the Ember dev server (Vite) and the API run on different origins, and the API has no CORS configuration. The API's URLs, such as `/vendors`, are also URLs that Ember serves as pages, because Ember uses history-based locations.

## Decision

- Front-end code calls the API only through the `api-client` service, which is a thin wrapper around `fetch`. It has `getJson`, `postJson` and `putJson`. It builds the query string and leaves out undefined and empty values, sends and reads JSON, and returns the typed response body.
- A response that is not OK is thrown as an `ApiError`. It carries `status`, `title`, `detail`, `type`, `errors` (each with `code`, `detail`, and `pointer`), and `traceId` from the problem body. A response that is not problem JSON, such as a proxy error, and a failed network call are also thrown as an `ApiError`, with no `errors` and with status 0 for the network failure.
- A feature has its own service that wraps the client for one resource, such as `vendors-api`. It owns the paths, and it types its calls with the generated types ([ADR 0016](0016-generated-api-types.md)). Routes and components use that service. Nothing outside `api-client` calls `fetch`.
- The client sends every request under `/api`. In development, the Vite dev server forwards `/api/*` to the API at `https://localhost:7012` with the `/api` prefix removed, so the browser sees one origin and no CORS is needed. The `/api` prefix also keeps API calls apart from Ember's page URLs.
- The methods are named `getJson`, `postJson` and `putJson`, not `get`, `post` and `put`, because Ember's `Service` base class already has a `get` method.

## Consequences

- Handling a failure is the same on every screen: catch an `ApiError` and read `errors` and `traceId`.
- A form can place a validation error next to its field by matching `pointer` to the field name, such as `#/name`.
- The `/api` proxy exists only in the Vite dev server. A deployed front end needs something that routes `/api` to the API, and that is not decided.
- The client has `getJson`, `postJson` and `putJson` only. A method is added when a feature needs it.

## Reference implementation

- The client and `ApiError`: [`api-client.ts`](../../src/web/app/services/api-client.ts), used as `this.apiClient.getJson<VendorPage>('/vendors', query)`
- A feature service: [`vendors-api.ts`](../../src/web/app/services/vendors-api.ts)
- The dev proxy: [`vite.config.mjs`](../../src/web/vite.config.mjs)
- Tests of the client: [`api-client-test.ts`](../../src/web/tests/unit/services/api-client-test.ts)
