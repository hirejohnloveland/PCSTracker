import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import type ErrorDialogService from 'web/services/error-dialog';

module('Unit | Service | error-dialog', function (hooks) {
  setupTest(hooks);

  function lookup(owner: {
    lookup(name: string): unknown;
  }): ErrorDialogService {
    return owner.lookup('service:error-dialog') as ErrorDialogService;
  }

  test('a 4xx failure is a warning that shows the API detail and no trace id', function (assert) {
    const errorDialog = lookup(this.owner);

    errorDialog.show(
      new ApiError({
        status: 409,
        title: 'Conflict',
        detail: 'This feature is not implemented yet.',
        traceId: 'trace-409',
      }),
    );

    assert.deepEqual(errorDialog.current, {
      severity: 'warning',
      title: 'Unable to complete this action',
      message: 'This feature is not implemented yet.',
      traceId: null,
    });
  });

  test('a 5xx failure is an error that shows the trace id', function (assert) {
    const errorDialog = lookup(this.owner);

    errorDialog.show(
      new ApiError({
        status: 500,
        title: 'Internal Server Error',
        detail: 'An unexpected error occurred.',
        traceId: 'trace-500',
      }),
    );

    assert.deepEqual(errorDialog.current, {
      severity: 'error',
      title: 'Something went wrong',
      message: 'An unexpected error occurred.',
      traceId: 'trace-500',
    });
  });

  test('a network failure is an error with the title as its message and no trace id', function (assert) {
    const errorDialog = lookup(this.owner);

    errorDialog.show(
      new ApiError({ status: 0, title: 'The server could not be reached.' }),
    );

    assert.deepEqual(errorDialog.current, {
      severity: 'error',
      title: 'Something went wrong',
      message: 'The server could not be reached.',
      traceId: null,
    });
  });

  test('anything that is not an ApiError shows a generic message', function (assert) {
    const errorDialog = lookup(this.owner);

    errorDialog.show(new TypeError('x is not a function'));

    assert.deepEqual(errorDialog.current, {
      severity: 'error',
      title: 'Something went wrong',
      message: 'An unexpected error occurred.',
      traceId: null,
    });
  });

  test('dismiss clears the current error', function (assert) {
    const errorDialog = lookup(this.owner);
    errorDialog.show(new ApiError({ status: 409, title: 'Conflict' }));

    errorDialog.dismiss();

    assert.strictEqual(errorDialog.current, null);
  });
});
