# ADR 0007: Ember.js for the Front End

## Context

The solution has an API and needs a browser front end. Ember.js is a convention-based JavaScript framework. `ember new` generates a standard folder layout, and Ember finds routes, route templates, controllers, and services by file name and location.

## Decision

- The front end is an Ember.js application in `src/web`.
- It is generated with `ember new` and keeps the generated layout. New files go where Ember's generators put them, using Ember's standard folder names.
- It starts as a minimal app: JavaScript rather than TypeScript, and no Ember Data.

## Consequences

- Where front-end code belongs follows Ember's conventions, not the `Core` folder rules.
- Developers need Node.js and npm in addition to the .NET SDK, and Chrome to run the tests.
- There is no data layer yet, because Ember Data is not installed.
