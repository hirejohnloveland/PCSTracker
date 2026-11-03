import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import NavBar from 'web/components/nav-bar';

module('Integration | Component | nav-bar', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders the brand and the Home link', async function (assert) {
    await render(<template><NavBar /></template>);

    assert.dom('nav').exists();
    assert.dom('.nav-brand').hasText('Tracker');
    assert.dom('.nav-links').containsText('Home');
  });
});
