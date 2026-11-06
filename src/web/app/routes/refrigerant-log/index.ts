import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type RefrigerantLogApiService from 'web/services/refrigerant-log-api';
import type { RefrigerantLogPage } from 'web/services/refrigerant-log-api';
import type TechniciansApiService from 'web/services/technicians-api';

const MAX_PAGE_SIZE = 100;

type RefrigerantLogIndexParams = {
  dateFrom: string;
  dateTo: string;
  technician: string;
  customer: string;
  page: number;
};

export interface RefrigerantLogIndexModel {
  entries: RefrigerantLogPage;
  technicians: readonly string[];
}

export default class RefrigerantLogIndexRoute extends Route {
  @service declare refrigerantLogApi: RefrigerantLogApiService;
  @service declare techniciansApi: TechniciansApiService;

  queryParams = {
    dateFrom: { refreshModel: true, replace: true },
    dateTo: { refreshModel: true, replace: true },
    technician: { refreshModel: true, replace: true },
    customer: { refreshModel: true, replace: true },
    page: { refreshModel: true },
  };

  async model(
    params: Record<string, unknown>,
  ): Promise<RefrigerantLogIndexModel> {
    const { dateFrom, dateTo, technician, customer, page } =
      params as RefrigerantLogIndexParams;

    // Every technician is offered, not only active ones, so the history of a
    // technician who has left can still be found.
    const technicians = await this.techniciansApi.list({
      pageSize: MAX_PAGE_SIZE,
    });
    const technicianNames = technicians.data.map(
      (technician) => technician.fullName,
    );

    // The API rejects a range that ends before it starts. The page says so, and
    // no entry can be in such a range.
    if (dateFrom !== '' && dateTo !== '' && dateTo < dateFrom) {
      return {
        entries: {
          data: [],
          pagination: { page: 1, pageSize: 25, totalCount: 0, totalPages: 0 },
          metadata: { traceId: '', timestamp: new Date().toISOString() },
        },
        technicians: technicianNames,
      };
    }

    const entries = await this.refrigerantLogApi.list({
      dateFrom,
      dateTo,
      technician,
      customer,
      page,
    });

    return { entries, technicians: technicianNames };
  }
}
