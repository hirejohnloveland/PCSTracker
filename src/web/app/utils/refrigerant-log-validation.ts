export type RefrigerantLogField =
  | 'date'
  | 'technician'
  | 'refrigerantType'
  | 'amountLbs'
  | 'notes'
  | 'ticketNumber'
  | 'jobName'
  | 'vendorInvoiceRef';

export interface RefrigerantLogChoices {
  technicians: readonly string[];
  refrigerants: readonly string[];
}

// These match the limits in RefrigerantLogEntryDtoValidator in the API.
const MAX_LENGTHS: Partial<Record<RefrigerantLogField, number>> = {
  ticketNumber: 50,
  jobName: 200,
  vendorInvoiceRef: 100,
  notes: 4000,
};

const LABELS: Partial<Record<RefrigerantLogField, string>> = {
  ticketNumber: 'Ticket number',
  jobName: 'Job name',
  vendorInvoiceRef: 'Vendor invoice ref',
  notes: 'Notes',
};

export const TECHNICIAN_MESSAGE =
  'Technician must be the full name of an existing technician.';
export const REFRIGERANT_TYPE_MESSAGE =
  'Refrigerant type must be one of the configured refrigerant types.';
export const CUSTOMER_MESSAGE =
  'Customer must be the name of an existing customer.';

export function validateRefrigerantLogField(
  field: RefrigerantLogField,
  value: string,
  choices: RefrigerantLogChoices,
): string | null {
  const text = value.trim();

  switch (field) {
    case 'date':
      return text === '' ? 'Date is required.' : null;
    case 'technician':
      if (text === '') {
        return 'Technician is required.';
      }
      return choices.technicians.includes(text) ? null : TECHNICIAN_MESSAGE;
    case 'refrigerantType':
      if (text === '') {
        return 'Refrigerant type is required.';
      }
      return choices.refrigerants.includes(text)
        ? null
        : REFRIGERANT_TYPE_MESSAGE;
    case 'amountLbs': {
      if (text === '') {
        return 'Amount used is required.';
      }
      const amount = Number(text);
      return Number.isFinite(amount) && amount > 0
        ? null
        : 'Amount used must be greater than zero.';
    }
    case 'notes':
      if (text === '') {
        return 'Notes are required.';
      }
      break;
    default:
      break;
  }

  const max = MAX_LENGTHS[field];
  if (max !== undefined && value.length > max) {
    return `${LABELS[field]} must be ${max} characters or fewer.`;
  }

  return null;
}
