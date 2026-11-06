import { click, render, settled } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import ToastHost from 'web/components/toast-host';
import type ToastsService from 'web/services/toasts';

module('Integration | Component | toast-host', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders a live region that is empty until a toast is shown', async function (assert) {
    await render(<template><ToastHost /></template>);

    assert.dom('[role="status"]').hasAttribute('aria-live', 'polite');
    assert.dom('.toast').doesNotExist();
  });

  test('it shows a toast with its message and a labelled Dismiss button', async function (assert) {
    const toasts = this.owner.lookup('service:toasts') as ToastsService;
    await render(<template><ToastHost /></template>);

    toasts.success('Vendor saved.');
    await settled();

    assert.dom('.toast.success').hasText(/Vendor saved\./);
    assert
      .dom('.toast button')
      .hasAttribute('aria-label', 'Dismiss notification');
  });

  test('the Dismiss button removes its toast', async function (assert) {
    const toasts = this.owner.lookup('service:toasts') as ToastsService;
    await render(<template><ToastHost /></template>);
    toasts.success('Vendor saved.');
    await settled();

    await click('.toast button');

    assert.dom('.toast').doesNotExist();
  });
});
