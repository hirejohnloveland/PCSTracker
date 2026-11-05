import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import VendorRow from 'web/components/vendor-row';
import type { Vendor } from 'web/services/vendors-api';

module('Integration | Component | vendor-row', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders the name, description, contact, phone, notes, QuickBooks id and Active status', async function (assert) {
    const vendor: Vendor = {
      id: '1',
      name: 'Acme Parts',
      isActive: true,
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
    assert.dom('td.contact').hasText('Pat');
    assert.dom('td.phone').hasText('555-0100');
    assert.dom('td.notes').hasText('Net 30');
    assert.dom('td.quickbooks-id').hasText('QB-42');
    assert.dom('td:last-child').hasText('Active');
  });

  test('it renders blank cells and Inactive for an inactive vendor with no optional details', async function (assert) {
    const vendor: Vendor = {
      id: '2',
      name: 'Old Supply',
      isActive: false,
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
    assert.dom('td.contact').hasText('');
    assert.dom('td.phone').hasText('');
    assert.dom('td.notes').hasText('');
    assert.dom('td.quickbooks-id').hasText('');
    assert.dom('td:last-child').hasText('Inactive');
  });

  test('it does not show the vendor id', async function (assert) {
    const vendor: Vendor = {
      id: 'vendors/1-A',
      name: 'Acme Parts',
      isActive: true,
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

    assert.dom('tr').doesNotContainText('vendors/1-A');
  });
});
