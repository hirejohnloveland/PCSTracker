import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import VendorRow from 'web/components/vendor-row';
import type { Vendor } from 'web/services/vendors-api';

module('Integration | Component | vendor-row', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders the name, QuickBooks id and Active status', async function (assert) {
    const vendor: Vendor = {
      id: '1',
      name: 'Acme Parts',
      isActive: true,
      quickbooksId: 'QB-42',
    };

    await render(
      <template>
        <table><tbody><VendorRow @vendor={{vendor}} /></tbody></table>
      </template>,
    );

    assert.dom('td:nth-child(1)').hasText('Acme Parts');
    assert.dom('td.quickbooks-id').hasText('QB-42');
    assert.dom('td:nth-child(3)').hasText('Active');
  });

  test('it renders Inactive for an inactive vendor', async function (assert) {
    const vendor: Vendor = {
      id: '2',
      name: 'Old Supply',
      isActive: false,
      quickbooksId: null,
    };

    await render(
      <template>
        <table><tbody><VendorRow @vendor={{vendor}} /></tbody></table>
      </template>,
    );

    assert.dom('td:nth-child(3)').hasText('Inactive');
  });
});
