import { module, test } from 'qunit';
import { setupTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import type ApiClientService from 'web/services/api-client';

module('Unit | Service | api-client', function (hooks) {
  setupTest(hooks);

  const realFetch = window.fetch.bind(window);
  let requests: { url: string; init: RequestInit | undefined }[];

  function stubFetch(response: Response | Error): void {
    requests = [];
    window.fetch = (input, init) => {
      const url =
        typeof input === 'string'
          ? input
          : input instanceof URL
            ? input.href
            : input.url;
      requests.push({ url, init });
      return response instanceof Error
        ? Promise.reject(response)
        : Promise.resolve(response);
    };
  }

  hooks.afterEach(function () {
    window.fetch = realFetch;
  });

  test('get sends the query under /api and skips blank values', async function (assert) {
    stubFetch(Response.json({ data: [] }));
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    const body = await client.getJson('/vendors', {
      name: '',
      page: 2,
      other: undefined,
    });

    assert.deepEqual(body, { data: [] });
    assert.strictEqual(requests[0]?.url, '/api/vendors?page=2');
    assert.strictEqual(requests[0]?.init?.method, 'GET');
  });

  test('get sends a boolean query value, including false', async function (assert) {
    stubFetch(Response.json({ data: [] }));
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    await client.getJson('/technicians', { active: false, pageSize: 100 });

    assert.strictEqual(
      requests[0]?.url,
      '/api/technicians?active=false&pageSize=100',
    );
  });

  test('post sends the body as JSON', async function (assert) {
    stubFetch(Response.json({ data: { id: 'vendors/1-A' } }, { status: 201 }));
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    await client.postJson('/vendors', { name: 'Acme' });

    assert.strictEqual(requests[0]?.url, '/api/vendors');
    assert.strictEqual(requests[0]?.init?.method, 'POST');
    assert.strictEqual(requests[0]?.init?.body, '{"name":"Acme"}');
  });

  test('put sends the body as JSON', async function (assert) {
    stubFetch(Response.json({ data: { id: 'vendors-1-A' } }));
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    await client.putJson('/vendors/vendors-1-A', { name: 'Acme' });

    assert.strictEqual(requests[0]?.url, '/api/vendors/vendors-1-A');
    assert.strictEqual(requests[0]?.init?.method, 'PUT');
    assert.strictEqual(requests[0]?.init?.body, '{"name":"Acme"}');
    assert.strictEqual(
      (requests[0]?.init?.headers as Record<string, string>)['Content-Type'],
      'application/json',
    );
  });

  test('post without a body sends no body and no content type', async function (assert) {
    stubFetch(Response.json({ data: {} }));
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    await client.postJson('/quickbooks/vendors/sync');

    assert.strictEqual(requests[0]?.url, '/api/quickbooks/vendors/sync');
    assert.strictEqual(requests[0]?.init?.body, undefined);
    assert.notOk(
      (requests[0]?.init?.headers as Record<string, string>)['Content-Type'],
    );
  });

  test('a problem response becomes an ApiError with its errors and trace id', async function (assert) {
    stubFetch(
      Response.json(
        {
          type: 'https://api.example.com/problems/validation',
          title: 'Validation Failed',
          status: 400,
          detail: 'Vendor name is required.',
          traceId: 'trace-1',
          errors: [
            {
              code: 'InvalidRequest',
              detail: 'Vendor name is required.',
              pointer: '#/name',
            },
          ],
        },
        {
          status: 400,
          headers: { 'Content-Type': 'application/problem+json' },
        },
      ),
    );
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    try {
      await client.postJson('/vendors', { name: '' });
      assert.true(false, 'expected an ApiError');
    } catch (error) {
      assert.true(error instanceof ApiError);
      const apiError = error as ApiError;
      assert.strictEqual(apiError.status, 400);
      assert.strictEqual(apiError.traceId, 'trace-1');
      assert.strictEqual(apiError.errors[0]?.pointer, '#/name');
    }
  });

  test('a failure that is not problem JSON still becomes an ApiError', async function (assert) {
    stubFetch(
      new Response('Bad Gateway', { status: 502, statusText: 'Bad Gateway' }),
    );
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    try {
      await client.getJson('/vendors');
      assert.true(false, 'expected an ApiError');
    } catch (error) {
      assert.true(error instanceof ApiError);
      assert.strictEqual((error as ApiError).status, 502);
      assert.deepEqual((error as ApiError).errors, []);
    }
  });

  test('a network failure becomes an ApiError with status 0', async function (assert) {
    stubFetch(new TypeError('Failed to fetch'));
    const client = this.owner.lookup('service:api-client') as ApiClientService;

    try {
      await client.getJson('/vendors');
      assert.true(false, 'expected an ApiError');
    } catch (error) {
      assert.true(error instanceof ApiError);
      assert.strictEqual((error as ApiError).status, 0);
    }
  });
});
