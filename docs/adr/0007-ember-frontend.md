# ADR 0007: Ember.js for the Front End

## Context

The solution has an API and needs a browser front end. Ember.js is a convention-based JavaScript framework that supports TypeScript. `ember new` generates a standard folder layout, and Ember finds routes, route templates, controllers, and services by file name and location.

## Decision

- The front end is an Ember.js application in `src/web`.
- It is generated with `ember new` and keeps the generated layout. New files go where Ember's generators put them, using Ember's standard folder names.
- It is written in TypeScript (`.ts` and `.gts` files).
- It does not use Ember Data or WarpDrive. Code that calls the API uses the browser's `fetch` through the API client ([ADR 0017](0017-api-client.md)).

## Consequences

- Where front-end code belongs follows Ember's conventions, not the `Core` folder rules.
- Developers need Node.js and npm in addition to the .NET SDK, and Chrome to run the tests.
- There is no data layer, because neither Ember Data nor WarpDrive is installed.
