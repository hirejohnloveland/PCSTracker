import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type VendorsDetailsController from 'web/controllers/vendors/details';
import type VendorsApiService from 'web/services/vendors-api';
import type { Vendor } from 'web/services/vendors-api';

export default class VendorsDetailsRoute extends Route {
  @service declare vendorsApi: VendorsApiService;

  async model(params: Record<string, unknown>): Promise<Vendor> {
    const response = await this.vendorsApi.get(params['vendor_id'] as string);
    return response.data;
  }

  setupController(
    controller: VendorsDetailsController,
    model: Vendor,
    transition: Parameters<Route['setupController']>[2],
  ): void {
    super.setupController(controller, model, transition);
    controller.load(model);
  }
}
