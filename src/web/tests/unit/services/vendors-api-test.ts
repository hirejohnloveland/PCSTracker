import Service from '@ember/service';
import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import type VendorsApiService from 'web/services/vendors-api';
import type { VendorPage } from 'web/services/vendors-api';

module('Unit | Service | vendors-api', function (hooks) {
  setupTest(hooks);

  test('list asks the API client for /vendors with the query', async function (assert) {
    const calls: { path: string; query: unknown }[] = [];
    const page: VendorPage = {
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

    const vendorsApi = this.owner.lookup(
      'service:vendors-api',
    ) as VendorsApiService;
    const result = await vendorsApi.list({ name: 'Ac', page: 2 });

    assert.strictEqual(result, page);
    assert.deepEqual(calls, [
      { path: '/vendors', query: { name: 'Ac', page: 2 } },
    ]);
  });

  test('get asks the API client for the vendor by id', async function (assert) {
    const calls: string[] = [];
    const response = { data: { id: 'vendors-1-A' } };

    class FakeApiClient extends Service {
      getJson(path: string) {
        calls.push(path);
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const vendorsApi = this.owner.lookup(
      'service:vendors-api',
    ) as VendorsApiService;

    assert.strictEqual(await vendorsApi.get('vendors-1-A'), response as never);
    assert.deepEqual(calls, ['/vendors/vendors-1-A']);
  });

  test('update puts the vendor to its id', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];
    const response = { data: { id: 'vendors-1-A' } };

    class FakeApiClient extends Service {
      putJson(path: string, body: unknown) {
        calls.push({ path, body });
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const vendorsApi = this.owner.lookup(
      'service:vendors-api',
    ) as VendorsApiService;
    const vendor = { name: 'Acme' };

    assert.strictEqual(
      await vendorsApi.update('vendors-1-A', vendor),
      response as never,
    );
    assert.deepEqual(calls, [{ path: '/vendors/vendors-1-A', body: vendor }]);
  });
});
