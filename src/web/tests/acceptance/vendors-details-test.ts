import Service from '@ember/service';
import { click, currentURL, fillIn, visit } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import type {
  ListVendorsQuery,
  Vendor,
  VendorPage,
  VendorResponse,
  VendorUpdate,
} from 'web/services/vendors-api';

const vendor: Vendor = {
  id: 'vendors-1-A',
  name: 'Acme Parts',
  quickbooksId: 'qb-1',
  description: 'Refrigeration parts',
  contact: 'Pat',
  phone: '555-0100',
  notes: 'Net 30',
};

function respond(data: Vendor): VendorResponse {
  return {
    data,
    metadata: { traceId: 'test', timestamp: new Date().toISOString() },
  };
}

class FakeVendorsApi extends Service {
  static updates: { id: string; vendor: VendorUpdate }[] = [];
  static failure: ApiError | null = null;

  list(query: ListVendorsQuery): Promise<VendorPage> {
    return Promise.resolve({
      data: [vendor],
      pagination: {
        page: query.page ?? 1,
        pageSize: 25,
        totalCount: 1,
        totalPages: 1,
      },
      metadata: { traceId: 'test', timestamp: new Date().toISOString() },
    });
  }

  get(id: string): Promise<VendorResponse> {
    return Promise.resolve(respond({ ...vendor, id }));
  }

  update(id: string, update: VendorUpdate): Promise<VendorResponse> {
    FakeVendorsApi.updates.push({ id, vendor: update });
    if (FakeVendorsApi.failure !== null) {
      return Promise.reject(FakeVendorsApi.failure);
    }

    return Promise.resolve(
      respond({
        id,
        name: update.name.trim(),
        quickbooksId: update.quickbooksId ?? null,
        description: update.description ?? null,
        contact: update.contact ?? null,
        phone: update.phone ?? null,
        notes: update.notes ?? null,
      }),
    );
  }
}

module('Acceptance | vendor details', function (hooks) {
  setupApplicationTest(hooks);

  const realConfirm = window.confirm.bind(window);

  hooks.beforeEach(function () {
    FakeVendorsApi.updates = [];
    FakeVendorsApi.failure = null;
    this.owner.register('service:vendors-api', FakeVendorsApi);
  });

  hooks.afterEach(function () {
    window.confirm = realConfirm;
  });

  test('it shows every field and not the vendor id', async function (assert) {
    await visit('/vendors/vendors-1-A');

    assert.dom('#vendor-name').hasValue('Acme Parts');
    assert.dom('#vendor-description').hasValue('Refrigeration parts');
    assert.dom('#vendor-contact').hasValue('Pat');
    assert.dom('#vendor-phone').hasValue('555-0100');
    assert.dom('#vendor-quickbooks-id').hasValue('qb-1');
    assert.dom('#vendor-notes').hasValue('Net 30');
    assert.dom('main').doesNotContainText('vendors-1-A');
  });

  test('Save starts disabled and Back to List is available', async function (assert) {
    await visit('/vendors/vendors-1-A');

    assert.dom('button[type="submit"]').hasText('Save').isDisabled();
    assert.dom('.form-actions button[type="button"]').hasText('Back to List');
  });

  test('editing any field enables Save, and restoring the value disables it again', async function (assert) {
    await visit('/vendors/vendors-1-A');

    const edits: [string, string, string][] = [
      ['#vendor-name', 'Acme Supply', 'Acme Parts'],
      ['#vendor-description', 'Other', 'Refrigeration parts'],
      ['#vendor-contact', 'Sam', 'Pat'],
      ['#vendor-phone', '555-0199', '555-0100'],
      ['#vendor-quickbooks-id', 'qb-2', 'qb-1'],
      ['#vendor-notes', 'Net 60', 'Net 30'],
    ];

    for (const [selector, changed, original] of edits) {
      await fillIn(selector, changed);
      assert.dom('button[type="submit"]').isNotDisabled(`${selector} dirty`);

      await fillIn(selector, original);
      assert.dom('button[type="submit"]').isDisabled(`${selector} clean`);
    }
  });

  test('Save sends every field to the update API and then disables itself', async function (assert) {
    await visit('/vendors/vendors-1-A');
    await fillIn('#vendor-name', 'Acme Supply');
    await fillIn('#vendor-notes', '');

    await click('button[type="submit"]');

    assert.deepEqual(FakeVendorsApi.updates, [
      {
        id: 'vendors-1-A',
        vendor: {
          name: 'Acme Supply',
          description: 'Refrigeration parts',
          contact: 'Pat',
          phone: '555-0100',
          notes: '',
          quickbooksId: 'qb-1',
        },
      },
    ]);
    assert.dom('button[type="submit"]').isDisabled();
    assert.dom('#vendor-name').hasValue('Acme Supply');
    assert.dom('dialog').doesNotExist();
  });

  test('a failed Save shows the error message on screen and leaves Save enabled', async function (assert) {
    FakeVendorsApi.failure = new ApiError({
      status: 400,
      title: 'Validation Failed',
      detail: 'Vendor name is required.',
    });
    await visit('/vendors/vendors-1-A');
    await fillIn('#vendor-name', '');

    await click('button[type="submit"]');

    assert.dom('dialog[open]').exists();
    assert.dom('dialog').containsText('Vendor name is required.');
    assert.dom('#vendor-name').hasValue('');

    await click('dialog button');

    assert.dom('dialog').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('Back to List returns to the list without a prompt when nothing changed', async function (assert) {
    let asked = false;
    window.confirm = () => {
      asked = true;
      return true;
    };
    await visit('/vendors/vendors-1-A');

    await click('.form-actions button[type="button"]');

    assert.false(asked);
    assert.strictEqual(currentURL(), '/vendors');
  });

  test('Back to List with unsaved changes stays when the user declines', async function (assert) {
    window.confirm = () => false;
    await visit('/vendors/vendors-1-A');
    await fillIn('#vendor-name', 'Acme Supply');

    await click('.form-actions button[type="button"]');

    assert.strictEqual(currentURL(), '/vendors/vendors-1-A');
    assert.dom('#vendor-name').hasValue('Acme Supply');
  });

  test('Back to List with unsaved changes leaves when the user confirms', async function (assert) {
    window.confirm = () => true;
    await visit('/vendors/vendors-1-A');
    await fillIn('#vendor-name', 'Acme Supply');

    await click('.form-actions button[type="button"]');

    assert.strictEqual(currentURL(), '/vendors');
  });
});
