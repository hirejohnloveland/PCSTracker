import Service from '@ember/service';
import { click, currentURL, fillIn, visit } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import type {
  ListTechniciansQuery,
  Technician,
  TechnicianPage,
  TechnicianResponse,
  TechnicianRolesResponse,
} from 'web/services/technicians-api';

const PAGE_SIZE = 25;

const allTechnicians: Technician[] = Array.from({ length: 30 }, (_, index) => ({
  id: `technicians-${index + 1}-A`,
  fullName: `${index < 5 ? 'Bob' : 'Tech'} ${String(index + 1).padStart(2, '0')}`,
  role: index === 1 ? 'Installer' : 'Service Tech',
  active: index !== 1,
  quickbooksId: null,
  phone: null,
  notes: null,
}));

class FakeTechniciansApi extends Service {
  list(query: ListTechniciansQuery): Promise<TechnicianPage> {
    const text = (query.name ?? '').toLowerCase();
    const matches = allTechnicians.filter((technician) =>
      technician.fullName.toLowerCase().includes(text),
    );
    const page = query.page ?? 1;
    const start = (page - 1) * PAGE_SIZE;

    return Promise.resolve({
      data: matches.slice(start, start + PAGE_SIZE),
      pagination: {
        page,
        pageSize: PAGE_SIZE,
        totalCount: matches.length,
        totalPages: Math.ceil(matches.length / PAGE_SIZE),
      },
      metadata: { traceId: 'test', timestamp: new Date().toISOString() },
    });
  }

  get(id: string): Promise<TechnicianResponse> {
    const technician = allTechnicians.find((candidate) => candidate.id === id);
    if (technician === undefined) {
      return Promise.reject(new Error(`No technician ${id}`));
    }

    return Promise.resolve({
      data: technician,
      metadata: { traceId: 'test', timestamp: new Date().toISOString() },
    });
  }

  roles(): Promise<TechnicianRolesResponse> {
    return Promise.resolve({
      data: { roles: ['Service Tech', 'Installer', 'Other'] },
      metadata: { traceId: 'test', timestamp: new Date().toISOString() },
    });
  }
}

class FakeQuickbooksApi extends Service {
  static failure: ApiError = new ApiError({ status: 500, title: 'unset' });

  syncTechnicians(): Promise<never> {
    return Promise.reject(FakeQuickbooksApi.failure);
  }
}

module('Acceptance | technicians list', function (hooks) {
  setupApplicationTest(hooks);

  hooks.beforeEach(function () {
    this.owner.register('service:technicians-api', FakeTechniciansApi);
    this.owner.register('service:quickbooks-api', FakeQuickbooksApi);
  });

  test('the navbar leads to the first page of technicians', async function (assert) {
    await visit('/');
    await click('nav a[href="/technicians"]');

    assert.strictEqual(currentURL(), '/technicians');
    assert.dom('table.technicians tbody tr').exists({ count: PAGE_SIZE });
    assert.dom('nav.pagination').containsText('Page 1 of 2');
  });

  test('the table shows the full name, role and active, and not the technician id', async function (assert) {
    await visit('/technicians');

    assert
      .dom('table.technicians thead')
      .hasText('Full Name Role Active Actions');
    assert
      .dom('table.technicians tbody tr:nth-child(1) td.role')
      .hasText('Service Tech');
    assert
      .dom('table.technicians tbody tr:nth-child(1) td.active')
      .hasText('Yes');
    assert
      .dom('table.technicians tbody tr:nth-child(2) td.role')
      .hasText('Installer');
    assert
      .dom('table.technicians tbody tr:nth-child(2) td.active')
      .hasText('No');
    assert.dom('table.technicians').doesNotContainText('technicians-1-A');
  });

  test('a Details button opens that technician’s details page', async function (assert) {
    await visit('/technicians');

    await click('table.technicians tbody tr:nth-child(1) a.button');

    assert.strictEqual(currentURL(), '/technicians/technicians-1-A');
  });

  test('Next and Previous move between pages', async function (assert) {
    await visit('/technicians');
    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/technicians?page=2');
    assert.dom('table.technicians tbody tr').exists({ count: 5 });

    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/technicians');
    assert.dom('table.technicians tbody tr').exists({ count: PAGE_SIZE });
  });

  test('filtering by name shows only matching technicians and returns to page 1', async function (assert) {
    await visit('/technicians?page=2');
    await fillIn('#technician-name-filter', 'bob');

    assert.strictEqual(currentURL(), '/technicians?name=bob');
    assert.dom('table.technicians tbody tr').exists({ count: 5 });
    assert.dom('table.technicians tbody').containsText('Bob 01');
  });

  test('Clear filters is enabled only while a filter is applied, and clears it', async function (assert) {
    await visit('/technicians');
    assert.dom('form.filter button').hasText('Clear filters').isDisabled();

    await fillIn('#technician-name-filter', 'bob');
    assert.dom('form.filter button').isNotDisabled();

    await click('form.filter button');

    assert.strictEqual(currentURL(), '/technicians');
    assert.dom('#technician-name-filter').hasValue('');
  });

  test('a filter with no matches says so', async function (assert) {
    await visit('/technicians?name=zzz');

    assert.dom('table.technicians').doesNotExist();
    assert.dom('main').containsText('No technicians found.');
  });

  test('Sync with QuickBooks shows the API message in a warning dialog that Close dismisses', async function (assert) {
    FakeQuickbooksApi.failure = new ApiError({
      status: 409,
      title: 'Conflict',
      detail: 'This feature is not implemented yet.',
      traceId: 'trace-409',
    });
    await visit('/technicians');
    assert.dom('dialog').doesNotExist();

    await click('.page-header button');

    assert.dom('dialog[open]').hasClass('warning');
    assert.dom('dialog').containsText('This feature is not implemented yet.');

    await click('dialog button');

    assert.dom('dialog').doesNotExist();
    assert.strictEqual(currentURL(), '/technicians');
  });
});
