import { get, del, post, patch } from './api'
import type { AIAccountDto, GenerateOAuthUrlResponse, ExchangeOAuthCodeRequest, AccountQuotaStatus } from '@/types/account'

/**
 * AI 账户服务
 */
export const accountService = {
  /**
   * 获取 AI 账户列表
   */
  getAccounts(): Promise<AIAccountDto[]> {
    return get<AIAccountDto[]>('/accounts')
  },

  /**
   * 删除 AI 账户
   */
  deleteAccount(id: number): Promise<void> {
    return del<void>(`/accounts/${id}`)
  },

  /**
   * 切换 AI 账户的启用/禁用状态
   */
  toggleAccountStatus(id: number): Promise<AIAccountDto> {
    return patch<AIAccountDto>(`/accounts/${id}/toggle-status`, {})
  },

  /**
   * 批量获取账户配额状态
   */
  getAccountQuotaStatuses(accountIds: number[]): Promise<Record<number, AccountQuotaStatus>> {
    return post<Record<number, AccountQuotaStatus>>('/accounts/quota-statuses', accountIds)
  },
}

/**
 * OpenAI OAuth 服务
 */
export const openaiOAuthService = {
  /**
   * 生成 OpenAI OAuth 授权链接
   */
  generateOAuthUrl(proxy?: any): Promise<GenerateOAuthUrlResponse> {
    return post<GenerateOAuthUrlResponse>('/openai/oauth/authorize', {
      proxy: proxy || null
    })
  },

  /**
   * 交换授权码获取 Token 并创建账户
   */
  exchangeOAuthCode(request: ExchangeOAuthCodeRequest): Promise<AIAccountDto> {
    return post<AIAccountDto>('/openai/oauth/callback', request)
  },
}

/**
 * Gemini OAuth 服务
 */
export const geminiOAuthService = {
  /**
   * 生成 Gemini OAuth 授权链接
   */
  generateOAuthUrl(proxy?: any): Promise<GenerateOAuthUrlResponse> {
    return post<GenerateOAuthUrlResponse>('/gemini/oauth/authorize', {
      proxy: proxy || null
    })
  },

  /**
   * 交换授权码获取 Token 并创建账户
   */
  exchangeOAuthCode(request: ExchangeOAuthCodeRequest): Promise<AIAccountDto> {
    return post<AIAccountDto>('/gemini/oauth/callback', request)
  },
}
