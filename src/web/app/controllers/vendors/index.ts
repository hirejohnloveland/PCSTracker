import Controller from '@ember/controller';
import { action } from '@ember/object';
import { tracked } from '@glimmer/tracking';

export default class VendorsIndexController extends Controller {
  queryParams = ['name', 'page'];

  @tracked name = '';
  @tracked page = 1;

  @action
  applyFilter(event: SubmitEvent): void {
    event.preventDefault();
    const value = new FormData(event.currentTarget as HTMLFormElement).get(
      'name',
    );
    this.name = typeof value === 'string' ? value.trim() : '';
    this.page = 1;
  }
}
