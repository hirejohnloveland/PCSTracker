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
});
