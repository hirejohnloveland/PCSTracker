import Service from '@ember/service';
import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import type RefrigerantLogApiService from 'web/services/refrigerant-log-api';
import type { RefrigerantLogEntryInput } from 'web/services/refrigerant-log-api';

module('Unit | Service | refrigerant-log-api', function (hooks) {
  setupTest(hooks);

  function lookup(owner: { lookup(name: string): unknown }) {
    return owner.lookup(
      'service:refrigerant-log-api',
    ) as RefrigerantLogApiService;
  }

  const input: RefrigerantLogEntryInput = {
    date: '2026-05-15',
    technician: 'Bob Osborne',
    refrigerantType: 'R-22',
    amountLbs: 6,
    notes: 'Recharge',
  };

  test('list asks the API client for /refrigerant-log with the query', async function (assert) {
    const calls: { path: string; query: unknown }[] = [];
    const page = { data: [] };

    class FakeApiClient extends Service {
      getJson(path: string, query: unknown) {
        calls.push({ path, query });
        return Promise.resolve(page);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    const result = await lookup(this.owner).list({
      dateFrom: '2026-05-01',
      technician: 'Bob Osborne',
      page: 2,
    });

    assert.strictEqual(result, page as never);
    assert.deepEqual(calls, [
      {
        path: '/refrigerant-log',
        query: {
          dateFrom: '2026-05-01',
          technician: 'Bob Osborne',
          page: 2,
        },
      },
    ]);
  });

  test('get and refrigerants ask the API client for the entry and the types', async function (assert) {
    const calls: string[] = [];

    class FakeApiClient extends Service {
      getJson(path: string) {
        calls.push(path);
        return Promise.resolve({ data: {} });
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    await lookup(this.owner).get('refrigerantlogentries/1-A');
    await lookup(this.owner).refrigerants();

    assert.deepEqual(calls, [
      '/refrigerant-log/refrigerantlogentries%2F1-A',
      '/refrigerant-log/refrigerants',
    ]);
  });

  test('create posts the entry', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];
    const response = { data: { id: 'refrigerantlogentries/1-A' } };

    class FakeApiClient extends Service {
      postJson(path: string, body: unknown) {
        calls.push({ path, body });
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    assert.strictEqual(
      await lookup(this.owner).create(input),
      response as never,
    );
    assert.deepEqual(calls, [{ path: '/refrigerant-log', body: input }]);
  });

  test('update puts the entry to its id', async function (assert) {
    const calls: { path: string; body: unknown }[] = [];
    const response = { data: { id: 'refrigerantlogentries/1-A' } };

    class FakeApiClient extends Service {
      putJson(path: string, body: unknown) {
        calls.push({ path, body });
        return Promise.resolve(response);
      }
    }
    this.owner.register('service:api-client', FakeApiClient);

    assert.strictEqual(
      await lookup(this.owner).update('refrigerantlogentries/1-A', input),
      response as never,
    );
    assert.deepEqual(calls, [
      { path: '/refrigerant-log/refrigerantlogentries%2F1-A', body: input },
    ]);
  });
});
