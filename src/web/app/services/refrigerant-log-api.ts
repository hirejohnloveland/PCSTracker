import Service, { service } from '@ember/service';
import type ApiClientService from 'web/services/api-client';
import type { components, paths } from 'web/types/api';

export type RefrigerantLogEntry =
  components['schemas']['RefrigerantLogEntryResponseDto'];
export type RefrigerantLogEntryResponse =
  components['schemas']['ApiResponseOfRefrigerantLogEntryResponseDto'];
export type RefrigerantLogEntryInput =
  components['schemas']['RefrigerantLogEntryDto'];
export type RefrigerantLogPage =
  components['schemas']['PagedApiResponseOfRefrigerantLogEntryResponseDto'];
export type RefrigerantTypesResponse =
  components['schemas']['ApiResponseOfRefrigerantTypesResponseDto'];
export type ListRefrigerantLogQuery = NonNullable<
  paths['/refrigerantlog']['get']['parameters']['query']
>;

export default class RefrigerantLogApiService extends Service {
  @service declare apiClient: ApiClientService;

  async list(query: ListRefrigerantLogQuery): Promise<RefrigerantLogPage> {
    return this.apiClient.getJson<RefrigerantLogPage>('/refrigerantlog', query);
  }

  async get(id: string): Promise<RefrigerantLogEntryResponse> {
    return this.apiClient.getJson<RefrigerantLogEntryResponse>(
      `/refrigerantlog/${encodeURIComponent(id)}`,
    );
  }

  async refrigerants(): Promise<RefrigerantTypesResponse> {
    return this.apiClient.getJson<RefrigerantTypesResponse>(
      '/refrigerantlog/refrigerants',
    );
  }

  async create(
    entry: RefrigerantLogEntryInput,
  ): Promise<RefrigerantLogEntryResponse> {
    return this.apiClient.postJson<RefrigerantLogEntryResponse>(
      '/refrigerantlog',
      entry,
    );
  }

  async update(
    id: string,
    entry: RefrigerantLogEntryInput,
  ): Promise<RefrigerantLogEntryResponse> {
    return this.apiClient.putJson<RefrigerantLogEntryResponse>(
      `/refrigerantlog/${encodeURIComponent(id)}`,
      entry,
    );
  }
}

declare module '@ember/service' {
  interface Registry {
    'refrigerant-log-api': RefrigerantLogApiService;
  }
}
