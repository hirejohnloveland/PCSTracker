import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type CustomersApiService from 'web/services/customers-api';

type CustomersIndexParams = {
  name: string;
  page: number;
};

export default class CustomersIndexRoute extends Route {
  @service declare customersApi: CustomersApiService;

  queryParams = {
    name: { refreshModel: true, replace: true },
    page: { refreshModel: true },
  };

  async model(params: Record<string, unknown>) {
    const { name, page } = params as CustomersIndexParams;
    return this.customersApi.list({ name, page });
  }
}
