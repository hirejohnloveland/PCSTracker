import Controller from '@ember/controller';
import { action } from '@ember/object';
import type RouterService from '@ember/routing/router-service';
import { service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import type ErrorDialogService from 'web/services/error-dialog';
import type VendorsApiService from 'web/services/vendors-api';
import type { Vendor } from 'web/services/vendors-api';

type TextField =
  'name' | 'description' | 'contact' | 'phone' | 'notes' | 'quickbooksId';

interface VendorForm {
  name: string;
  description: string;
  contact: string;
  phone: string;
  notes: string;
  quickbooksId: string;
}

const DISCARD_MESSAGE = 'Discard unsaved changes?';

function toForm(vendor: Vendor): VendorForm {
  return {
    name: vendor.name,
    description: vendor.description ?? '',
    contact: vendor.contact ?? '',
    phone: vendor.phone ?? '',
    notes: vendor.notes ?? '',
    quickbooksId: vendor.quickbooksId ?? '',
  };
}

export default class VendorsDetailsController extends Controller {
  @service declare errorDialog: ErrorDialogService;
  @service declare router: RouterService;
  @service declare vendorsApi: VendorsApiService;

  @tracked name = '';
  @tracked description = '';
  @tracked contact = '';
  @tracked phone = '';
  @tracked notes = '';
  @tracked quickbooksId = '';
  @tracked isSaving = false;
  @tracked private saved: VendorForm = toForm({
    id: '',
    name: '',
    quickbooksId: null,
    description: null,
    contact: null,
    phone: null,
    notes: null,
  });

  private vendorId = '';

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

  get saveDisabled(): boolean {
    return !this.isDirty || this.isSaving;
  }

  load(vendor: Vendor): void {
    this.vendorId = vendor.id;
    this.isSaving = false;
    this.applySaved(vendor);
  }

  @action
  setText(event: Event): void {
    const field = event.target as HTMLInputElement | HTMLTextAreaElement;
    this[field.name as TextField] = field.value;
  }

  @action
  async save(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (this.saveDisabled) {
      return;
    }

    this.isSaving = true;
    try {
      const response = await this.vendorsApi.update(this.vendorId, {
        name: this.name,
        description: this.description,
        contact: this.contact,
        phone: this.phone,
        notes: this.notes,
        quickbooksId: this.quickbooksId,
      });
      this.applySaved(response.data);
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

    this.router.transitionTo('vendors');
  }

  private applySaved(vendor: Vendor): void {
    const form = toForm(vendor);
    this.saved = form;
    this.name = form.name;
    this.description = form.description;
    this.contact = form.contact;
    this.phone = form.phone;
    this.notes = form.notes;
    this.quickbooksId = form.quickbooksId;
  }
}
