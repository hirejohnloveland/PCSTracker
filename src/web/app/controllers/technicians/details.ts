import Controller from '@ember/controller';
import { action } from '@ember/object';
import type RouterService from '@ember/routing/router-service';
import { service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import type ErrorDialogService from 'web/services/error-dialog';
import type TechniciansApiService from 'web/services/technicians-api';
import type { Technician } from 'web/services/technicians-api';
import type ToastsService from 'web/services/toasts';
import {
  validateTechnicianField,
  type TechnicianTextField,
} from 'web/utils/technician-validation';

const TEXT_FIELDS: TechnicianTextField[] = [
  'fullName',
  'role',
  'phone',
  'notes',
  'quickbooksId',
];

interface TechnicianForm {
  fullName: string;
  role: string;
  active: boolean;
  phone: string;
  notes: string;
  quickbooksId: string;
}

const DISCARD_MESSAGE = 'Discard unsaved changes?';

function toForm(technician: Technician): TechnicianForm {
  return {
    fullName: technician.fullName,
    role: technician.role,
    active: technician.active,
    phone: technician.phone ?? '',
    notes: technician.notes ?? '',
    quickbooksId: technician.quickbooksId ?? '',
  };
}

export default class TechniciansDetailsController extends Controller {
  @service declare errorDialog: ErrorDialogService;
  @service declare router: RouterService;
  @service declare techniciansApi: TechniciansApiService;
  @service declare toasts: ToastsService;

  @tracked fullName = '';
  @tracked role = '';
  @tracked active = true;
  @tracked phone = '';
  @tracked notes = '';
  @tracked quickbooksId = '';
  @tracked isSaving = false;
  @tracked roles: readonly string[] = [];
  @tracked private saved: TechnicianForm = toForm({
    id: '',
    fullName: '',
    role: '',
    active: true,
    quickbooksId: null,
    phone: null,
    notes: null,
  });

  private technicianId = '';

  get isDirty(): boolean {
    return (
      this.fullName !== this.saved.fullName ||
      this.role !== this.saved.role ||
      this.active !== this.saved.active ||
      this.phone !== this.saved.phone ||
      this.notes !== this.saved.notes ||
      this.quickbooksId !== this.saved.quickbooksId
    );
  }

  get errors(): Record<TechnicianTextField, string | null> {
    return {
      fullName: validateTechnicianField('fullName', this.fullName, this.roles),
      role: validateTechnicianField('role', this.role, this.roles),
      phone: validateTechnicianField('phone', this.phone, this.roles),
      notes: validateTechnicianField('notes', this.notes, this.roles),
      quickbooksId: validateTechnicianField(
        'quickbooksId',
        this.quickbooksId,
        this.roles,
      ),
    };
  }

  get isValid(): boolean {
    const errors = this.errors;
    return TEXT_FIELDS.every((field) => errors[field] === null);
  }

  get saveDisabled(): boolean {
    return !this.isDirty || !this.isValid || this.isSaving;
  }

  // The role dropdown offers the configured roles. When the technician's role is
  // not one of them, nothing is selected until the user picks one.
  get roleOptions(): { value: string; selected: boolean }[] {
    return this.roles.map((role) => ({
      value: role,
      selected: role === this.role,
    }));
  }

  get roleUnselected(): boolean {
    return !this.roles.includes(this.role);
  }

  load(technician: Technician, roles: readonly string[]): void {
    this.technicianId = technician.id;
    this.roles = roles;
    this.isSaving = false;
    this.applySaved(technician);
  }

  @action
  setText(event: Event): void {
    const field = event.target as
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement;
    this[field.name as TechnicianTextField] = field.value;
  }

  @action
  setActive(event: Event): void {
    this.active = (event.target as HTMLInputElement).checked;
  }

  @action
  async save(event: SubmitEvent): Promise<void> {
    event.preventDefault();
    if (this.saveDisabled) {
      return;
    }

    this.isSaving = true;
    try {
      const response = await this.techniciansApi.update(this.technicianId, {
        fullName: this.fullName,
        role: this.role,
        active: this.active,
        phone: this.phone,
        notes: this.notes,
        quickbooksId: this.quickbooksId,
      });
      this.applySaved(response.data);
      this.toasts.success('Technician saved.');
      this.router.transitionTo('technicians');
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

    this.router.transitionTo('technicians');
  }

  private applySaved(technician: Technician): void {
    const form = toForm(technician);
    this.saved = form;
    this.fullName = form.fullName;
    this.role = form.role;
    this.active = form.active;
    this.phone = form.phone;
    this.notes = form.notes;
    this.quickbooksId = form.quickbooksId;
  }
}
