import { on } from '@ember/modifier';
import { action } from '@ember/object';
import type RouterService from '@ember/routing/router-service';
import { service } from '@ember/service';
import Component from '@glimmer/component';
import { tracked } from '@glimmer/tracking';
import { cancelDebounce, debounceTask } from 'ember-lifeline';
import type CustomersApiService from 'web/services/customers-api';
import type ErrorDialogService from 'web/services/error-dialog';
import type RefrigerantLogApiService from 'web/services/refrigerant-log-api';
import type {
  RefrigerantLogEntry,
  RefrigerantLogEntryInput,
} from 'web/services/refrigerant-log-api';
import type ToastsService from 'web/services/toasts';
import {
  CUSTOMER_MESSAGE,
  validateRefrigerantLogField,
  type RefrigerantLogField,
} from 'web/utils/refrigerant-log-validation';

const LOOKUP_DELAY_MS = 300;
const SUGGESTION_COUNT = 10;
const MAX_PAGE_SIZE = 100;
const DISCARD_MESSAGE = 'Discard unsaved changes?';

type TextField = RefrigerantLogField | 'customer';

const VALIDATED_FIELDS: RefrigerantLogField[] = [
  'date',
  'technician',
  'refrigerantType',
  'amountLbs',
  'notes',
  'ticketNumber',
  'jobName',
  'vendorInvoiceRef',
];

interface RefrigerantLogForm {
  date: string;
  technician: string;
  refrigerantType: string;
  amountLbs: string;
  notes: string;
  ticketNumber: string;
  jobName: string;
  customer: string;
  vendorInvoiceRef: string;
}

// The date input takes and gives yyyy-mm-dd, in the user's own time zone.
function today(): string {
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
}

function toForm(entry: RefrigerantLogEntry | undefined): RefrigerantLogForm {
  return {
    date: entry?.date ?? today(),
    technician: entry?.technician ?? '',
    refrigerantType: entry?.refrigerantType ?? '',
    amountLbs: entry === undefined ? '' : String(entry.amountLbs),
    notes: entry?.notes ?? '',
    ticketNumber: entry?.ticketNumber ?? '',
    jobName: entry?.jobName ?? '',
    customer: entry?.customer ?? '',
    vendorInvoiceRef: entry?.vendorInvoiceRef ?? '',
  };
}

export interface RefrigerantLogFormSignature {
  Args: {
    // The entry to edit. Without one, the form adds a new entry.
    entry?: RefrigerantLogEntry;
    technicians: readonly string[];
    refrigerants: readonly string[];
  };
}

export default class RefrigerantLogFormComponent extends Component<RefrigerantLogFormSignature> {
  @service declare customersApi: CustomersApiService;
  @service declare errorDialog: ErrorDialogService;
  @service declare refrigerantLogApi: RefrigerantLogApiService;
  @service declare router: RouterService;
  @service declare toasts: ToastsService;

  private readonly saved = toForm(this.args.entry);

  @tracked date = this.saved.date;
  @tracked technician = this.saved.technician;
  @tracked refrigerantType = this.saved.refrigerantType;
  @tracked amountLbs = this.saved.amountLbs;
  @tracked notes = this.saved.notes;
  @tracked ticketNumber = this.saved.ticketNumber;
  @tracked jobName = this.saved.jobName;
  @tracked customer = this.saved.customer;
  @tracked vendorInvoiceRef = this.saved.vendorInvoiceRef;
  @tracked isSaving = false;
  @tracked private touched: ReadonlySet<string> = new Set();
  @tracked customerError: string | null = null;
  @tracked customerOptions: readonly string[] =
    this.saved.customer === '' ? [] : [this.saved.customer];

  get isDirty(): boolean {
    return (Object.keys(this.saved) as (keyof RefrigerantLogForm)[]).some(
      (field) => this[field] !== this.saved[field],
    );
  }

  // The active technicians. When the entry being edited was logged for a
  // technician who is no longer active, that technician stays in the list.
  get technicianNames(): readonly string[] {
    const { technicians } = this.args;
    return this.saved.technician === '' ||
      technicians.includes(this.saved.technician)
      ? technicians
      : [...technicians, this.saved.technician];
  }

  get errors(): Record<RefrigerantLogField, string | null> {
    const choices = {
      technicians: this.technicianNames,
      refrigerants: this.args.refrigerants,
    };
    const errors = {} as Record<RefrigerantLogField, string | null>;
    for (const field of VALIDATED_FIELDS) {
      errors[field] = validateRefrigerantLogField(field, this[field], choices);
    }
    return errors;
  }

  // A blank new entry would open with every required field in error, so a new
  // entry shows a field's error only once the user has changed that field. An
  // entry being edited shows its errors straight away.
  get shownErrors(): Record<RefrigerantLogField, string | null> {
    const errors = this.errors;
    if (this.args.entry !== undefined) {
      return errors;
    }

    const shown = { ...errors };
    for (const field of VALIDATED_FIELDS) {
      if (!this.touched.has(field)) {
        shown[field] = null;
      }
    }
    return shown;
  }

  get isValid(): boolean {
    const errors = this.errors;
    return VALIDATED_FIELDS.every((field) => errors[field] === null);
  }

  get saveDisabled(): boolean {
    return !this.isDirty || !this.isValid || this.isSaving;
  }

  get technicianOptions(): { value: string; selected: boolean }[] {
    return this.technicianNames.map((name) => ({
      value: name,
      selected: name === this.technician,
    }));
  }

  get technicianUnselected(): boolean {
    return !this.technicianNames.includes(this.technician);
  }

  get refrigerantOptions(): { value: string; selected: boolean }[] {
    return this.args.refrigerants.map((type) => ({
      value: type,
      selected: type === this.refrigerantType,
    }));
  }

  get refrigerantUnselected(): boolean {
    return !this.args.refrigerants.includes(this.refrigerantType);
  }

  willDestroy(): void {
    super.willDestroy();
    cancelDebounce(this, 'suggestCustomers');
  }

  @action
  setText(event: Event): void {
    const field = event.target as
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement;
    this[field.name as TextField] = field.value;
    this.touched = new Set([...this.touched, field.name]);
  }

  @action
  setCustomer(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.customer = value;
    this.customerError = null;
    debounceTask(this, 'suggestCustomers', value, LOOKUP_DELAY_MS);
  }

  async suggestCustomers(text: string): Promise<void> {
    const name = text.trim();
    if (name === '') {
      return;
    }

    try {
      const page = await this.customersApi.list({
        name,
        pageSize: SUGGESTION_COUNT,
      });
      if (!this.isDestroying && !this.isDestroyed) {
        this.customerOptions = page.data.map((customer) => customer.name);
      }
    } catch {
      // The suggestions are a convenience. Saving still checks the customer.
    }
  }

  // The customer must be the name of an existing customer. A name the user
  // picked from the suggestions is known to exist; anything else is looked up.
  private async customerExists(): Promise<boolean> {
    const name = this.customer.trim();
    if (name === '' || this.customerOptions.includes(name)) {
      return true;
    }

    const page = await this.customersApi.list({
      name,
      pageSize: MAX_PAGE_SIZE,
    });
    return page.data.some((customer) => customer.name === name);
  }

  @action
  async save(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (this.saveDisabled) {
      return;
    }

    this.isSaving = true;
    try {
      if (!(await this.customerExists())) {
        this.customerError = CUSTOMER_MESSAGE;
        return;
      }

      const entry: RefrigerantLogEntryInput = {
        date: this.date,
        technician: this.technician,
        refrigerantType: this.refrigerantType,
        amountLbs: Number(this.amountLbs),
        notes: this.notes,
        ticketNumber: this.ticketNumber,
        jobName: this.jobName,
        customer: this.customer,
        vendorInvoiceRef: this.vendorInvoiceRef,
      };
      if (this.args.entry === undefined) {
        await this.refrigerantLogApi.create(entry);
      } else {
        await this.refrigerantLogApi.update(this.args.entry.id, entry);
      }

      this.toasts.success('Refrigerant log entry saved.');
      this.router.transitionTo('refrigerant-log');
    } catch (error) {
      this.errorDialog.show(error);
    } finally {
      this.isSaving = false;
    }
  }

  @action
  back(): void {
    if (this.isDirty && !window.confirm(DISCARD_MESSAGE)) {
      return;
    }

    this.router.transitionTo('refrigerant-log');
  }

  <template>
    <form class="refrigerant-log-details" {{on "submit" this.save}}>
      <label for="refrigerant-log-date">Date</label>
      <input
        id="refrigerant-log-date"
        name="date"
        type="date"
        aria-describedby={{if
          this.shownErrors.date
          "refrigerant-log-date-error"
        }}
        value={{this.date}}
        {{on "input" this.setText}}
      />
      {{#if this.shownErrors.date}}
        <p class="field-error" id="refrigerant-log-date-error">
          {{this.shownErrors.date}}
        </p>
      {{/if}}

      <label for="refrigerant-log-technician">Technician</label>
      <select
        id="refrigerant-log-technician"
        name="technician"
        aria-invalid={{if this.shownErrors.technician "true"}}
        aria-describedby={{if
          this.shownErrors.technician
          "refrigerant-log-technician-error"
        }}
        {{on "change" this.setText}}
      >
        <option value="" selected={{this.technicianUnselected}}>Select a
          technician</option>
        {{#each this.technicianOptions as |choice|}}
          <option value={{choice.value}} selected={{choice.selected}}>
            {{choice.value}}
          </option>
        {{/each}}
      </select>
      {{#if this.shownErrors.technician}}
        <p class="field-error" id="refrigerant-log-technician-error">
          {{this.shownErrors.technician}}
        </p>
      {{/if}}

      <label for="refrigerant-log-refrigerant-type">Refrigerant Type</label>
      <select
        id="refrigerant-log-refrigerant-type"
        name="refrigerantType"
        aria-invalid={{if this.shownErrors.refrigerantType "true"}}
        aria-describedby={{if
          this.shownErrors.refrigerantType
          "refrigerant-log-refrigerant-type-error"
        }}
        {{on "change" this.setText}}
      >
        <option value="" selected={{this.refrigerantUnselected}}>Select a
          refrigerant type</option>
        {{#each this.refrigerantOptions as |choice|}}
          <option value={{choice.value}} selected={{choice.selected}}>
            {{choice.value}}
          </option>
        {{/each}}
      </select>
      {{#if this.shownErrors.refrigerantType}}
        <p class="field-error" id="refrigerant-log-refrigerant-type-error">
          {{this.shownErrors.refrigerantType}}
        </p>
      {{/if}}

      <label for="refrigerant-log-amount">Amount Used (lbs)</label>
      <input
        id="refrigerant-log-amount"
        name="amountLbs"
        type="number"
        inputmode="decimal"
        step="any"
        min="0"
        aria-invalid={{if this.shownErrors.amountLbs "true"}}
        aria-describedby={{if
          this.shownErrors.amountLbs
          "refrigerant-log-amount-error"
        }}
        value={{this.amountLbs}}
        {{on "input" this.setText}}
      />
      {{#if this.shownErrors.amountLbs}}
        <p class="field-error" id="refrigerant-log-amount-error">
          {{this.shownErrors.amountLbs}}
        </p>
      {{/if}}

      <label for="refrigerant-log-ticket-number">Ticket Number</label>
      <input
        id="refrigerant-log-ticket-number"
        name="ticketNumber"
        type="text"
        aria-invalid={{if this.shownErrors.ticketNumber "true"}}
        aria-describedby={{if
          this.shownErrors.ticketNumber
          "refrigerant-log-ticket-number-error"
        }}
        value={{this.ticketNumber}}
        {{on "input" this.setText}}
      />
      {{#if this.shownErrors.ticketNumber}}
        <p class="field-error" id="refrigerant-log-ticket-number-error">
          {{this.shownErrors.ticketNumber}}
        </p>
      {{/if}}

      <label for="refrigerant-log-job-name">Job Name</label>
      <input
        id="refrigerant-log-job-name"
        name="jobName"
        type="text"
        aria-invalid={{if this.shownErrors.jobName "true"}}
        aria-describedby={{if
          this.shownErrors.jobName
          "refrigerant-log-job-name-error"
        }}
        value={{this.jobName}}
        {{on "input" this.setText}}
      />
      {{#if this.shownErrors.jobName}}
        <p class="field-error" id="refrigerant-log-job-name-error">
          {{this.shownErrors.jobName}}
        </p>
      {{/if}}

      <label for="refrigerant-log-customer">Customer</label>
      <input
        id="refrigerant-log-customer"
        name="customer"
        type="text"
        list="refrigerant-log-customer-options"
        autocomplete="off"
        aria-invalid={{if this.customerError "true"}}
        aria-describedby={{if
          this.customerError
          "refrigerant-log-customer-error"
        }}
        value={{this.customer}}
        {{on "input" this.setCustomer}}
      />
      <datalist id="refrigerant-log-customer-options">
        {{#each this.customerOptions as |name|}}
          <option value={{name}}></option>
        {{/each}}
      </datalist>
      {{#if this.customerError}}
        <p class="field-error" id="refrigerant-log-customer-error">
          {{this.customerError}}
        </p>
      {{/if}}

      <label for="refrigerant-log-vendor-invoice-ref">Vendor Invoice Ref</label>
      <input
        id="refrigerant-log-vendor-invoice-ref"
        name="vendorInvoiceRef"
        type="text"
        aria-invalid={{if this.shownErrors.vendorInvoiceRef "true"}}
        aria-describedby={{if
          this.shownErrors.vendorInvoiceRef
          "refrigerant-log-vendor-invoice-ref-error"
        }}
        value={{this.vendorInvoiceRef}}
        {{on "input" this.setText}}
      />
      {{#if this.shownErrors.vendorInvoiceRef}}
        <p class="field-error" id="refrigerant-log-vendor-invoice-ref-error">
          {{this.shownErrors.vendorInvoiceRef}}
        </p>
      {{/if}}

      <label for="refrigerant-log-notes">Notes</label>
      <textarea
        id="refrigerant-log-notes"
        name="notes"
        rows="5"
        aria-invalid={{if this.shownErrors.notes "true"}}
        aria-describedby={{if
          this.shownErrors.notes
          "refrigerant-log-notes-error"
        }}
        value={{this.notes}}
        {{on "input" this.setText}}
      ></textarea>
      {{#if this.shownErrors.notes}}
        <p class="field-error" id="refrigerant-log-notes-error">
          {{this.shownErrors.notes}}
        </p>
      {{/if}}

      <div class="form-actions">
        <button type="submit" disabled={{this.saveDisabled}}>
          Save
        </button>
        <button type="button" {{on "click" this.back}}>
          Back to List
        </button>
      </div>
    </form>
  </template>
}
