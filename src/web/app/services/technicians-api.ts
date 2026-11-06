import Service, { service } from '@ember/service';
import type ApiClientService from 'web/services/api-client';
import type { components, paths } from 'web/types/api';

export type Technician = components['schemas']['TechnicianResponseDto'];
export type TechnicianResponse =
  components['schemas']['ApiResponseOfTechnicianResponseDto'];
export type TechnicianUpdate = components['schemas']['TechnicianDto'];
export type TechnicianPage =
  components['schemas']['PagedApiResponseOfTechnicianResponseDto'];
export type TechnicianRolesResponse =
  components['schemas']['ApiResponseOfTechnicianRolesResponseDto'];
export type ListTechniciansQuery = NonNullable<
  paths['/technicians']['get']['parameters']['query']
>;

export default class TechniciansApiService extends Service {
  @service declare apiClient: ApiClientService;

  async list(query: ListTechniciansQuery): Promise<TechnicianPage> {
    return this.apiClient.getJson<TechnicianPage>('/technicians', query);
  }

  async get(id: string): Promise<TechnicianResponse> {
    return this.apiClient.getJson<TechnicianResponse>(
      `/technicians/${encodeURIComponent(id)}`,
    );
  }

  async roles(): Promise<TechnicianRolesResponse> {
    return this.apiClient.getJson<TechnicianRolesResponse>(
      '/technicians/roles',
    );
  }

  async update(
    id: string,
    technician: TechnicianUpdate,
  ): Promise<TechnicianResponse> {
    return this.apiClient.putJson<TechnicianResponse>(
      `/technicians/${encodeURIComponent(id)}`,
      technician,
    );
  }
}
