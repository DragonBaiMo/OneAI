// AI请求日志查询请求
export interface AIRequestLogQueryRequest {
  accountId?: number;
  startTime?: string;
  endTime?: string;
  model?: string;
  isSuccess?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

// AI请求日志响应
export interface AIRequestLogDto {
  id: number;
  requestId: string;
  conversationId?: string;
  sessionId?: string;

  // 账户信息
  accountId?: number;
  accountName?: string;
  accountEmail?: string;
  provider?: string;

  // 请求信息
  model: string;
  isStreaming: boolean;
  messageSummary?: string;

  // 响应信息
  statusCode?: number;
  isSuccess: boolean;
  errorMessage?: string;
  retryCount: number;
  totalAttempts: number;
  responseSummary?: string;

  // Token使用
  promptTokens?: number;
  completionTokens?: number;
  totalTokens?: number;

  // 性能指标
  requestStartTime: string;
  requestEndTime?: string;
  durationMs?: number;
  timeToFirstByteMs?: number;

  // 配额和限流
  isRateLimited: boolean;
  rateLimitResetSeconds?: number;
  sessionStickinessUsed: boolean;

  // 其他元数据
  clientIp?: string;
  createdAt: string;
}

// 分页响应
export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// 日志统计信息
export interface LogStatistics {
  totalRequests: number;
  completedRequests: number;
  successRequests: number;
  failedRequests: number;
  inProgressRequests: number;
  successRate: number;
  totalTokens: number;
  avgDurationMs: number;
  modelStats: Array<{
    model: string;
    count: number;
    totalTokens: number;
  }>;
  timeRange: {
    start: string;
    end: string;
  };
}
