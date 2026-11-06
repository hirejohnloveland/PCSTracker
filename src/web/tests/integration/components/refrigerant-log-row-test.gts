import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import RefrigerantLogRow from 'web/components/refrigerant-log-row';
import type { RefrigerantLogEntry } from 'web/services/refrigerant-log-api';

module('Integration | Component | refrigerant-log-row', function (hooks) {
  setupRenderingTest(hooks);

  const entry: RefrigerantLogEntry = {
    id: 'refrigerantlogentries/1-A',
    date: '2026-05-15',
    technician: 'Aldo Cipriani',
    refrigerantType: 'R-404A',
    amountLbs: 24.5,
    notes: 'Leak repair - 3 cylinders recharged',
    ticketNumber: 'EM-0415',
    jobName: 'ACS-BK-01',
    customer: 'Atlantic Cold Storage - Brooklyn',
    vendorInvoiceRef: 'URI-88430',
  };

  test('it renders the entry as a table row with a Details button', async function (assert) {
    await render(
      <template>
        <table><tbody><RefrigerantLogRow @entry={{entry}} /></tbody></table>
      </template>,
    );

    assert.dom('td.date').hasText('05/15/2026');
    assert.dom('td:nth-child(2)').hasText('Aldo Cipriani');
    assert.dom('td.ticket').hasText('EM-0415');
    assert.dom('td.job').hasText('ACS-BK-01');
    assert.dom('td.customer').hasText('Atlantic Cold Storage - Brooklyn');
    assert.dom('td.refrigerant').hasText('R-404A');
    assert.dom('td.amount').hasText('24.5');
    assert.dom('td.actions a.button').hasText('Details');
  });

  test('it leaves the optional cells empty and keeps the notes and invoice ref off the row', async function (assert) {
    const minimal: RefrigerantLogEntry = {
      ...entry,
      ticketNumber: null,
      jobName: null,
      customer: null,
      vendorInvoiceRef: null,
    };

    await render(
      <template>
        <table><tbody><RefrigerantLogRow @entry={{minimal}} /></tbody></table>
      </template>,
    );

    assert.dom('td.ticket').hasText('');
    assert.dom('td.job').hasText('');
    assert.dom('td.customer').hasText('');
    assert.dom('td').exists({ count: 8 });
    assert.dom('tr').doesNotContainText('Leak repair');
    assert.dom('tr').doesNotContainText('URI-88430');
    assert.dom('tr').doesNotContainText('refrigerantlogentries');
  });
});
