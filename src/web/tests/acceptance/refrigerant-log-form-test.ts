import Service from '@ember/service';
import { click, currentURL, fillIn, visit } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import { CUSTOMER_MESSAGE } from 'web/utils/refrigerant-log-validation';
import type {
  CustomerPage,
  ListCustomersQuery,
} from 'web/services/customers-api';
import type {
  RefrigerantLogEntry,
  RefrigerantLogEntryInput,
  RefrigerantLogEntryResponse,
  RefrigerantLogPage,
  RefrigerantTypesResponse,
} from 'web/services/refrigerant-log-api';
import type { TechnicianPage } from 'web/services/technicians-api';

const TECHNICIANS = ['Aldo Cipriani', 'Bob Osborne'];
const REFRIGERANTS = ['R-22', 'R-404A'];
const CUSTOMERS = ['Metro Food Market - Bronx', 'Atlantic Cold Storage'];

const entry: RefrigerantLogEntry = {
  id: 'refrigerantlogentries-1-A',
  date: '2026-05-15',
  technician: 'Aldo Cipriani',
  refrigerantType: 'R-404A',
  amountLbs: 24.5,
  notes: 'Leak repair',
  ticketNumber: 'EM-0415',
  jobName: 'ACS-BK-01',
  customer: 'Atlantic Cold Storage',
  vendorInvoiceRef: 'URI-88430',
};

function meta() {
  return { traceId: 'test', timestamp: new Date().toISOString() };
}

class FakeRefrigerantLogApi extends Service {
  static created: RefrigerantLogEntryInput[] = [];
  static updates: { id: string; entry: RefrigerantLogEntryInput }[] = [];
  static failure: ApiError | null = null;
  static current: RefrigerantLogEntry = entry;

  list(): Promise<RefrigerantLogPage> {
    return Promise.resolve({
      data: [],
      pagination: { page: 1, pageSize: 25, totalCount: 0, totalPages: 0 },
      metadata: meta(),
    });
  }

  get(id: string): Promise<RefrigerantLogEntryResponse> {
    return Promise.resolve({
      data: { ...FakeRefrigerantLogApi.current, id },
      metadata: meta(),
    });
  }

  refrigerants(): Promise<RefrigerantTypesResponse> {
    return Promise.resolve({
      data: { refrigerants: REFRIGERANTS },
      metadata: meta(),
    });
  }

  create(
    input: RefrigerantLogEntryInput,
  ): Promise<RefrigerantLogEntryResponse> {
    FakeRefrigerantLogApi.created.push(input);
    return this.respond('refrigerantlogentries-9-A', input);
  }

  update(
    id: string,
    input: RefrigerantLogEntryInput,
  ): Promise<RefrigerantLogEntryResponse> {
    FakeRefrigerantLogApi.updates.push({ id, entry: input });
    return this.respond(id, input);
  }

  private respond(
    id: string,
    input: RefrigerantLogEntryInput,
  ): Promise<RefrigerantLogEntryResponse> {
    if (FakeRefrigerantLogApi.failure !== null) {
      return Promise.reject(FakeRefrigerantLogApi.failure);
    }

    return Promise.resolve({
      data: {
        id,
        date: input.date,
        technician: input.technician,
        refrigerantType: input.refrigerantType,
        amountLbs: input.amountLbs,
        notes: input.notes,
        ticketNumber: input.ticketNumber ?? null,
        jobName: input.jobName ?? null,
        customer: input.customer ?? null,
        vendorInvoiceRef: input.vendorInvoiceRef ?? null,
      },
      metadata: meta(),
    });
  }
}

class FakeTechniciansApi extends Service {
  list(): Promise<TechnicianPage> {
    return Promise.resolve({
      data: TECHNICIANS.map((fullName, index) => ({
        id: `technicians-${index + 1}-A`,
        fullName,
        role: 'Service Tech',
        active: true,
        quickbooksId: null,
        phone: null,
        notes: null,
      })),
      pagination: {
        page: 1,
        pageSize: 100,
        totalCount: TECHNICIANS.length,
        totalPages: 1,
      },
      metadata: meta(),
    });
  }
}

class FakeCustomersApi extends Service {
  static queries: ListCustomersQuery[] = [];

  list(query: ListCustomersQuery): Promise<CustomerPage> {
    FakeCustomersApi.queries.push(query);
    const text = (query.name ?? '').toLowerCase();
    const matches = CUSTOMERS.filter((name) =>
      name.toLowerCase().includes(text),
    );

    return Promise.resolve({
      data: matches.map((name, index) => ({
        id: `customers-${index + 1}-A`,
        name,
        quickbooksId: null,
        description: null,
        contact: null,
        phone: null,
        notes: null,
      })),
      pagination: {
        page: 1,
        pageSize: query.pageSize ?? 25,
        totalCount: matches.length,
        totalPages: 1,
      },
      metadata: meta(),
    });
  }
}

async function fillRequiredFields(): Promise<void> {
  await fillIn('#refrigerant-log-technician', 'Bob Osborne');
  await fillIn('#refrigerant-log-refrigerant-type', 'R-22');
  await fillIn('#refrigerant-log-amount', '6');
  await fillIn('#refrigerant-log-notes', 'Compressor replacement');
}

module('Acceptance | refrigerant log form', function (hooks) {
  setupApplicationTest(hooks);

  const realConfirm = window.confirm.bind(window);

  hooks.beforeEach(function () {
    FakeRefrigerantLogApi.created = [];
    FakeRefrigerantLogApi.updates = [];
    FakeRefrigerantLogApi.failure = null;
    FakeRefrigerantLogApi.current = entry;
    FakeCustomersApi.queries = [];
    this.owner.register('service:refrigerant-log-api', FakeRefrigerantLogApi);
    this.owner.register('service:technicians-api', FakeTechniciansApi);
    this.owner.register('service:customers-api', FakeCustomersApi);
  });

  hooks.afterEach(function () {
    window.confirm = realConfirm;
  });

  module('adding an entry', function () {
    test('the form opens with a date, the choices to pick from, Save disabled and no errors showing', async function (assert) {
      await visit('/refrigerant-log/new');

      assert.dom('#refrigerant-log-date').hasAnyValue();
      assert
        .dom('#refrigerant-log-technician option')
        .exists(
          { count: TECHNICIANS.length + 1 },
          'the technicians and a placeholder',
        );
      assert
        .dom('#refrigerant-log-refrigerant-type option')
        .exists(
          { count: REFRIGERANTS.length + 1 },
          'the types and a placeholder',
        );
      assert.dom('button[type="submit"]').hasText('Save').isDisabled();
      assert.dom('.field-error').doesNotExist();
    });

    test('a field shows its error once the user has changed it', async function (assert) {
      await visit('/refrigerant-log/new');

      await fillIn('#refrigerant-log-amount', '0');
      assert
        .dom('#refrigerant-log-amount-error')
        .hasText('Amount used must be greater than zero.');
      assert
        .dom('#refrigerant-log-amount')
        .hasAttribute('aria-invalid', 'true');

      await fillIn('#refrigerant-log-notes', '  ');
      assert.dom('#refrigerant-log-notes-error').hasText('Notes are required.');
      assert.dom('button[type="submit"]').isDisabled();
    });

    test('Save becomes available once the required fields are valid', async function (assert) {
      await visit('/refrigerant-log/new');

      await fillRequiredFields();

      assert.dom('.field-error').doesNotExist();
      assert.dom('button[type="submit"]').isNotDisabled();
    });

    test('Save sends every field to the create API, shows a success toast and returns to the list', async function (assert) {
      await visit('/refrigerant-log/new');
      await fillRequiredFields();
      await fillIn('#refrigerant-log-date', '2026-05-14');
      await fillIn('#refrigerant-log-ticket-number', 'EM-0412');
      await fillIn('#refrigerant-log-job-name', 'MFM-BX-01');
      await fillIn('#refrigerant-log-vendor-invoice-ref', 'PO-2026-0081');

      await click('button[type="submit"]');

      assert.deepEqual(FakeRefrigerantLogApi.created, [
        {
          date: '2026-05-14',
          technician: 'Bob Osborne',
          refrigerantType: 'R-22',
          amountLbs: 6,
          notes: 'Compressor replacement',
          ticketNumber: 'EM-0412',
          jobName: 'MFM-BX-01',
          customer: '',
          vendorInvoiceRef: 'PO-2026-0081',
        },
      ]);
      assert.strictEqual(currentURL(), '/refrigerant-log');
      assert.dom('.toast.success').hasText(/Refrigerant log entry saved\./);
    });

    test('typing a customer suggests matching customers', async function (assert) {
      await visit('/refrigerant-log/new');

      await fillIn('#refrigerant-log-customer', 'metro');

      assert.deepEqual(FakeCustomersApi.queries, [
        { name: 'metro', pageSize: 10 },
      ]);
      assert
        .dom('#refrigerant-log-customer-options option')
        .exists({ count: 1 });
      assert
        .dom('#refrigerant-log-customer-options option')
        .hasAttribute('value', 'Metro Food Market - Bronx');
    });

    test('a customer picked from the suggestions is saved', async function (assert) {
      await visit('/refrigerant-log/new');
      await fillRequiredFields();
      await fillIn('#refrigerant-log-customer', 'metro');
      await fillIn('#refrigerant-log-customer', 'Metro Food Market - Bronx');
      FakeCustomersApi.queries = [];

      await click('button[type="submit"]');

      assert.strictEqual(
        FakeRefrigerantLogApi.created[0]?.customer,
        'Metro Food Market - Bronx',
      );
      assert.deepEqual(FakeCustomersApi.queries, [], 'no further lookup');
      assert.strictEqual(currentURL(), '/refrigerant-log');
    });

    test('a customer that does not exist is not saved and shows a message', async function (assert) {
      await visit('/refrigerant-log/new');
      await fillRequiredFields();
      await fillIn('#refrigerant-log-customer', 'Nobody Inc');

      await click('button[type="submit"]');

      assert.dom('#refrigerant-log-customer-error').hasText(CUSTOMER_MESSAGE);
      assert.deepEqual(FakeRefrigerantLogApi.created, []);
      assert.strictEqual(currentURL(), '/refrigerant-log/new');

      await fillIn('#refrigerant-log-customer', 'Atlantic Cold Storage');

      assert.dom('#refrigerant-log-customer-error').doesNotExist();
    });

    test('a customer typed in full without picking a suggestion is looked up and saved', async function (assert) {
      await visit('/refrigerant-log/new');
      await fillRequiredFields();
      FakeCustomersApi.queries = [];
      // Typing and saving happen faster than the suggestions arrive.
      const input = document.querySelector<HTMLInputElement>(
        '#refrigerant-log-customer',
      ) as HTMLInputElement;
      input.value = 'Atlantic Cold Storage';
      input.dispatchEvent(new Event('input', { bubbles: true }));

      await click('button[type="submit"]');

      assert.strictEqual(
        FakeRefrigerantLogApi.created[0]?.customer,
        'Atlantic Cold Storage',
      );
    });

    test('a failed Save shows the error on screen, keeps the entries typed and leaves Save enabled', async function (assert) {
      FakeRefrigerantLogApi.failure = new ApiError({
        status: 400,
        title: 'Validation Failed',
        detail: 'The entry could not be saved.',
      });
      await visit('/refrigerant-log/new');
      await fillRequiredFields();

      await click('button[type="submit"]');

      assert.dom('dialog[open]').containsText('The entry could not be saved.');
      assert.dom('#refrigerant-log-notes').hasValue('Compressor replacement');

      await click('dialog button');

      assert.dom('dialog').doesNotExist();
      assert.dom('button[type="submit"]').isNotDisabled();
      assert.strictEqual(currentURL(), '/refrigerant-log/new');
    });

    test('Back to List leaves without a prompt when nothing changed, and asks first when something did', async function (assert) {
      let asked = 0;
      window.confirm = () => {
        asked++;
        return false;
      };
      await visit('/refrigerant-log/new');

      await click('.form-actions button[type="button"]');

      assert.strictEqual(asked, 0);
      assert.strictEqual(currentURL(), '/refrigerant-log');

      await visit('/refrigerant-log/new');
      await fillIn('#refrigerant-log-notes', 'Something');
      await click('.form-actions button[type="button"]');

      assert.strictEqual(asked, 1);
      assert.strictEqual(currentURL(), '/refrigerant-log/new');

      window.confirm = () => true;
      await click('.form-actions button[type="button"]');

      assert.strictEqual(currentURL(), '/refrigerant-log');
    });
  });

  module('editing an entry', function () {
    test('it shows every field and not the entry id', async function (assert) {
      await visit('/refrigerant-log/refrigerantlogentries-1-A');

      assert.dom('#refrigerant-log-date').hasValue('2026-05-15');
      assert.dom('#refrigerant-log-technician').hasValue('Aldo Cipriani');
      assert.dom('#refrigerant-log-refrigerant-type').hasValue('R-404A');
      assert.dom('#refrigerant-log-amount').hasValue('24.5');
      assert.dom('#refrigerant-log-ticket-number').hasValue('EM-0415');
      assert.dom('#refrigerant-log-job-name').hasValue('ACS-BK-01');
      assert.dom('#refrigerant-log-customer').hasValue('Atlantic Cold Storage');
      assert.dom('#refrigerant-log-vendor-invoice-ref').hasValue('URI-88430');
      assert.dom('#refrigerant-log-notes').hasValue('Leak repair');
      assert.dom('main').doesNotContainText('refrigerantlogentries');
      assert.dom('button[type="submit"]').isDisabled();
    });

    test('editing a field enables Save, and restoring it disables Save again', async function (assert) {
      await visit('/refrigerant-log/refrigerantlogentries-1-A');

      await fillIn('#refrigerant-log-notes', 'Changed');
      assert.dom('button[type="submit"]').isNotDisabled();

      await fillIn('#refrigerant-log-notes', 'Leak repair');
      assert.dom('button[type="submit"]').isDisabled();
    });

    test('Save sends every field to the update API for that entry and returns to the list', async function (assert) {
      await visit('/refrigerant-log/refrigerantlogentries-1-A');
      await fillIn('#refrigerant-log-amount', '30');
      await fillIn('#refrigerant-log-ticket-number', '');

      await click('button[type="submit"]');

      assert.deepEqual(FakeRefrigerantLogApi.updates, [
        {
          id: 'refrigerantlogentries-1-A',
          entry: {
            date: '2026-05-15',
            technician: 'Aldo Cipriani',
            refrigerantType: 'R-404A',
            amountLbs: 30,
            notes: 'Leak repair',
            ticketNumber: '',
            jobName: 'ACS-BK-01',
            customer: 'Atlantic Cold Storage',
            vendorInvoiceRef: 'URI-88430',
          },
        },
      ]);
      assert.strictEqual(currentURL(), '/refrigerant-log');
      assert.dom('.toast.success').hasText(/Refrigerant log entry saved\./);
    });

    test('an entry logged for a technician who is no longer active keeps that technician', async function (assert) {
      FakeRefrigerantLogApi.current = { ...entry, technician: 'Carl Gone' };

      await visit('/refrigerant-log/refrigerantlogentries-1-A');

      assert.dom('#refrigerant-log-technician').hasValue('Carl Gone');
      assert
        .dom('#refrigerant-log-technician option')
        .exists({ count: TECHNICIANS.length + 2 });
      assert.dom('.field-error').doesNotExist();

      await fillIn('#refrigerant-log-notes', 'Changed');

      assert.dom('button[type="submit"]').isNotDisabled();
    });

    test('an entry whose refrigerant type is no longer configured has none selected, and Save stays disabled until one is chosen', async function (assert) {
      FakeRefrigerantLogApi.current = { ...entry, refrigerantType: 'R-9999' };

      await visit('/refrigerant-log/refrigerantlogentries-1-A');

      assert.dom('#refrigerant-log-refrigerant-type').hasValue('');
      assert.dom('#refrigerant-log-refrigerant-type-error').exists();
      assert.dom('button[type="submit"]').isDisabled();

      await fillIn('#refrigerant-log-refrigerant-type', 'R-22');

      assert.dom('#refrigerant-log-refrigerant-type-error').doesNotExist();
      assert.dom('button[type="submit"]').isNotDisabled();
    });
  });
});
