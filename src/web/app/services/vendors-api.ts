import Service, { service } from '@ember/service';
import type ApiClientService from 'web/services/api-client';
import type { components, paths } from 'web/types/api';

export type Vendor = components['schemas']['VendorResponseDto'];
export type VendorPage =
  components['schemas']['PagedApiResponseOfVendorResponseDto'];
export type ListVendorsQuery = NonNullable<
  paths['/vendors']['get']['parameters']['query']
>;

export default class VendorsApiService extends Service {
  @service declare apiClient: ApiClientService;

  async list(query: ListVendorsQuery): Promise<VendorPage> {
    return this.apiClient.getJson<VendorPage>('/vendors', query);
  }
}
