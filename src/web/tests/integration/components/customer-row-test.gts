import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import CustomerRow from 'web/components/customer-row';
import type { Customer } from 'web/services/customers-api';

module('Integration | Component | customer-row', function (hooks) {
  setupRenderingTest(hooks);

  test('it renders the name, the description and a Details button', async function (assert) {
    const customer: Customer = {
      id: 'customers-1-A',
      name: 'Acme Parts',
      quickbooksId: 'QB-42',
      description: 'Refrigeration parts',
      contact: 'Pat',
      phone: '555-0100',
      notes: 'Net 30',
    };

    await render(
      <template>
        <table><tbody><CustomerRow @customer={{customer}} /></tbody></table>
      </template>,
    );

    assert.dom('td:nth-child(1)').hasText('Acme Parts');
    assert.dom('td.description').hasText('Refrigeration parts');
    assert.dom('td.actions a.button').hasText('Details');
  });

  test('it renders a blank description for a customer with none', async function (assert) {
    const customer: Customer = {
      id: 'customers-2-A',
      name: 'Old Supply',
      quickbooksId: null,
      description: null,
      contact: null,
      phone: null,
      notes: null,
    };

    await render(
      <template>
        <table><tbody><CustomerRow @customer={{customer}} /></tbody></table>
      </template>,
    );

    assert.dom('td.description').hasText('');
  });

  test('it shows only the name, description and actions cells', async function (assert) {
    const customer: Customer = {
      id: 'customers-1-A',
      name: 'Acme Parts',
      quickbooksId: 'QB-42',
      description: null,
      contact: 'Pat',
      phone: '555-0100',
      notes: 'Net 30',
    };

    await render(
      <template>
        <table><tbody><CustomerRow @customer={{customer}} /></tbody></table>
      </template>,
    );

    assert.dom('td').exists({ count: 3 });
    assert.dom('tr').doesNotContainText('customers-1-A');
    assert.dom('tr').doesNotContainText('QB-42');
  });
});
