import Service from '@ember/service';
import { click, currentURL, fillIn, visit } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import { PHONE_MESSAGE } from 'web/utils/customer-validation';
import type {
  Customer,
  CustomerPage,
  CustomerResponse,
  CustomerUpdate,
  ListCustomersQuery,
} from 'web/services/customers-api';

const customer: Customer = {
  id: 'customers-1-A',
  name: 'Acme Parts',
  quickbooksId: 'qb-1',
  description: 'Refrigeration parts',
  contact: 'Pat',
  phone: '214-309-2915',
  notes: 'Net 30',
};

function respond(data: Customer): CustomerResponse {
  return {
    data,
    metadata: { traceId: 'test', timestamp: new Date().toISOString() },
  };
}

class FakeCustomersApi extends Service {
  static updates: { id: string; customer: CustomerUpdate }[] = [];
  static failure: ApiError | null = null;

  list(query: ListCustomersQuery): Promise<CustomerPage> {
    return Promise.resolve({
      data: [customer],
      pagination: {
        page: query.page ?? 1,
        pageSize: 25,
        totalCount: 1,
        totalPages: 1,
      },
      metadata: { traceId: 'test', timestamp: new Date().toISOString() },
    });
  }

  get(id: string): Promise<CustomerResponse> {
    return Promise.resolve(respond({ ...customer, id }));
  }

  update(id: string, update: CustomerUpdate): Promise<CustomerResponse> {
    FakeCustomersApi.updates.push({ id, customer: update });
    if (FakeCustomersApi.failure !== null) {
      return Promise.reject(FakeCustomersApi.failure);
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

module('Acceptance | customer details', function (hooks) {
  setupApplicationTest(hooks);

  const realConfirm = window.confirm.bind(window);

  hooks.beforeEach(function () {
    FakeCustomersApi.updates = [];
    FakeCustomersApi.failure = null;
    this.owner.register('service:customers-api', FakeCustomersApi);
  });

  hooks.afterEach(function () {
    window.confirm = realConfirm;
  });

  test('it shows every field and not the customer id', async function (assert) {
    await visit('/customers/customers-1-A');

    assert.dom('#customer-name').hasValue('Acme Parts');
    assert.dom('#customer-description').hasValue('Refrigeration parts');
    assert.dom('#customer-contact').hasValue('Pat');
    assert.dom('#customer-phone').hasValue('214-309-2915');
    assert.dom('#customer-quickbooks-id').hasValue('qb-1');
    assert.dom('#customer-notes').hasValue('Net 30');
    assert.dom('main').doesNotContainText('customers-1-A');
  });

  test('Save starts disabled and Back to List is available', async function (assert) {
    await visit('/customers/customers-1-A');

    assert.dom('button[type="submit"]').hasText('Save').isDisabled();
    assert.dom('.form-actions button[type="button"]').hasText('Back to List');
  });

  test('editing any field enables Save, and restoring the value disables it again', async function (assert) {
    await visit('/customers/customers-1-A');

    const edits: [string, string, string][] = [
      ['#customer-name', 'Acme Supply', 'Acme Parts'],
      ['#customer-description', 'Other', 'Refrigeration parts'],
      ['#customer-contact', 'Sam', 'Pat'],
      ['#customer-phone', '214-309-2916', '214-309-2915'],
      ['#customer-quickbooks-id', 'qb-2', 'qb-1'],
      ['#customer-notes', 'Net 60', 'Net 30'],
    ];

    for (const [selector, changed, original] of edits) {
      await fillIn(selector, changed);
      assert.dom('button[type="submit"]').isNotDisabled(`${selector} dirty`);

      await fillIn(selector, original);
      assert.dom('button[type="submit"]').isDisabled(`${selector} clean`);
    }
  });

  test('Save sends every field to the update API, shows a success toast and returns to the list', async function (assert) {
    await visit('/customers/customers-1-A');
    await fillIn('#customer-name', 'Acme Supply');
    await fillIn('#customer-notes', '');

    await click('button[type="submit"]');

    assert.deepEqual(FakeCustomersApi.updates, [
      {
        id: 'customers-1-A',
        customer: {
          name: 'Acme Supply',
          description: 'Refrigeration parts',
          contact: 'Pat',
          phone: '214-309-2915',
          notes: '',
          quickbooksId: 'qb-1',
        },
      },
    ]);
    assert.strictEqual(currentURL(), '/customers');
    assert.dom('.toast.success').hasText(/Customer saved\./);
    assert.dom('dialog').doesNotExist();
  });

  test('a failed Save shows the error message on screen and leaves Save enabled', async function (assert) {
    FakeCustomersApi.failure = new ApiError({
      status: 400,
      title: 'Validation Failed',
      detail: 'The customer could not be saved.',
    });
    await visit('/customers/customers-1-A');
    await fillIn('#customer-name', 'Acme Supply');

    await click('button[type="submit"]');

    assert.dom('dialog[open]').exists();
    assert.dom('dialog').containsText('The customer could not be saved.');
    assert.dom('#customer-name').hasValue('Acme Supply');

    await click('dialog button');

    assert.dom('dialog').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('an invalid phone shows its message and keeps Save disabled until it is fixed', async function (assert) {
    await visit('/customers/customers-1-A');
    assert.dom('.field-error').doesNotExist();

    await fillIn('#customer-phone', 'call me');

    assert.dom('#customer-phone-error').hasText(PHONE_MESSAGE);
    assert.dom('#customer-phone').hasAttribute('aria-invalid', 'true');
    assert.dom('button[type="submit"]').isDisabled();

    await fillIn('#customer-phone', '(214) 309-2916');

    assert.dom('#customer-phone-error').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('an invalid field keeps Save disabled even when another field is valid and changed', async function (assert) {
    await visit('/customers/customers-1-A');
    await fillIn('#customer-name', '');
    await fillIn('#customer-contact', 'Sam');

    assert.dom('#customer-name-error').hasText('Customer name is required.');
    assert.dom('button[type="submit"]').isDisabled();

    await fillIn('#customer-name', 'Acme Parts');

    assert.dom('#customer-name-error').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('a field over its length limit shows a message and disables Save', async function (assert) {
    await visit('/customers/customers-1-A');

    await fillIn('#customer-quickbooks-id', '9'.repeat(51));

    assert
      .dom('#customer-quickbooks-id-error')
      .hasText('QuickBooks Id must be 50 characters or fewer.');
    assert.dom('button[type="submit"]').isDisabled();
  });

  test('a blank phone is valid', async function (assert) {
    await visit('/customers/customers-1-A');

    await fillIn('#customer-phone', '');

    assert.dom('#customer-phone-error').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('Back to List returns to the list without a prompt when nothing changed', async function (assert) {
    let asked = false;
    window.confirm = () => {
      asked = true;
      return true;
    };
    await visit('/customers/customers-1-A');

    await click('.form-actions button[type="button"]');

    assert.false(asked);
    assert.strictEqual(currentURL(), '/customers');
  });

  test('Back to List with unsaved changes stays when the user declines', async function (assert) {
    window.confirm = () => false;
    await visit('/customers/customers-1-A');
    await fillIn('#customer-name', 'Acme Supply');

    await click('.form-actions button[type="button"]');

    assert.strictEqual(currentURL(), '/customers/customers-1-A');
    assert.dom('#customer-name').hasValue('Acme Supply');
  });

  test('Back to List with unsaved changes leaves when the user confirms', async function (assert) {
    window.confirm = () => true;
    await visit('/customers/customers-1-A');
    await fillIn('#customer-name', 'Acme Supply');

    await click('.form-actions button[type="button"]');

    assert.strictEqual(currentURL(), '/customers');
  });
});
