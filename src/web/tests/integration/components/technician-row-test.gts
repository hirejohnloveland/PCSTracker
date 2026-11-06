import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import TechnicianRow from 'web/components/technician-row';
import type { Technician } from 'web/services/technicians-api';

module('Integration | Component | technician-row', function (hooks) {
  setupRenderingTest(hooks);

  const technician: Technician = {
    id: 'technicians-1-A',
    fullName: 'Bob Osborne',
    role: 'Service Tech',
    active: true,
    quickbooksId: 'QB-42',
    phone: '214-309-2915',
    notes: 'Nights',
  };

  test('it renders the full name, the role, Yes for an active technician and a Details button', async function (assert) {
    await render(
      <template>
        <table><tbody><TechnicianRow
              @technician={{technician}}
            /></tbody></table>
      </template>,
    );

    assert.dom('td:nth-child(1)').hasText('Bob Osborne');
    assert.dom('td.role').hasText('Service Tech');
    assert.dom('td.active').hasText('Yes');
    assert.dom('td.actions a.button').hasText('Details');
  });

  test('it renders No for an inactive technician', async function (assert) {
    const inactive: Technician = { ...technician, active: false };

    await render(
      <template>
        <table><tbody><TechnicianRow @technician={{inactive}} /></tbody></table>
      </template>,
    );

    assert.dom('td.active').hasText('No');
  });

  test('it shows only the name, role, active and actions cells', async function (assert) {
    await render(
      <template>
        <table><tbody><TechnicianRow
              @technician={{technician}}
            /></tbody></table>
      </template>,
    );

    assert.dom('td').exists({ count: 4 });
    assert.dom('tr').doesNotContainText('technicians-1-A');
    assert.dom('tr').doesNotContainText('QB-42');
  });
});
