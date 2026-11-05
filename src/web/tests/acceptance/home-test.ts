import { module, test } from 'qunit';
import { visit, currentURL } from '@ember/test-helpers';
import { setupApplicationTest } from 'web/tests/helpers';

module('Acceptance | home', function (hooks) {
  setupApplicationTest(hooks);

  test('visiting / shows the navbar and the heading', async function (assert) {
    await visit('/');

    assert.strictEqual(currentURL(), '/');
    assert.dom('nav').exists();
    assert.dom('h1').hasText('PCS Tracker');
  });
});
