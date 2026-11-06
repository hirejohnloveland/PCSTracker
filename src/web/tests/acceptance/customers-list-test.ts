import Service from '@ember/service';
import {
  click,
  currentURL,
  fillIn,
  find,
  settled,
  triggerEvent,
  visit,
} from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import type {
  Customer,
  CustomerPage,
  CustomerResponse,
  ListCustomersQuery,
} from 'web/services/customers-api';

const PAGE_SIZE = 25;

const allCustomers: Customer[] = Array.from({ length: 30 }, (_, index) => ({
  id: `customers-${index + 1}-A`,
  name: `${index < 5 ? 'Acme' : 'Customer'} ${String(index + 1).padStart(2, '0')}`,
  quickbooksId: index === 0 ? 'qb-1' : null,
  description: index === 0 ? 'Refrigeration parts' : null,
  contact: null,
  phone: null,
  notes: null,
}));

class FakeCustomersApi extends Service {
  static calls: ListCustomersQuery[] = [];

  list(query: ListCustomersQuery): Promise<CustomerPage> {
    FakeCustomersApi.calls.push(query);
    const text = (query.name ?? '').toLowerCase();
    const matches = allCustomers.filter((customer) =>
      customer.name.toLowerCase().includes(text),
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
}

class FakeCustomerDetailsApi extends FakeCustomersApi {
  get(id: string): Promise<CustomerResponse> {
    const customer = allCustomers.find((candidate) => candidate.id === id);
    if (customer === undefined) {
      return Promise.reject(new Error(`No customer ${id}`));
    }

    return Promise.resolve({
      data: customer,
      metadata: { traceId: 'test', timestamp: new Date().toISOString() },
    });
  }
}

class FakeQuickbooksApi extends Service {
  static failure: ApiError = new ApiError({ status: 500, title: 'unset' });

  syncCustomers(): Promise<never> {
    return Promise.reject(FakeQuickbooksApi.failure);
  }
}

module('Acceptance | customers list', function (hooks) {
  setupApplicationTest(hooks);

  hooks.beforeEach(function () {
    FakeCustomersApi.calls = [];
    this.owner.register('service:customers-api', FakeCustomerDetailsApi);
    this.owner.register('service:quickbooks-api', FakeQuickbooksApi);
  });

  test('the navbar leads to the first page of customers', async function (assert) {
    await visit('/');
    await click('nav a[href="/customers"]');

    assert.strictEqual(currentURL(), '/customers');
    assert.dom('table.customers tbody tr').exists({ count: PAGE_SIZE });
    assert.dom('nav.pagination').containsText('Page 1 of 2');
  });

  test('the table shows only the name and description, and not the customer id', async function (assert) {
    await visit('/customers');

    assert.dom('table.customers thead').hasText('Name Description Actions');
    assert
      .dom('table.customers tbody tr:nth-child(1) td.description')
      .hasText('Refrigeration parts');
    assert
      .dom('table.customers tbody tr:nth-child(2) td.description')
      .hasText('');
    assert.dom('table.customers').doesNotContainText('customers-1-A');
  });

  test('a Details button opens that customer’s details page', async function (assert) {
    await visit('/customers');

    assert
      .dom('table.customers tbody tr:nth-child(1) a.button')
      .hasText('Details');
    await click('table.customers tbody tr:nth-child(1) a.button');

    assert.strictEqual(currentURL(), '/customers/customers-1-A');
  });

  test('Next and Previous move between pages', async function (assert) {
    await visit('/customers');
    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/customers?page=2');
    assert.dom('table.customers tbody tr').exists({ count: 5 });
    assert.dom('nav.pagination').containsText('Page 2 of 2');

    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/customers');
    assert.dom('table.customers tbody tr').exists({ count: PAGE_SIZE });
  });

  test('filtering by name shows only matching customers and returns to page 1', async function (assert) {
    await visit('/customers?page=2');
    await fillIn('#customer-name-filter', 'acme');

    assert.strictEqual(currentURL(), '/customers?name=acme');
    assert.dom('table.customers tbody tr').exists({ count: 5 });
    assert.dom('table.customers tbody').containsText('Acme 01');
    assert.dom('nav.pagination').containsText('Page 1 of 1');
  });

  test('typing quickly sends one request for the final text', async function (assert) {
    await visit('/customers');
    const input = find('#customer-name-filter') as HTMLInputElement;

    for (const text of ['a', 'ac', 'acm']) {
      input.value = text;
      input.dispatchEvent(new Event('input', { bubbles: true }));
    }
    await settled();

    assert.strictEqual(currentURL(), '/customers?name=acm');
    assert.deepEqual(
      FakeCustomersApi.calls.map((query) => query.name),
      ['', 'acm'],
    );
  });

  test('pressing Enter applies the filter', async function (assert) {
    await visit('/customers');
    const input = find('#customer-name-filter') as HTMLInputElement;
    input.value = 'acme';

    await triggerEvent('form.filter', 'submit');

    assert.strictEqual(currentURL(), '/customers?name=acme');
    assert.dom('table.customers tbody tr').exists({ count: 5 });
  });

  test('Clear filters is enabled only while a filter is applied', async function (assert) {
    await visit('/customers');
    assert.dom('form.filter button').hasText('Clear filters').isDisabled();

    await fillIn('#customer-name-filter', 'acme');
    assert.dom('form.filter button').isNotDisabled();
  });

  test('Clear filters empties the filter and returns to the full first page', async function (assert) {
    await visit('/customers?name=acme');
    assert.dom('table.customers tbody tr').exists({ count: 5 });

    await click('form.filter button');

    assert.strictEqual(currentURL(), '/customers');
    assert.dom('#customer-name-filter').hasValue('');
    assert.dom('table.customers tbody tr').exists({ count: PAGE_SIZE });
    assert.dom('form.filter button').isDisabled();
  });

  test('a filter with no matches says so', async function (assert) {
    await visit('/customers?name=zzz');

    assert.dom('table.customers').doesNotExist();
    assert.dom('main').containsText('No customers found.');
  });

  test('Sync with QuickBooks shows the API message in a warning dialog that Close dismisses', async function (assert) {
    FakeQuickbooksApi.failure = new ApiError({
      status: 409,
      title: 'Conflict',
      detail: 'This feature is not implemented yet.',
      traceId: 'trace-409',
    });
    await visit('/customers');
    assert.dom('dialog').doesNotExist();

    await click('.page-header button');

    assert.dom('dialog[open]').exists();
    assert.dom('dialog').hasClass('warning');
    assert.dom('dialog').containsText('This feature is not implemented yet.');
    assert.dom('dialog .trace-id').doesNotExist();

    await click('dialog button');

    assert.dom('dialog').doesNotExist();
    assert.strictEqual(currentURL(), '/customers');
  });

  test('an unexpected failure shows an error dialog with the trace id', async function (assert) {
    FakeQuickbooksApi.failure = new ApiError({
      status: 500,
      title: 'Internal Server Error',
      detail: 'An unexpected error occurred.',
      traceId: 'trace-500',
    });
    await visit('/customers');

    await click('.page-header button');

    assert.dom('dialog[open]').hasClass('error');
    assert.dom('dialog').containsText('An unexpected error occurred.');
    assert.dom('dialog .trace-id').containsText('trace-500');
  });
});
