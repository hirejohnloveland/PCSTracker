import Service, { service } from '@ember/service';
import type ApiClientService from 'web/services/api-client';
import type { components } from 'web/types/api';

export type QuickbooksSync =
  components['schemas']['ApiResponseOfQuickbooksSyncResponseDto'];

export default class QuickbooksApiService extends Service {
  @service declare apiClient: ApiClientService;

  async syncVendors(): Promise<QuickbooksSync> {
    return this.apiClient.postJson<QuickbooksSync>('/quickbooks/vendors/sync');
  }

  async syncCustomers(): Promise<QuickbooksSync> {
    return this.apiClient.postJson<QuickbooksSync>(
      '/quickbooks/customers/sync',
    );
  }
}
