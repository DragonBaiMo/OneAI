/**
 * 系统设置 DTO
 */
export interface SystemSettingsDto {
  key: string
  value: string | null
  description: string | null
  dataType: string
  isEditable: boolean
  createdAt: string
  updatedAt: string | null
}

/**
 * 设置更新请求
 */
export interface UpdateSystemSettingRequest {
  value: string | null
  description: string | null
}

/**
 * API Key 信息响应
 */
export interface ApiKeyInfoResponse {
  hasApiKey: boolean
  maskedApiKey: string | null
  apiKeyLength: number | null
}

/**
 * Token 刷新信息响应
 */
export interface TokenRefreshInfoResponse {
  tokenExpiresAt: string | null
  secondsUntilExpiry: number | null
  needsRefresh: boolean
  refreshBeforeExpiryMinutes: number
}

/**
 * 设置分组
 */
export interface SettingsGroup {
  title: string
  description: string
  settings: SystemSettingsDto[]
}
