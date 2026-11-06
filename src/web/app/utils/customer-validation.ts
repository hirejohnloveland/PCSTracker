import { parsePhoneNumberFromString } from 'libphonenumber-js/max';

export type CustomerTextField =
  'name' | 'description' | 'contact' | 'phone' | 'notes' | 'quickbooksId';

// These match the limits and the phone rule in CustomerDtoValidator in the API.
const MAX_LENGTHS: Record<CustomerTextField, number> = {
  name: 200,
  description: 500,
  contact: 200,
  phone: 50,
  notes: 4000,
  quickbooksId: 50,
};

const LABELS: Record<CustomerTextField, string> = {
  name: 'Customer name',
  description: 'Description',
  contact: 'Contact',
  phone: 'Phone',
  notes: 'Notes',
  quickbooksId: 'QuickBooks Id',
};

// libphonenumber also reads letters as a vanity number such as 1-800-FLOWERS, so
// the characters are checked first: digits, spaces and common punctuation, an
// optional leading + or (, and an optional extension (x123 or ext. 123).
const PHONE_CHARACTERS =
  /^\s*\+?\(?[0-9][0-9\s().-]*(?:\s*(?:x|ext\.?)\s*[0-9]+)?\s*$/i;

export const PHONE_MESSAGE =
  'Phone must be a valid US or Canadian number, with an optional extension such as x123.';

function isUsOrCanadianNumber(value: string): boolean {
  if (!PHONE_CHARACTERS.test(value)) {
    return false;
  }

  const number = parsePhoneNumberFromString(value, 'US');
  return (
    number !== undefined &&
    number.isValid() &&
    (number.country === 'US' || number.country === 'CA')
  );
}
export function validateCustomerField(
  field: CustomerTextField,
  value: string,
): string | null {
  if (field === 'name' && value.trim() === '') {
    return 'Customer name is required.';
  }

  if (value.length > MAX_LENGTHS[field]) {
    return `${LABELS[field]} must be ${MAX_LENGTHS[field]} characters or fewer.`;
  }

  if (
    field === 'phone' &&
    value.trim() !== '' &&
    !isUsOrCanadianNumber(value)
  ) {
    return PHONE_MESSAGE;
  }

  return null;
}
