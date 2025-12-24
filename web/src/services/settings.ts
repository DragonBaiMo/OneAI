import api, { ApiException } from './api'
import type {
  SystemSettingsDto,
  UpdateSystemSettingRequest,
  ApiKeyInfoResponse,
  TokenRefreshInfoResponse,
} from '@/types/settings'

/**
 * 系统设置服务
 */
export const settingsService = {
  /**
   * 获取所有设置
   */
  async getAllSettings(): Promise<Record<string, SystemSettingsDto>> {
    const response = await api.get<Record<string, SystemSettingsDto>>('/settings')
    return response
  },

  /**
   * 获取单个设置
   */
  async getSetting(key: string): Promise<SystemSettingsDto | null> {
    try {
      const response = await api.get<SystemSettingsDto>(`/settings/${key}`)
      return response
    } catch (error) {
      if (error instanceof ApiException && error.code === 404) {
        return null
      }
      throw error
    }
  },

  /**
   * 更新设置
   */
  async updateSetting(
    key: string,
    request: UpdateSystemSettingRequest
  ): Promise<SystemSettingsDto> {
    const response = await api.put<SystemSettingsDto>(
      `/settings/${key}`,
      request
    )
    return response
  },

  /**
   * 获取 API Key 信息
   */
  async getApiKeyInfo(): Promise<ApiKeyInfoResponse> {
    const response = await api.get<ApiKeyInfoResponse>('/settings/info/api-key')
    return response
  },

  /**
   * 获取 Token 刷新信息
   */
  async getTokenRefreshInfo(): Promise<TokenRefreshInfoResponse> {
    const response = await api.get<TokenRefreshInfoResponse>(
      '/settings/info/token-refresh'
    )
    return response
  },
}
