import Service from '@ember/service';
import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import type TechniciansApiService from 'web/services/technicians-api';
import type { TechnicianPage } from 'web/services/technicians-api';

module('Unit | Service | technicians-api', function (hooks) {
  setupTest(hooks);

  function lookup(owner: { lookup(name: string): unknown }) {
    return owner.lookup('service:technicians-api') as TechniciansApiService;
  }

  test('list asks the API client for /technicians with the query', async function (assert) {
    const calls: { path: string; query: unknown }[] = [];
    const page: TechnicianPage = {
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

    const result = await lookup(this.owner).list({ name: 'Bo', page: 2 });

    assert.strictEqual(result, page);
    assert.deepEqual(calls, [
      { path: '/technicians', query: { name: 'Bo', page: 2 } },
    ]);
  });

  test('get asks the API client for the technician by id', async function (assert) {
    const calls: string[] = [];
    const response = { data: { id: 'technicians-1-A' } };

    class FakeApiClient extends Service {
      getJson(path: string) {
        calls.push(path);
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    assert.strictEqual(
      await lookup(this.owner).get('technicians-1-A'),
      response as never,
    );
    assert.deepEqual(calls, ['/technicians/technicians-1-A']);
  });

  test('roles asks the API client for /technicians/roles', async function (assert) {
    const calls: string[] = [];
    const response = { data: { roles: ['Service Tech'] } };

    class FakeApiClient extends Service {
      getJson(path: string) {
        calls.push(path);
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    assert.strictEqual(await lookup(this.owner).roles(), response as never);
    assert.deepEqual(calls, ['/technicians/roles']);
  });

  test('update puts the technician to its id', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];
    const response = { data: { id: 'technicians-1-A' } };

    class FakeApiClient extends Service {
      putJson(path: string, body: unknown) {
        calls.push({ path, body });
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);
    const technician = { fullName: 'Bob Osborne', role: 'Service Tech' };

    assert.strictEqual(
      await lookup(this.owner).update('technicians-1-A', technician),
      response as never,
    );
    assert.deepEqual(calls, [
      { path: '/technicians/technicians-1-A', body: technician },
    ]);
  });
});
