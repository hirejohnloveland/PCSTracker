import Service from '@ember/service';
import { click, currentURL, fillIn, visit } from '@ember/test-helpers';
import { module, test } from 'qunit';
import { setupApplicationTest } from 'web/tests/helpers';
import { ApiError } from 'web/services/api-client';
import { PHONE_MESSAGE, ROLE_MESSAGE } from 'web/utils/technician-validation';
import type {
  ListTechniciansQuery,
  Technician,
  TechnicianPage,
  TechnicianResponse,
  TechnicianRolesResponse,
  TechnicianUpdate,
} from 'web/services/technicians-api';

const ROLES = ['Service Tech', 'Installer', 'Other'];

const technician: Technician = {
  id: 'technicians-1-A',
  fullName: 'Bob Osborne',
  role: 'Service Tech',
  active: true,
  quickbooksId: 'qb-1',
  phone: '214-309-2915',
  notes: 'Nights',
};

function meta() {
  return { traceId: 'test', timestamp: new Date().toISOString() };
}

class FakeTechniciansApi extends Service {
  static updates: { id: string; technician: TechnicianUpdate }[] = [];
  static failure: ApiError | null = null;
  static current: Technician = technician;

  list(query: ListTechniciansQuery): Promise<TechnicianPage> {
    return Promise.resolve({
      data: [FakeTechniciansApi.current],
      pagination: {
        page: query.page ?? 1,
        pageSize: 25,
        totalCount: 1,
        totalPages: 1,
      },
      metadata: meta(),
    });
  }

  get(id: string): Promise<TechnicianResponse> {
    return Promise.resolve({
      data: { ...FakeTechniciansApi.current, id },
      metadata: meta(),
    });
  }

  roles(): Promise<TechnicianRolesResponse> {
    return Promise.resolve({ data: { roles: ROLES }, metadata: meta() });
  }

  update(id: string, update: TechnicianUpdate): Promise<TechnicianResponse> {
    FakeTechniciansApi.updates.push({ id, technician: update });
    if (FakeTechniciansApi.failure !== null) {
      return Promise.reject(FakeTechniciansApi.failure);
    }

    return Promise.resolve({
      data: {
        id,
        fullName: update.fullName.trim(),
        role: update.role,
        active: update.active ?? true,
        quickbooksId: update.quickbooksId ?? null,
        phone: update.phone ?? null,
        notes: update.notes ?? null,
      },
      metadata: meta(),
    });
  }
}

module('Acceptance | technician details', function (hooks) {
  setupApplicationTest(hooks);

  const realConfirm = window.confirm.bind(window);

  hooks.beforeEach(function () {
    FakeTechniciansApi.updates = [];
    FakeTechniciansApi.failure = null;
    FakeTechniciansApi.current = technician;
    this.owner.register('service:technicians-api', FakeTechniciansApi);
  });

  hooks.afterEach(function () {
    window.confirm = realConfirm;
  });

  test('it shows every field, the configured roles, and not the technician id', async function (assert) {
    await visit('/technicians/technicians-1-A');

    assert.dom('#technician-full-name').hasValue('Bob Osborne');
    assert.dom('#technician-role').hasValue('Service Tech');
    assert.dom('#technician-active').isChecked();
    assert.dom('#technician-phone').hasValue('214-309-2915');
    assert.dom('#technician-quickbooks-id').hasValue('qb-1');
    assert.dom('#technician-notes').hasValue('Nights');
    assert
      .dom('#technician-role option')
      .exists({ count: ROLES.length + 1 }, 'the roles and a placeholder');
    assert.dom('main').doesNotContainText('technicians-1-A');
  });

  test('Save starts disabled and Back to List is available', async function (assert) {
    await visit('/technicians/technicians-1-A');

    assert.dom('button[type="submit"]').hasText('Save').isDisabled();
    assert.dom('.form-actions button[type="button"]').hasText('Back to List');
  });

  test('editing any field enables Save, and restoring the value disables it again', async function (assert) {
    await visit('/technicians/technicians-1-A');

    const edits: [string, string, string][] = [
      ['#technician-full-name', 'Robert Osborne', 'Bob Osborne'],
      ['#technician-role', 'Installer', 'Service Tech'],
      ['#technician-phone', '214-309-2916', '214-309-2915'],
      ['#technician-quickbooks-id', 'qb-2', 'qb-1'],
      ['#technician-notes', 'Days', 'Nights'],
    ];

    for (const [selector, changed, original] of edits) {
      await fillIn(selector, changed);
      assert.dom('button[type="submit"]').isNotDisabled(`${selector} dirty`);

      await fillIn(selector, original);
      assert.dom('button[type="submit"]').isDisabled(`${selector} clean`);
    }
  });

  test('toggling Active enables Save, and toggling it back disables it again', async function (assert) {
    await visit('/technicians/technicians-1-A');

    await click('#technician-active');
    assert.dom('#technician-active').isNotChecked();
    assert.dom('button[type="submit"]').isNotDisabled();

    await click('#technician-active');
    assert.dom('button[type="submit"]').isDisabled();
  });

  test('Save sends every field to the update API, shows a success toast and returns to the list', async function (assert) {
    await visit('/technicians/technicians-1-A');
    await fillIn('#technician-full-name', 'Robert Osborne');
    await fillIn('#technician-role', 'Installer');
    await click('#technician-active');
    await fillIn('#technician-notes', '');

    await click('button[type="submit"]');

    assert.deepEqual(FakeTechniciansApi.updates, [
      {
        id: 'technicians-1-A',
        technician: {
          fullName: 'Robert Osborne',
          role: 'Installer',
          active: false,
          phone: '214-309-2915',
          notes: '',
          quickbooksId: 'qb-1',
        },
      },
    ]);
    assert.strictEqual(currentURL(), '/technicians');
    assert.dom('.toast.success').hasText(/Technician saved\./);
    assert.dom('dialog').doesNotExist();
  });

  test('a failed Save shows the error message on screen and leaves Save enabled', async function (assert) {
    FakeTechniciansApi.failure = new ApiError({
      status: 400,
      title: 'Validation Failed',
      detail: 'The technician could not be saved.',
    });
    await visit('/technicians/technicians-1-A');
    await fillIn('#technician-full-name', 'Robert Osborne');

    await click('button[type="submit"]');

    assert
      .dom('dialog[open]')
      .containsText('The technician could not be saved.');
    assert.dom('#technician-full-name').hasValue('Robert Osborne');

    await click('dialog button');

    assert.dom('dialog').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('a technician whose role is no longer configured has no role selected, and Save stays disabled until one is chosen', async function (assert) {
    FakeTechniciansApi.current = { ...technician, role: 'Plumber' };

    await visit('/technicians/technicians-1-A');

    assert.dom('#technician-role').hasValue('');
    assert.dom('#technician-role-error').hasText(ROLE_MESSAGE);
    assert.dom('button[type="submit"]').isDisabled();

    await fillIn('#technician-role', 'Other');

    assert.dom('#technician-role-error').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('choosing the placeholder role shows that a role is required', async function (assert) {
    await visit('/technicians/technicians-1-A');

    await fillIn('#technician-role', '');

    assert.dom('#technician-role-error').hasText('Role is required.');
    assert.dom('#technician-role').hasAttribute('aria-invalid', 'true');
    assert.dom('button[type="submit"]').isDisabled();
  });

  test('an invalid phone shows its message and keeps Save disabled until it is fixed', async function (assert) {
    await visit('/technicians/technicians-1-A');
    assert.dom('.field-error').doesNotExist();

    await fillIn('#technician-phone', 'call me');

    assert.dom('#technician-phone-error').hasText(PHONE_MESSAGE);
    assert.dom('#technician-phone').hasAttribute('aria-invalid', 'true');
    assert.dom('button[type="submit"]').isDisabled();

    await fillIn('#technician-phone', '(214) 309-2916');

    assert.dom('#technician-phone-error').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('a blank full name shows a message and keeps Save disabled even when another field is changed', async function (assert) {
    await visit('/technicians/technicians-1-A');
    await fillIn('#technician-full-name', '');
    await fillIn('#technician-notes', 'Days');

    assert.dom('#technician-full-name-error').hasText('Full name is required.');
    assert.dom('button[type="submit"]').isDisabled();

    await fillIn('#technician-full-name', 'Bob Osborne');

    assert.dom('#technician-full-name-error').doesNotExist();
    assert.dom('button[type="submit"]').isNotDisabled();
  });

  test('a field over its length limit shows a message and disables Save', async function (assert) {
    await visit('/technicians/technicians-1-A');

    await fillIn('#technician-quickbooks-id', '9'.repeat(51));

    assert
      .dom('#technician-quickbooks-id-error')
      .hasText('QuickBooks Id must be 50 characters or fewer.');
    assert.dom('button[type="submit"]').isDisabled();
  });

  test('Back to List returns to the list without a prompt when nothing changed', async function (assert) {
    let asked = false;
    window.confirm = () => {
      asked = true;
      return true;
    };
    await visit('/technicians/technicians-1-A');

    await click('.form-actions button[type="button"]');

    assert.false(asked);
    assert.strictEqual(currentURL(), '/technicians');
  });

  test('Back to List with unsaved changes stays when the user declines and leaves when the user confirms', async function (assert) {
    window.confirm = () => false;
    await visit('/technicians/technicians-1-A');
    await fillIn('#technician-full-name', 'Robert Osborne');

    await click('.form-actions button[type="button"]');

    assert.strictEqual(currentURL(), '/technicians/technicians-1-A');
    assert.dom('#technician-full-name').hasValue('Robert Osborne');

    window.confirm = () => true;
    await click('.form-actions button[type="button"]');

    assert.strictEqual(currentURL(), '/technicians');
  });
});
