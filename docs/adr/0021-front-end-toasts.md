# ADR 0021: A Completed User Action Is Confirmed with a Toast

## Context

A successful action that says nothing leaves the user unsure whether it worked. A failure is shown in the error dialog ([ADR 0019](0019-front-end-error-handling.md)), which blocks the page and suits a problem the user must acknowledge. A success needs a lighter confirmation that does not interrupt the user.

No toast library is used. `ember-cli-notifications`, the addon inspected, is built on classic components, has no live region, and closes with a `div` instead of a button.

## Decision

- An action a user starts, such as clicking Save, confirms its success with `toasts.success(message)`. The action does not render its own message.
- The `toasts` service holds the visible toasts. The `ToastHost` component renders them once, in the application template, at the bottom center of the screen.
- A toast dismisses itself after 4 seconds, and its close button dismisses it sooner. Several toasts stack.
- `ToastHost` is a polite live region (`role="status"`), so screen readers announce a toast without moving focus.
- A toast confirms success only. A failure still goes to the error dialog.
- The only toast kind is `success`. Snackbars, which carry an action such as Undo, are not built.

## Consequences

- A toast is not a record. A user who looks away for 4 seconds can miss it, so it must not carry information the user needs to keep.
- The message is written by the front end, because there is no API response to author it.
- A toast does not pause on hover or focus.

## Reference implementation

- The service: [`toasts.ts`](../../src/web/app/services/toasts.ts)
- The host, rendered once: [`toast-host.gts`](../../src/web/app/components/toast-host.gts) in [`application.gts`](../../src/web/app/templates/application.gts)
- An action that uses it: [`vendors/details` controller](../../src/web/app/controllers/vendors/details.ts), `save`: `this.toasts.success('Vendor saved.');`
- Tests: [`toasts-test.ts`](../../src/web/tests/unit/services/toasts-test.ts), [`toast-host-test.gts`](../../src/web/tests/integration/components/toast-host-test.gts) and [`vendors-details-test.ts`](../../src/web/tests/acceptance/vendors-details-test.ts)
