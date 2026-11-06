import { waitUntil } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import type ToastsService from 'web/services/toasts';

module('Unit | Service | toasts', function (hooks) {
  setupTest(hooks);

  function lookup(owner: { lookup(name: string): unknown }): ToastsService {
    return owner.lookup('service:toasts') as ToastsService;
  }

  test('success adds a success toast with the message', function (assert) {
    const toasts = lookup(this.owner);

    toasts.success('Vendor saved.');

    assert.strictEqual(toasts.items.length, 1);
    assert.strictEqual(toasts.items[0]?.kind, 'success');
    assert.strictEqual(toasts.items[0]?.message, 'Vendor saved.');
  });

  test('toasts stack, and dismiss removes only the one named', function (assert) {
    const toasts = lookup(this.owner);
    toasts.success('First');
    toasts.success('Second');
    const [first, second] = toasts.items;

    toasts.dismiss(first!.id);

    assert.deepEqual(toasts.items, [second]);
  });

  test('a toast dismisses itself after the auto-dismiss time', async function (assert) {
    const toasts = lookup(this.owner);
    toasts.autoDismissMs = 10;

    toasts.success('Vendor saved.');
    assert.strictEqual(toasts.items.length, 1);

    await waitUntil(() => toasts.items.length === 0, { timeout: 1000 });

    assert.strictEqual(toasts.items.length, 0);
  });
});
