# ADR 0019: A Failed User Action Is Shown in One Error Dialog

## Context

The API reports every failure the same way ([ADR 0011](0011-problem-details.md)), and the API client throws each one as an `ApiError` ([ADR 0017](0017-api-client.md)). An action that fails and says nothing leaves the user unsure whether it worked. If each screen built its own dialog, the wording, severity, and layout would differ from screen to screen.

## Decision

- An action a user starts, such as clicking a button, catches the failure of its API call and passes it to `errorDialog.show(error)`. The action does not build its own message or dialog.
- The `error-dialog` service holds the error being shown. The `ErrorDialog` component renders it once, in the application template, as a native `<dialog>` opened with `showModal`. A Close button or the Escape key dismisses it. A second error replaces the first.
- The message is the `detail` of the problem the API sent, or its `title` when there is no `detail`. The front end does not write messages for API failures. A single error's message reaches the user as the API authored it ([ADR 0011](0011-problem-details.md)).
- The status sets the severity:
  - 4xx is an expected failure. The dialog is a warning titled "Unable to complete this action".
  - 5xx and a network failure (status 0) are unexpected. The dialog is an error titled "Something went wrong".
  - Anything thrown that is not an `ApiError` is an error with the message "An unexpected error occurred."
- The dialog shows the `traceId` as a reference for a 5xx failure, so a user can quote it to support. It is not shown for other failures.
- A form validates its fields against the front end's data model, and it does not send a request while a field is invalid. The API validates again ([ADR 0005](0005-fluent-validation.md)), so a validation failure from the API means the two sides disagree, and it is shown in this dialog like any other failure.
- This ADR does not decide how a route that fails to load shows its failure.

## Consequences

- A 4xx message is shown to the user as written, so the API must author those messages for a user.
- When a failure has several errors, the `detail` is the API's generic summary, and the individual errors are not listed.
- An action that does not catch its failure shows nothing.
- Only one error is visible at a time.

## Reference implementation

- The service and the status rules: [`error-dialog.ts`](../../src/web/app/services/error-dialog.ts)
- The dialog, rendered once: [`error-dialog.gts`](../../src/web/app/components/error-dialog.gts) in [`application.gts`](../../src/web/app/templates/application.gts)
- An action that uses it: [`vendors/index` controller](../../src/web/app/controllers/vendors/index.ts), `syncWithQuickbooks`: `this.errorDialog.show(error);`
- A failure the API authors for the user: [`QuickbooksApplicationService.SyncVendorsAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/QuickbooksApplicationService.cs)
- Tests: [`error-dialog-test.ts`](../../src/web/tests/unit/services/error-dialog-test.ts) and [`vendors-list-test.ts`](../../src/web/tests/acceptance/vendors-list-test.ts)
