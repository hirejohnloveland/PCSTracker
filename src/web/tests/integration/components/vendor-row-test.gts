import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import VendorRow from 'web/components/vendor-row';
import type { Vendor } from 'web/services/vendors-api';

module('Integration | Component | vendor-row', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders the name, the description and a Details button', async function (assert) {
    const vendor: Vendor = {
      id: 'vendors-1-A',
      name: 'Acme Parts',
      quickbooksId: 'QB-42',
      description: 'Refrigeration parts',
      contact: 'Pat',
      phone: '555-0100',
      notes: 'Net 30',
    };

    await render(
      <template>
        <table><tbody><VendorRow @vendor={{vendor}} /></tbody></table>
      </template>,
    );

    assert.dom('td:nth-child(1)').hasText('Acme Parts');
    assert.dom('td.description').hasText('Refrigeration parts');
    assert.dom('td.actions a.button').hasText('Details');
  });

  test('it renders a blank description for a vendor with none', async function (assert) {
    const vendor: Vendor = {
      id: 'vendors-2-A',
      name: 'Old Supply',
      quickbooksId: null,
      description: null,
      contact: null,
      phone: null,
      notes: null,
    };

    await render(
      <template>
        <table><tbody><VendorRow @vendor={{vendor}} /></tbody></table>
      </template>,
    );

    assert.dom('td.description').hasText('');
  });

  test('it shows only the name, description and actions cells', async function (assert) {
    const vendor: Vendor = {
      id: 'vendors-1-A',
      name: 'Acme Parts',
      quickbooksId: 'QB-42',
      description: null,
      contact: 'Pat',
      phone: '555-0100',
      notes: 'Net 30',
    };

    await render(
      <template>
        <table><tbody><VendorRow @vendor={{vendor}} /></tbody></table>
      </template>,
    );

    assert.dom('td').exists({ count: 3 });
    assert.dom('tr').doesNotContainText('vendors-1-A');
    assert.dom('tr').doesNotContainText('QB-42');
  });
});
