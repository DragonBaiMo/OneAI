import api from './api';
import type {
  AIRequestLogQueryRequest,
  AIRequestLogDto,
  PagedResponse,
  LogStatistics,
} from '@/types/logs';

export const logsApi = {
  // 查询日志列表
  queryLogs: async (
    request: AIRequestLogQueryRequest
  ): Promise<PagedResponse<AIRequestLogDto>> => {
    return api.post<PagedResponse<AIRequestLogDto>>('/logs/query', request);
  },

  // 获取日志统计信息
  getStatistics: async (params?: {
    accountId?: number;
    startTime?: string;
    endTime?: string;
  }): Promise<LogStatistics> => {
    return api.get<LogStatistics>('/logs/statistics', params);
  },
};
