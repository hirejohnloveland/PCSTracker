import Controller from '@ember/controller';
import { action } from '@ember/object';
import type RouterService from '@ember/routing/router-service';
import { service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import type CustomersApiService from 'web/services/customers-api';
import type { Customer } from 'web/services/customers-api';
import type ErrorDialogService from 'web/services/error-dialog';
import type ToastsService from 'web/services/toasts';
import {
  validateCustomerField,
  type CustomerTextField,
} from 'web/utils/customer-validation';

const TEXT_FIELDS: CustomerTextField[] = [
  'name',
  'description',
  'contact',
  'phone',
  'notes',
  'quickbooksId',
];

interface CustomerForm {
  name: string;
  description: string;
  contact: string;
  phone: string;
  notes: string;
  quickbooksId: string;
}

const DISCARD_MESSAGE = 'Discard unsaved changes?';

function toForm(customer: Customer): CustomerForm {
  return {
    name: customer.name,
    description: customer.description ?? '',
    contact: customer.contact ?? '',
    phone: customer.phone ?? '',
    notes: customer.notes ?? '',
    quickbooksId: customer.quickbooksId ?? '',
  };
}

export default class CustomersDetailsController extends Controller {
  @service declare customersApi: CustomersApiService;
  @service declare errorDialog: ErrorDialogService;
  @service declare router: RouterService;
  @service declare toasts: ToastsService;

  @tracked name = '';
  @tracked description = '';
  @tracked contact = '';
  @tracked phone = '';
  @tracked notes = '';
  @tracked quickbooksId = '';
  @tracked isSaving = false;
  @tracked private saved: CustomerForm = toForm({
    id: '',
    name: '',
    quickbooksId: null,
    description: null,
    contact: null,
    phone: null,
    notes: null,
  });

  private customerId = '';

  get isDirty(): boolean {
    return (
      this.name !== this.saved.name ||
      this.description !== this.saved.description ||
      this.contact !== this.saved.contact ||
      this.phone !== this.saved.phone ||
      this.notes !== this.saved.notes ||
      this.quickbooksId !== this.saved.quickbooksId
    );
  }

  get errors(): Record<CustomerTextField, string | null> {
    return {
      name: validateCustomerField('name', this.name),
      description: validateCustomerField('description', this.description),
      contact: validateCustomerField('contact', this.contact),
      phone: validateCustomerField('phone', this.phone),
      notes: validateCustomerField('notes', this.notes),
      quickbooksId: validateCustomerField('quickbooksId', this.quickbooksId),
    };
  }

  get isValid(): boolean {
    const errors = this.errors;
    return TEXT_FIELDS.every((field) => errors[field] === null);
  }

  get saveDisabled(): boolean {
    return !this.isDirty || !this.isValid || this.isSaving;
  }

  load(customer: Customer): void {
    this.customerId = customer.id;
    this.isSaving = false;
    this.applySaved(customer);
  }

  @action
  setText(event: Event): void {
    const field = event.target as HTMLInputElement | HTMLTextAreaElement;
    this[field.name as CustomerTextField] = field.value;
  }

  @action
  async save(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (this.saveDisabled) {
      return;
    }

    this.isSaving = true;
    try {
      const response = await this.customersApi.update(this.customerId, {
        name: this.name,
        description: this.description,
        contact: this.contact,
        phone: this.phone,
        notes: this.notes,
        quickbooksId: this.quickbooksId,
      });
      this.applySaved(response.data);
      this.toasts.success('Customer saved.');
      this.router.transitionTo('customers');
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

    this.router.transitionTo('customers');
  }

  private applySaved(customer: Customer): void {
    const form = toForm(customer);
    this.saved = form;
    this.name = form.name;
    this.description = form.description;
    this.contact = form.contact;
    this.phone = form.phone;
    this.notes = form.notes;
    this.quickbooksId = form.quickbooksId;
  }
}
