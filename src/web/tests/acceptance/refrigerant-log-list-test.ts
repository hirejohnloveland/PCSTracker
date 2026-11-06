import Service from '@ember/service';
import { click, currentURL, fillIn, visit } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import type {
  ListRefrigerantLogQuery,
  RefrigerantLogEntry,
  RefrigerantLogEntryResponse,
  RefrigerantLogPage,
  RefrigerantTypesResponse,
} from 'web/services/refrigerant-log-api';
import type {
  ListTechniciansQuery,
  TechnicianPage,
} from 'web/services/technicians-api';

const PAGE_SIZE = 25;
const NAMES = ['Aldo Cipriani', 'Bob Osborne'];

const allEntries: RefrigerantLogEntry[] = Array.from(
  { length: 30 },
  (_, index) => {
    const day = String(30 - index).padStart(2, '0');
    return {
      id: `refrigerantlogentries-${index + 1}-A`,
      date: `2026-04-${day}`,
      technician: NAMES[index % 2] as string,
      refrigerantType: index % 2 === 0 ? 'R-404A' : 'R-22',
      amountLbs: index + 1,
      notes: `Notes ${index + 1}`,
      ticketNumber: `EM-${String(index + 1).padStart(4, '0')}`,
      jobName: null,
      customer:
        index < 3 ? 'Metro Food Market - Bronx' : 'Atlantic Cold Storage',
      vendorInvoiceRef: null,
    };
  },
);

function meta() {
  return { traceId: 'test', timestamp: new Date().toISOString() };
}

class FakeRefrigerantLogApi extends Service {
  static queries: ListRefrigerantLogQuery[] = [];

  get(id: string): Promise<RefrigerantLogEntryResponse> {
    return Promise.resolve({
      data: allEntries.find((entry) => entry.id === id) as RefrigerantLogEntry,
      metadata: meta(),
    });
  }

  refrigerants(): Promise<RefrigerantTypesResponse> {
    return Promise.resolve({
      data: { refrigerants: ['R-22', 'R-404A'] },
      metadata: meta(),
    });
  }

  list(query: ListRefrigerantLogQuery): Promise<RefrigerantLogPage> {
    FakeRefrigerantLogApi.queries.push(query);
    const customer = (query.customer ?? '').toLowerCase();
    const matches = allEntries.filter(
      (entry) =>
        // The API client leaves a blank filter out of the request.
        (!query.dateFrom || entry.date >= query.dateFrom) &&
        (!query.dateTo || entry.date <= query.dateTo) &&
        (!query.technician || entry.technician === query.technician) &&
        (entry.customer ?? '').toLowerCase().includes(customer),
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
      metadata: meta(),
    });
  }
}

class FakeTechniciansApi extends Service {
  list(query: ListTechniciansQuery): Promise<TechnicianPage> {
    return Promise.resolve({
      data: NAMES.map((fullName, index) => ({
        id: `technicians-${index + 1}-A`,
        fullName,
        role: 'Service Tech',
        active: true,
        quickbooksId: null,
        phone: null,
        notes: null,
      })),
      pagination: {
        page: 1,
        pageSize: query.pageSize ?? 25,
        totalCount: NAMES.length,
        totalPages: 1,
      },
      metadata: meta(),
    });
  }
}

module('Acceptance | refrigerant log list', function (hooks) {
  setupApplicationTest(hooks);

  hooks.beforeEach(function () {
    FakeRefrigerantLogApi.queries = [];
    this.owner.register('service:refrigerant-log-api', FakeRefrigerantLogApi);
    this.owner.register('service:technicians-api', FakeTechniciansApi);
  });

  test('the navbar leads to the first page of the log, newest entries first', async function (assert) {
    await visit('/');
    await click('nav a[href="/refrigerant-log"]');

    assert.strictEqual(currentURL(), '/refrigerant-log');
    assert.dom('table.refrigerant-log tbody tr').exists({ count: PAGE_SIZE });
    assert.dom('nav.pagination').containsText('Page 1 of 2');
  });

  test('the table shows the date, technician, ticket, job, customer, refrigerant and amount, and not the notes or id', async function (assert) {
    await visit('/refrigerant-log');

    assert
      .dom('table.refrigerant-log thead')
      .hasText('Date Technician Ticket Job Customer Refrigerant Lbs Actions');
    assert
      .dom('table.refrigerant-log tbody tr:nth-child(1)')
      .hasText(
        '04/30/2026 Aldo Cipriani EM-0001 Metro Food Market - Bronx R-404A 1 Details',
      );
    assert.dom('table.refrigerant-log').doesNotContainText('Notes 1');
    assert
      .dom('table.refrigerant-log')
      .doesNotContainText('refrigerantlogentries');
  });

  test('Add Entry opens the new entry page', async function (assert) {
    await visit('/refrigerant-log');

    await click('table.refrigerant-log tbody tr:nth-child(1) a.button');

    assert.strictEqual(
      currentURL(),
      '/refrigerant-log/refrigerantlogentries-1-A',
    );
  });

  test('Next and Previous move between pages', async function (assert) {
    await visit('/refrigerant-log');
    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/refrigerant-log?page=2');
    assert.dom('table.refrigerant-log tbody tr').exists({ count: 5 });

    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/refrigerant-log');
  });

  test('the technician filter offers every technician and shows only that technician’s entries', async function (assert) {
    await visit('/refrigerant-log?page=2');

    assert
      .dom('#refrigerant-log-technician-filter option')
      .exists({ count: NAMES.length + 1 });

    await fillIn('#refrigerant-log-technician-filter', 'Bob Osborne');

    assert.strictEqual(
      currentURL(),
      '/refrigerant-log?technician=Bob%20Osborne',
    );
    assert.dom('table.refrigerant-log tbody tr').exists({ count: 15 });
    assert.dom('table.refrigerant-log tbody').doesNotContainText('Aldo');
  });

  test('the date range filters the entries and the dates stay in the fields', async function (assert) {
    await visit('/refrigerant-log');

    await fillIn('#refrigerant-log-date-from', '2026-04-10');
    await fillIn('#refrigerant-log-date-to', '2026-04-12');

    assert.strictEqual(
      currentURL(),
      '/refrigerant-log?dateFrom=2026-04-10&dateTo=2026-04-12',
    );
    assert.dom('table.refrigerant-log tbody tr').exists({ count: 3 });
    assert.dom('#refrigerant-log-date-from').hasValue('2026-04-10');
    assert.dom('#refrigerant-log-date-to').hasValue('2026-04-12');
  });

  test('a date range that ends before it starts says so and asks the API for nothing', async function (assert) {
    await visit('/refrigerant-log?dateFrom=2026-04-12&dateTo=2026-04-10');

    assert
      .dom('#refrigerant-log-date-range-error')
      .hasText('Date to must be on or after date from.');
    assert.dom('table.refrigerant-log').doesNotExist();
    assert.deepEqual(FakeRefrigerantLogApi.queries, []);
  });

  test('filtering by customer matches inside the name and returns to page 1', async function (assert) {
    await visit('/refrigerant-log?page=2');
    await fillIn('#refrigerant-log-customer-filter', 'metro');

    assert.strictEqual(currentURL(), '/refrigerant-log?customer=metro');
    assert.dom('table.refrigerant-log tbody tr').exists({ count: 3 });
  });

  test('Clear filters is enabled only while a filter is applied, and clears them all', async function (assert) {
    await visit('/refrigerant-log');
    assert.dom('form.filter button').hasText('Clear filters').isDisabled();

    await fillIn('#refrigerant-log-customer-filter', 'metro');
    await fillIn('#refrigerant-log-technician-filter', 'Aldo Cipriani');
    await fillIn('#refrigerant-log-date-from', '2026-04-01');
    assert.dom('form.filter button').isNotDisabled();

    await click('form.filter button');

    assert.strictEqual(currentURL(), '/refrigerant-log');
    assert.dom('#refrigerant-log-customer-filter').hasValue('');
    assert.dom('#refrigerant-log-technician-filter').hasValue('');
    assert.dom('#refrigerant-log-date-from').hasValue('');
  });

  test('a filter with no matches says so', async function (assert) {
    await visit('/refrigerant-log?customer=zzz');

    assert.dom('table.refrigerant-log').doesNotExist();
    assert.dom('main').containsText('No refrigerant log entries found.');
  });
});
