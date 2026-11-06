import Controller from '@ember/controller';
import { action } from '@ember/object';
import { service } from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { cancelDebounce, debounceTask } from 'ember-lifeline';
import type ErrorDialogService from 'web/services/error-dialog';
import type QuickbooksApiService from 'web/services/quickbooks-api';

const FILTER_DELAY_MS = 300;

export default class CustomersIndexController extends Controller {
  @service declare errorDialog: ErrorDialogService;
  @service declare quickbooksApi: QuickbooksApiService;

  queryParams = ['name', 'page'];

  @tracked name = '';
  @tracked page = 1;

  get hasFilters(): boolean {
    return this.name !== '';
  }

  @action
  clearFilters(): void {
    cancelDebounce(this, 'applyName');
    this.applyName('');
  }

  @action
  filterAsYouType(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    debounceTask(this, 'applyName', value, FILTER_DELAY_MS);
  }

  @action
  filterNow(event: SubmitEvent): void {
    event.preventDefault();
    cancelDebounce(this, 'applyName');
    const value = new FormData(event.currentTarget as HTMLFormElement).get(
      'name',
    );
    this.applyName(typeof value === 'string' ? value : '');
  }

  @action
  async syncWithQuickbooks(): Promise<void> {
    try {
      await this.quickbooksApi.syncCustomers();
    } catch (error) {
      this.errorDialog.show(error);
    }
  }

  applyName(value: string): void {
    const name = value.trim();
    if (name !== this.name) {
      this.name = name;
      this.page = 1;
    }
  }
}
