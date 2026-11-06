import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type RefrigerantLogApiService from 'web/services/refrigerant-log-api';
import type TechniciansApiService from 'web/services/technicians-api';

const MAX_PAGE_SIZE = 100;

export interface RefrigerantLogNewModel {
  technicians: readonly string[];
  refrigerants: readonly string[];
}

export default class RefrigerantLogNewRoute extends Route {
  @service declare refrigerantLogApi: RefrigerantLogApiService;
  @service declare techniciansApi: TechniciansApiService;

  async model(): Promise<RefrigerantLogNewModel> {
    const [technicians, refrigerants] = await Promise.all([
      this.techniciansApi.list({ active: true, pageSize: MAX_PAGE_SIZE }),
      this.refrigerantLogApi.refrigerants(),
    ]);

    return {
      technicians: technicians.data.map((technician) => technician.fullName),
      refrigerants: refrigerants.data.refrigerants,
    };
  }
}
