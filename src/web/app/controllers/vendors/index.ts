import Controller from '@ember/controller';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';
import { cancelDebounce, debounceTask } from 'ember-lifeline';

const FILTER_DELAY_MS = 300;

export default class VendorsIndexController extends Controller {
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

  applyName(value: string): void {
    const name = value.trim();
    if (name !== this.name) {
      this.name = name;
      this.page = 1;
    }
  }
}
