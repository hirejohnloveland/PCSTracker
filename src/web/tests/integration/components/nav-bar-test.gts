import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import NavBar from 'web/components/nav-bar';

module('Integration | Component | nav-bar', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders the brand and the Vendor Management and Customer Management links', async function (assert) {
    await render(<template><NavBar /></template>);

    assert.dom('nav').exists();
    assert.dom('.nav-brand').hasText('PCS Home');
    assert.dom('.nav-links').containsText('Vendor Management');
    assert.dom('.nav-links').containsText('Customer Management');
  });
});
