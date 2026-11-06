import Service, { service } from '@ember/service';
import type ApiClientService from 'web/services/api-client';
import type { components, paths } from 'web/types/api';

export type Customer = components['schemas']['CustomerResponseDto'];
export type CustomerResponse =
  components['schemas']['ApiResponseOfCustomerResponseDto'];
export type CustomerUpdate = components['schemas']['CustomerDto'];
export type CustomerPage =
  components['schemas']['PagedApiResponseOfCustomerResponseDto'];
export type ListCustomersQuery = NonNullable<
  paths['/customers']['get']['parameters']['query']
>;

export default class CustomersApiService extends Service {
  @service declare apiClient: ApiClientService;

  async list(query: ListCustomersQuery): Promise<CustomerPage> {
    return this.apiClient.getJson<CustomerPage>('/customers', query);
  }

  async get(id: string): Promise<CustomerResponse> {
    return this.apiClient.getJson<CustomerResponse>(
      `/customers/${encodeURIComponent(id)}`,
    );
  }

  async update(
    id: string,
    customer: CustomerUpdate,
  ): Promise<CustomerResponse> {
    return this.apiClient.putJson<CustomerResponse>(
      `/customers/${encodeURIComponent(id)}`,
      customer,
    );
  }
}
