# ADR 0019: A List Screen's Text Filter Applies as the User Types

## Context

A list that narrows while the user types feels immediate, but a request for every keystroke is wasteful, and every applied filter changes the URL ([ADR 0014](0014-pagination.md)). The Ember lint rule `ember/no-runloop` rejects `debounce` from `@ember/runloop` and points to `ember-lifeline` or `ember-concurrency`.

## Decision

- A text filter on a list screen is a query parameter on the list's route and controller, beside `page`, with an empty default.
- Typing in the filter input applies the text 300 ms after the last keystroke. The controller does this with `debounceTask` from `ember-lifeline`, which cancels the wait if the controller is destroyed. There is no Filter button.
- Pressing Enter in the input applies the text at once and cancels the pending wait, through `cancelDebounce`.
- Applying trims the text. Text that is the same as the current filter does nothing. A change resets `page` to 1.
- The route declares the filter's query parameter with `replace: true`, so each applied value replaces the current history entry and Back leaves the list. Changes to `page` still add entries.
- `ember-lifeline` is a dev dependency. `debounceTask` takes the method's name as a string, so the method it calls is public on the controller.

## Consequences

- A list request is sent once the user pauses for 300 ms, not once for each keystroke.
- A filter value that was replaced in the history cannot be reached with Back.
- The delay is a constant in the controller and is the same for every list screen that copies it.

## Reference implementation

- The controller: [`vendors/index` controller](../../src/web/app/controllers/vendors/index.ts): `debounceTask(this, 'applyName', value, FILTER_DELAY_MS);`
- The route: [`vendors/index` route](../../src/web/app/routes/vendors/index.ts), where `name: { refreshModel: true, replace: true }`
- The input and form: [`vendors/index` template](../../src/web/app/templates/vendors/index.gts)
- Tests, including that typing quickly sends one request: [`vendors-list-test.ts`](../../src/web/tests/acceptance/vendors-list-test.ts)
