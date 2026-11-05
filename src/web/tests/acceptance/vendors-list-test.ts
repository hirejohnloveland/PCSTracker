import Service from '@ember/service';
import { click, currentURL, fillIn, visit } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import type {
  ListVendorsQuery,
  Vendor,
  VendorPage,
} from 'web/services/vendors-api';

const PAGE_SIZE = 25;

const allVendors: Vendor[] = Array.from({ length: 30 }, (_, index) => ({
  id: `vendors/${index + 1}-A`,
  name: `${index < 5 ? 'Acme' : 'Vendor'} ${String(index + 1).padStart(2, '0')}`,
  isActive: index % 2 === 0,
}));

class FakeVendorsApi extends Service {
  list(query: ListVendorsQuery): Promise<VendorPage> {
    const prefix = (query.name ?? '').toLowerCase();
    const matches = allVendors.filter((vendor) =>
      vendor.name.toLowerCase().startsWith(prefix),
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

module('Acceptance | vendors list', function (hooks) {
  setupApplicationTest(hooks);

  hooks.beforeEach(function () {
    this.owner.register('service:vendors-api', FakeVendorsApi);
  });

  test('the navbar leads to the first page of vendors', async function (assert) {
    await visit('/');
    await click('nav a[href="/vendors"]');

    assert.strictEqual(currentURL(), '/vendors');
    assert.dom('table.vendors tbody tr').exists({ count: PAGE_SIZE });
    assert.dom('nav.pagination').containsText('Page 1 of 2');
  });

  test('Next and Previous move between pages', async function (assert) {
    await visit('/vendors');
    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/vendors?page=2');
    assert.dom('table.vendors tbody tr').exists({ count: 5 });
    assert.dom('nav.pagination').containsText('Page 2 of 2');

    await click('nav.pagination a');

    assert.strictEqual(currentURL(), '/vendors');
    assert.dom('table.vendors tbody tr').exists({ count: PAGE_SIZE });
  });

  test('filtering by name shows only matching vendors and returns to page 1', async function (assert) {
    await visit('/vendors?page=2');
    await fillIn('#vendor-name-filter', 'acme');
    await click('form.filter button');

    assert.strictEqual(currentURL(), '/vendors?name=acme');
    assert.dom('table.vendors tbody tr').exists({ count: 5 });
    assert.dom('table.vendors tbody').containsText('Acme 01');
    assert.dom('nav.pagination').containsText('Page 1 of 1');
  });

  test('a filter with no matches says so', async function (assert) {
    await visit('/vendors?name=zzz');

    assert.dom('table.vendors').doesNotExist();
    assert.dom('main').containsText('No vendors found.');
  });
});
