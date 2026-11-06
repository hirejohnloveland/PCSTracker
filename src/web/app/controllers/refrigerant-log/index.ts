import Controller from '@ember/controller';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { cancelDebounce, debounceTask } from 'ember-lifeline';
import type { RefrigerantLogIndexModel } from 'web/routes/refrigerant-log/index';

const FILTER_DELAY_MS = 300;

type SelectFilter = 'dateFrom' | 'dateTo' | 'technician';

export default class RefrigerantLogIndexController extends Controller {
  queryParams = ['dateFrom', 'dateTo', 'technician', 'customer', 'page'];

  @tracked dateFrom = '';
  @tracked dateTo = '';
  @tracked technician = '';
  @tracked customer = '';
  @tracked page = 1;

  get hasFilters(): boolean {
    return (
      this.dateFrom !== '' ||
      this.dateTo !== '' ||
      this.technician !== '' ||
      this.customer !== ''
    );
  }

  // The technicians the route loaded. A technician in the URL that is not among
  // them is still offered, so the filter shows what the list is filtered by.
  get technicianOptions(): { value: string; selected: boolean }[] {
    const { technicians } = this.model as RefrigerantLogIndexModel;
    const names =
      this.technician === '' || technicians.includes(this.technician)
        ? technicians
        : [...technicians, this.technician];

    return names.map((name) => ({
      value: name,
      selected: name === this.technician,
    }));
  }

  get dateRangeError(): string | null {
    return this.dateFrom !== '' &&
      this.dateTo !== '' &&
      this.dateTo < this.dateFrom
      ? 'Date to must be on or after date from.'
      : null;
  }

  @action
  clearFilters(): void {
    cancelDebounce(this, 'applyCustomer');
    this.dateFrom = '';
    this.dateTo = '';
    this.technician = '';
    this.customer = '';
    this.page = 1;
  }

  // The date and technician inputs apply as soon as the user changes them.
  @action
  applyFilter(event: Event): void {
    const input = event.target as HTMLInputElement | HTMLSelectElement;
    const name = input.name as SelectFilter;
    if (input.value !== this[name]) {
      this[name] = input.value;
      this.page = 1;
    }
  }

  @action
  filterCustomerAsYouType(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    debounceTask(this, 'applyCustomer', value, FILTER_DELAY_MS);
  }

  @action
  filterNow(event: SubmitEvent): void {
    event.preventDefault();
    cancelDebounce(this, 'applyCustomer');
    const value = new FormData(event.currentTarget as HTMLFormElement).get(
      'customer',
    );
    this.applyCustomer(typeof value === 'string' ? value : '');
  }

  applyCustomer(value: string): void {
    const customer = value.trim();
    if (customer !== this.customer) {
      this.customer = customer;
      this.page = 1;
    }
  }
}
