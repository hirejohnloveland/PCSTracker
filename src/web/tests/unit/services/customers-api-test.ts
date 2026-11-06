import Service from '@ember/service';
import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import type CustomersApiService from 'web/services/customers-api';
import type { CustomerPage } from 'web/services/customers-api';

module('Unit | Service | customers-api', function (hooks) {
  setupTest(hooks);

  test('list asks the API client for /customers with the query', async function (assert) {
    const calls: { path: string; query: unknown }[] = [];
    const page: CustomerPage = {
      data: [],
      pagination: { page: 2, pageSize: 25, totalCount: 0, totalPages: 0 },
      metadata: { traceId: 'test', timestamp: '2026-11-03T00:00:00Z' },
    };

    class FakeApiClient extends Service {
      getJson(path: string, query: unknown) {
        calls.push({ path, query });
        return Promise.resolve(page);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const customersApi = this.owner.lookup(
      'service:customers-api',
    ) as CustomersApiService;
    const result = await customersApi.list({ name: 'Ac', page: 2 });

    assert.strictEqual(result, page);
    assert.deepEqual(calls, [
      { path: '/customers', query: { name: 'Ac', page: 2 } },
    ]);
  });

  test('get asks the API client for the customer by id', async function (assert) {
    const calls: string[] = [];
    const response = { data: { id: 'customers-1-A' } };

    class FakeApiClient extends Service {
      getJson(path: string) {
        calls.push(path);
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const customersApi = this.owner.lookup(
      'service:customers-api',
    ) as CustomersApiService;

    assert.strictEqual(
      await customersApi.get('customers-1-A'),
      response as never,
    );
    assert.deepEqual(calls, ['/customers/customers-1-A']);
  });

  test('update puts the customer to its id', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];
    const response = { data: { id: 'customers-1-A' } };

    class FakeApiClient extends Service {
      putJson(path: string, body: unknown) {
        calls.push({ path, body });
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const customersApi = this.owner.lookup(
      'service:customers-api',
    ) as CustomersApiService;
    const customer = { name: 'Acme' };

    assert.strictEqual(
      await customersApi.update('customers-1-A', customer),
      response as never,
    );
    assert.deepEqual(calls, [
      { path: '/customers/customers-1-A', body: customer },
    ]);
  });
});
