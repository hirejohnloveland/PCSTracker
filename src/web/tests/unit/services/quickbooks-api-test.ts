import Service from '@ember/service';
import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import type QuickbooksApiService from 'web/services/quickbooks-api';

module('Unit | Service | quickbooks-api', function (hooks) {
  setupTest(hooks);

  test('syncVendors posts to /quickbooks/vendors/sync with no body', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];

    class FakeApiClient extends Service {
      postJson(path: string, body?: unknown) {
        calls.push({ path, body });
        return Promise.resolve({ data: { vendorsSynced: 0 } });
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const quickbooksApi = this.owner.lookup(
      'service:quickbooks-api',
    ) as QuickbooksApiService;
    await quickbooksApi.syncVendors();

    assert.deepEqual(calls, [
      { path: '/quickbooks/vendors/sync', body: undefined },
    ]);
  });

  test('syncCustomers posts to /quickbooks/customers/sync with no body', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];

    class FakeApiClient extends Service {
      postJson(path: string, body?: unknown) {
        calls.push({ path, body });
        return Promise.resolve({ data: { customersSynced: 0 } });
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const quickbooksApi = this.owner.lookup(
      'service:quickbooks-api',
    ) as QuickbooksApiService;
    await quickbooksApi.syncCustomers();

    assert.deepEqual(calls, [
      { path: '/quickbooks/customers/sync', body: undefined },
    ]);
  });

  test('syncTechnicians posts to /quickbooks/technicians/sync with no body', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];

    class FakeApiClient extends Service {
      postJson(path: string, body?: unknown) {
        calls.push({ path, body });
        return Promise.resolve({ data: { techniciansSynced: 0 } });
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const quickbooksApi = this.owner.lookup(
      'service:quickbooks-api',
    ) as QuickbooksApiService;
    await quickbooksApi.syncTechnicians();

    assert.deepEqual(calls, [
      { path: '/quickbooks/technicians/sync', body: undefined },
    ]);
  });
});
