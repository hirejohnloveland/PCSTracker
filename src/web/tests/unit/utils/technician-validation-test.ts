import { module, test } from 'qunit';
import {
  PHONE_MESSAGE,
  ROLE_MESSAGE,
  validateTechnicianField,
} from 'web/utils/technician-validation';

const ROLES = ['Service Tech', 'Installer', 'Other'];

module('Unit | Utility | technician-validation', function () {
  test('a blank full name is required, and a name over 200 characters is too long', function (assert) {
    assert.strictEqual(
      validateTechnicianField('fullName', '   ', ROLES),
      'Full name is required.',
    );
    assert.strictEqual(
      validateTechnicianField('fullName', 'a'.repeat(200), ROLES),
      null,
    );
    assert.strictEqual(
      validateTechnicianField('fullName', 'a'.repeat(201), ROLES),
      'Full name must be 200 characters or fewer.',
    );
  });

  test('a blank role is required', function (assert) {
    assert.strictEqual(
      validateTechnicianField('role', '', ROLES),
      'Role is required.',
    );
    assert.strictEqual(
      validateTechnicianField('role', '   ', ROLES),
      'Role is required.',
    );
  });

  test('a role must be one of the configured roles', function (assert) {
    for (const role of ROLES) {
      assert.strictEqual(validateTechnicianField('role', role, ROLES), null);
    }

    assert.strictEqual(
      validateTechnicianField('role', 'Plumber', ROLES),
      ROLE_MESSAGE,
    );
    assert.strictEqual(
      validateTechnicianField('role', 'service tech', ROLES),
      ROLE_MESSAGE,
    );
    assert.strictEqual(
      validateTechnicianField('role', 'Service Tech', []),
      ROLE_MESSAGE,
    );
  });

  test('the optional fields allow blank values and enforce their length limits', function (assert) {
    const limits = { notes: 4000, quickbooksId: 50 } as const;

    for (const [field, limit] of Object.entries(limits)) {
      const key = field as keyof typeof limits;
      assert.strictEqual(
        validateTechnicianField(key, '', ROLES),
        null,
        `${key} blank`,
      );
      assert.strictEqual(
        validateTechnicianField(key, '9'.repeat(limit), ROLES),
        null,
        `${key} at the limit`,
      );
      assert.notStrictEqual(
        validateTechnicianField(key, '9'.repeat(limit + 1), ROLES),
        null,
        `${key} over the limit`,
      );
    }
  });

  test('a phone over 50 characters is too long', function (assert) {
    assert.strictEqual(
      validateTechnicianField('phone', '2'.repeat(51), ROLES),
      'Phone must be 50 characters or fewer.',
    );
  });

  test('valid US and Canadian phone numbers are accepted', function (assert) {
    const phones = [
      '',
      '   ',
      '2143092915',
      '(214) 309-2915',
      '214.309.2915',
      '1-214-309-2915',
      '+1 214 309 2915',
      '214-309-2915 x123',
      '214-309-2915 ext. 123',
      '+1 416 392 2489',
    ];

    for (const phone of phones) {
      assert.strictEqual(
        validateTechnicianField('phone', phone, ROLES),
        null,
        phone,
      );
    }
  });

  test('anything that is not a valid US or Canadian number is rejected', function (assert) {
    const phones = [
      'call me',
      '1-800-FLOWERS',
      '214-309-2915 x',
      '-2143092915',
      '214-309-2915; DROP',
      '1-214-309-29156314-1',
      '5550100',
      '309-2915',
      '123-456-7890',
      '+44 20 7946 0958',
    ];

    for (const phone of phones) {
      assert.strictEqual(
        validateTechnicianField('phone', phone, ROLES),
        PHONE_MESSAGE,
        phone,
      );
    }
  });
});
