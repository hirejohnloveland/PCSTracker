import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type VendorsApiService from 'web/services/vendors-api';

type VendorsIndexParams = {
  name: string;
  page: number;
};

export default class VendorsIndexRoute extends Route {
  @service declare vendorsApi: VendorsApiService;

  queryParams = {
    name: { refreshModel: true, replace: true },
    page: { refreshModel: true },
  };

  async model(params: Record<string, unknown>) {
    const { name, page } = params as VendorsIndexParams;
    return this.vendorsApi.list({ name, page });
  }
}
