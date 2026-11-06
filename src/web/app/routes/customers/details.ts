import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type CustomersDetailsController from 'web/controllers/customers/details';
import type CustomersApiService from 'web/services/customers-api';
import type { Customer } from 'web/services/customers-api';

export default class CustomersDetailsRoute extends Route {
  @service declare customersApi: CustomersApiService;

  async model(params: Record<string, unknown>): Promise<Customer> {
    const response = await this.customersApi.get(
      params['customer_id'] as string,
    );
    return response.data;
  }

  setupController(
    controller: CustomersDetailsController,
    model: Customer,
    transition: Parameters<Route['setupController']>[2],
  ): void {
    super.setupController(controller, model, transition);
    controller.load(model);
  }
}
