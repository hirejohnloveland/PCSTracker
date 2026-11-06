import { module, test } from 'qunit';
import {
  REFRIGERANT_TYPE_MESSAGE,
  TECHNICIAN_MESSAGE,
  validateRefrigerantLogField,
} from 'web/utils/refrigerant-log-validation';

const CHOICES = {
  technicians: ['Bob Osborne', 'Aldo Cipriani'],
  refrigerants: ['R-22', 'R-404A'],
};

module('Unit | Utility | refrigerant-log-validation', function () {
  test('a blank date is required', function (assert) {
    assert.strictEqual(
      validateRefrigerantLogField('date', '', CHOICES),
      'Date is required.',
    );
    assert.strictEqual(
      validateRefrigerantLogField('date', '2026-05-15', CHOICES),
      null,
    );
  });

  test('a technician is required and must be one of the choices', function (assert) {
    assert.strictEqual(
      validateRefrigerantLogField('technician', '  ', CHOICES),
      'Technician is required.',
    );
    assert.strictEqual(
      validateRefrigerantLogField('technician', 'Nobody', CHOICES),
      TECHNICIAN_MESSAGE,
    );
    assert.strictEqual(
      validateRefrigerantLogField('technician', 'Bob Osborne', CHOICES),
      null,
    );
  });

  test('a refrigerant type is required and must be one of the choices', function (assert) {
    assert.strictEqual(
      validateRefrigerantLogField('refrigerantType', '', CHOICES),
      'Refrigerant type is required.',
    );
    assert.strictEqual(
      validateRefrigerantLogField('refrigerantType', 'R-9999', CHOICES),
      REFRIGERANT_TYPE_MESSAGE,
    );
    assert.strictEqual(
      validateRefrigerantLogField('refrigerantType', 'R-22', CHOICES),
      null,
    );
  });

  test('the amount is required and must be greater than zero', function (assert) {
    assert.strictEqual(
      validateRefrigerantLogField('amountLbs', '', CHOICES),
      'Amount used is required.',
    );
    for (const value of ['0', '-1', 'abc']) {
      assert.strictEqual(
        validateRefrigerantLogField('amountLbs', value, CHOICES),
        'Amount used must be greater than zero.',
        value,
      );
    }
    assert.strictEqual(
      validateRefrigerantLogField('amountLbs', '0.5', CHOICES),
      null,
    );
  });

  test('notes are required and limited to 4000 characters', function (assert) {
    assert.strictEqual(
      validateRefrigerantLogField('notes', '   ', CHOICES),
      'Notes are required.',
    );
    assert.strictEqual(
      validateRefrigerantLogField('notes', 'n'.repeat(4000), CHOICES),
      null,
    );
    assert.strictEqual(
      validateRefrigerantLogField('notes', 'n'.repeat(4001), CHOICES),
      'Notes must be 4000 characters or fewer.',
    );
  });

  test('the optional fields may be blank and have length limits', function (assert) {
    const limits: [
      'ticketNumber' | 'jobName' | 'vendorInvoiceRef',
      number,
      string,
    ][] = [
      ['ticketNumber', 50, 'Ticket number'],
      ['jobName', 200, 'Job name'],
      ['vendorInvoiceRef', 100, 'Vendor invoice ref'],
    ];

    for (const [field, max, label] of limits) {
      assert.strictEqual(validateRefrigerantLogField(field, '', CHOICES), null);
      assert.strictEqual(
        validateRefrigerantLogField(field, 'x'.repeat(max), CHOICES),
        null,
      );
      assert.strictEqual(
        validateRefrigerantLogField(field, 'x'.repeat(max + 1), CHOICES),
        `${label} must be ${max} characters or fewer.`,
      );
    }
  });
});
