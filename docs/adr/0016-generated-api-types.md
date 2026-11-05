# ADR 0016: The Front End's API Types Are Generated from the OpenAPI Document

## Context

The front end sends requests to the API and reads its responses. The API's OpenAPI document describes every request and response shape, and [ADR 0015](0015-openapi-responses.md) keeps those shapes strict: required properties are marked required, and `data` and ids are never null. A TypeScript type written by hand for the same shape can drift from the API without any check noticing.

`src/api/openapi.json` is a committed copy of the document the API serves. A test, `Given_OpenApiSnapshot`, fails when the file differs from the served document and rewrites it when run with `UPDATE_OPENAPI_SNAPSHOT=true`.

## Decision

- The TypeScript types for API requests, responses, and problems are generated from `src/api/openapi.json` by `openapi-typescript`, a dev dependency. Nobody writes them by hand.
- `npm run generate:api` in `src/web` writes `app/types/api.d.ts`. The file is committed and excluded from Prettier and ESLint because it is generated.
- After an API change, rewrite `src/api/openapi.json` with the snapshot test, then run `npm run generate:api`.
- Code imports the generated `components` and `paths` types from `web/types/api` and gives the ones it uses a short name where it uses them, such as `Vendor` in `vendors-api`.
- `package.json` has an npm `overrides` entry that points the `typescript` peer dependency of `openapi-typescript` at the project's TypeScript. Version 7.13.0 declares `^5.x` and the project uses TypeScript 6. Its output was generated from this API's document and compiled under TypeScript 6 without errors.

## Consequences

- A change to a request or response shape shows up as a type error in the front end after the types are regenerated.
- Nothing checks that `app/types/api.d.ts` matches `src/api/openapi.json`, so a forgotten `generate:api` leaves the types stale.
- The types are as precise as the document. It does not state limits that only a validator enforces, such as the range of `page` and `pageSize` ([ADR 0015](0015-openapi-responses.md)).
- The override stays in `package.json` until `openapi-typescript` declares support for the project's TypeScript version.

## Reference implementation

- The script and the override, in [`package.json`](../../src/web/package.json): `"generate:api": "openapi-typescript ../api/openapi.json -o app/types/api.d.ts"`
- The generated types: [`app/types/api.d.ts`](../../src/web/app/types/api.d.ts)
- The source document: [`openapi.json`](../../src/api/openapi.json), kept current by [`Given_OpenApiSnapshot`](../../tests/api/PartsTrackerErp.GoldenPattern.Tests/Api/OpenApi/Given_OpenApiSnapshot.cs)
- Short names for generated types: [`vendors-api.ts`](../../src/web/app/services/vendors-api.ts)
