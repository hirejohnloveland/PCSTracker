import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type RefrigerantLogApiService from 'web/services/refrigerant-log-api';
import type { RefrigerantLogEntry } from 'web/services/refrigerant-log-api';
import type TechniciansApiService from 'web/services/technicians-api';

const MAX_PAGE_SIZE = 100;

export interface RefrigerantLogDetailsModel {
  entry: RefrigerantLogEntry;
  technicians: readonly string[];
  refrigerants: readonly string[];
}

export default class RefrigerantLogDetailsRoute extends Route {
  @service declare refrigerantLogApi: RefrigerantLogApiService;
  @service declare techniciansApi: TechniciansApiService;

  async model(
    params: Record<string, unknown>,
  ): Promise<RefrigerantLogDetailsModel> {
    const [entry, technicians, refrigerants] = await Promise.all([
      this.refrigerantLogApi.get(params['entry_id'] as string),
      this.techniciansApi.list({ active: true, pageSize: MAX_PAGE_SIZE }),
      this.refrigerantLogApi.refrigerants(),
    ]);

    return {
      entry: entry.data,
      technicians: technicians.data.map((technician) => technician.fullName),
      refrigerants: refrigerants.data.refrigerants,
    };
  }
}
