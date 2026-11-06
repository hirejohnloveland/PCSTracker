import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type TechniciansDetailsController from 'web/controllers/technicians/details';
import type TechniciansApiService from 'web/services/technicians-api';
import type { Technician } from 'web/services/technicians-api';

interface TechniciansDetailsModel {
  technician: Technician;
  roles: readonly string[];
}

export default class TechniciansDetailsRoute extends Route {
  @service declare techniciansApi: TechniciansApiService;

  async model(
    params: Record<string, unknown>,
  ): Promise<TechniciansDetailsModel> {
    const [technician, roles] = await Promise.all([
      this.techniciansApi.get(params['technician_id'] as string),
      this.techniciansApi.roles(),
    ]);

    return { technician: technician.data, roles: roles.data.roles };
  }

  setupController(
    controller: TechniciansDetailsController,
    model: TechniciansDetailsModel,
    transition: Parameters<Route['setupController']>[2],
  ): void {
    super.setupController(controller, model, transition);
    controller.load(model.technician, model.roles);
  }
}
