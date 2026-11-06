import Route from '@ember/routing/route';
import { service } from '@ember/service';
import type TechniciansApiService from 'web/services/technicians-api';

type TechniciansIndexParams = {
  name: string;
  page: number;
};

export default class TechniciansIndexRoute extends Route {
  @service declare techniciansApi: TechniciansApiService;

  queryParams = {
    name: { refreshModel: true, replace: true },
    page: { refreshModel: true },
  };

  async model(params: Record<string, unknown>) {
    const { name, page } = params as TechniciansIndexParams;
    return this.techniciansApi.list({ name, page });
  }
}
