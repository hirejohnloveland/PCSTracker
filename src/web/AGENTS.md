# AGENTS.md (front end)

Guidance for AI agents working in `src/web`.

Before adding or moving files, read [docs/project-structure.md](../../docs/project-structure.md) (Web application and Web folder reference). Keep Ember's generated layout; add files with Ember's generators so they land in the standard places ([ADR 0007](../../docs/adr/0007-ember-frontend.md)).

Architecture decisions for this app are recorded in [docs/adr/README.md](../../docs/adr/README.md). Read the ones that cover the area you are changing. Front-end ADRs agents most often need:

| ADR                                                     | Topic                                                             |
| ------------------------------------------------------- | ----------------------------------------------------------------- |
| [0007](../../docs/adr/0007-ember-frontend.md)           | Ember.js layout and TypeScript                                    |
| [0014](../../docs/adr/0014-pagination.md)               | List pagination (query params and controls)                       |
| [0016](../../docs/adr/0016-generated-api-types.md)      | OpenAPI-generated types; do not edit `app/types/api.d.ts` by hand |
| [0017](../../docs/adr/0017-api-client.md)               | Call the API only through `api-client` and feature services       |
| [0018](../../docs/adr/0018-list-text-filters.md)        | List text filters and debounce via `ember-lifeline`               |
| [0019](../../docs/adr/0019-front-end-error-handling.md) | Failed user actions go through the shared error dialog            |
| [0020](../../docs/adr/0020-front-end-responsiveness.md) | Mobile-first layout; one shared breakpoint                        |

## Rules agents most often get wrong

- Write TypeScript (`.ts` / `.gts`). Do not use Ember Data or WarpDrive.
- Do not call `fetch` from routes, controllers, or components. Use `api-client` or a feature service such as `vendors-api`.
- After an API contract change, regenerate types with `npm run generate:api` (from `src/web`) once `src/api/openapi.json` is updated. Do not hand-edit `app/types/api.d.ts`.
- A user-started action that fails shows the error with `errorDialog.show(error)`. Do not build a one-off dialog or invent API failure copy.
- New and changed screens must work at phone width. Prefer CSS in `app/styles/app.css`; follow [ADR 0020](../../docs/adr/0020-front-end-responsiveness.md).
- Debounce filter typing with `debounceTask` from `ember-lifeline`, not `@ember/runloop`.
- Keep global styles in `app/styles/app.css`. Do not add a CSS framework unless the user asks.

## Commands

Run these from `src/web`:

- `npm start` — dev server
- `npm test` — build (development) and run Testem
- `npm run lint` — JS, templates, CSS, format check, and types
- `npm run lint:fix` — auto-fix what lint can fix, then format
- `npm run generate:api` — refresh `app/types/api.d.ts` from `../api/openapi.json`

Before finishing front-end work, run `npm run lint` and `npm test`, and fix what you broke.
