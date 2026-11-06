import { module, test } from 'qunit';
import {
  PHONE_MESSAGE,
  validateCustomerField,
} from 'web/utils/customer-validation';

module('Unit | Utility | customer-validation', function () {
  test('a blank name is required, and a name over 200 characters is too long', function (assert) {
    assert.strictEqual(
      validateCustomerField('name', '   '),
      'Customer name is required.',
    );
    assert.strictEqual(validateCustomerField('name', 'a'.repeat(200)), null);
    assert.strictEqual(
      validateCustomerField('name', 'a'.repeat(201)),
      'Customer name must be 200 characters or fewer.',
    );
  });

  test('the optional fields allow blank values and enforce their length limits', function (assert) {
    const limits = {
      description: 500,
      contact: 200,
      notes: 4000,
      quickbooksId: 50,
    } as const;

    for (const [field, limit] of Object.entries(limits)) {
      const key = field as keyof typeof limits;
      assert.strictEqual(validateCustomerField(key, ''), null, `${key} blank`);
      assert.strictEqual(
        validateCustomerField(key, '9'.repeat(limit)),
        null,
        `${key} at the limit`,
      );
      assert.notStrictEqual(
        validateCustomerField(key, '9'.repeat(limit + 1)),
        null,
        `${key} over the limit`,
      );
    }
  });

  test('a phone over 50 characters is too long', function (assert) {
    assert.strictEqual(
      validateCustomerField('phone', '2'.repeat(51)),
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
      assert.strictEqual(validateCustomerField('phone', phone), null, phone);
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
        validateCustomerField('phone', phone),
        PHONE_MESSAGE,
        phone,
      );
    }
  });
});
