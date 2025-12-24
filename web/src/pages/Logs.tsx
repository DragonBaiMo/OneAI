import { useState, useEffect } from 'react';
import { format, subDays } from 'date-fns';
import { RefreshCw, Eye } from 'lucide-react';
import { logsApi } from '@/services/logs';
import { accountService } from '@/services/account';
import type {
  AIRequestLogDto,
  AIRequestLogQueryRequest,
  PagedResponse,
  LogStatistics,
} from '@/types/logs';
import type { AIAccountDto } from '@/types/account';
import { Card } from '@/components/animate-ui/components/card';
import { Input } from '@/components/animate-ui/components/input';
import { Label } from '@/components/animate-ui/components/label';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/animate-ui/components/radix/dialog';

export default function Logs() {
  // 状态管理
  const [logs, setLogs] = useState<AIRequestLogDto[]>([]);
  const [accounts, setAccounts] = useState<AIAccountDto[]>([]);
  const [statistics, setStatistics] = useState<LogStatistics | null>(null);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({
    totalCount: 0,
    pageNumber: 1,
    pageSize: 50,
    totalPages: 0,
  });
  const [selectedLog, setSelectedLog] = useState<AIRequestLogDto | null>(null);
  const [detailDialogOpen, setDetailDialogOpen] = useState(false);

  // 查询参数
  const [filters, setFilters] = useState<AIRequestLogQueryRequest>({
    accountId: undefined,
    startTime: format(subDays(new Date(), 7), "yyyy-MM-dd'T'HH:mm:ss"),
    endTime: format(new Date(), "yyyy-MM-dd'T'HH:mm:ss"),
    pageNumber: 1,
    pageSize: 50,
  });

  // 加载账户列表
  useEffect(() => {
    const loadAccounts = async () => {
      try {
        const data = await accountService.getAccounts();
        setAccounts(data);
      } catch (error) {
        console.error('加载账户列表失败:', error);
      }
    };
    loadAccounts();
  }, []);

  // 加载日志和统计信息
  const loadLogs = async () => {
    setLoading(true);
    try {
      const [logsResponse, stats] = await Promise.all([
        logsApi.queryLogs(filters),
        logsApi.getStatistics({
          accountId: filters.accountId,
          startTime: filters.startTime,
          endTime: filters.endTime,
        }),
      ]);

      setLogs(logsResponse.items);
      setPagination({
        totalCount: logsResponse.totalCount,
        pageNumber: logsResponse.pageNumber,
        pageSize: logsResponse.pageSize,
        totalPages: logsResponse.totalPages,
      });
      setStatistics(stats);
    } catch (error) {
      console.error('加载日志失败:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadLogs();
  }, [filters]);

  // 处理筛选变化
  const handleFilterChange = (key: keyof AIRequestLogQueryRequest, value: any) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
      pageNumber: key === 'pageNumber' ? value : 1, // 修改筛选条件时重置到第一页
    }));
  };

  // 格式化时间
  const formatDateTime = (dateStr: string) => {
    return format(new Date(dateStr), 'yyyy-MM-dd HH:mm:ss');
  };

  // 格式化持续时间
  const formatDuration = (ms?: number) => {
    if (!ms) return '-';
    if (ms < 1000) return `${ms}ms`;
    return `${(ms / 1000).toFixed(2)}s`;
  };

  // 打开详情对话框
  const handleViewDetail = (log: AIRequestLogDto) => {
    setSelectedLog(log);
    setDetailDialogOpen(true);
  };

  return (
    <div className="p-6 space-y-6">
      {/* 标题 */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">请求日志</h1>
          <p className="text-muted-foreground mt-1">查看和分析 AI 模型请求日志</p>
        </div>
        <button
          onClick={() => loadLogs()}
          disabled={loading}
          className="flex items-center gap-2 px-4 py-2 rounded-md border border-input bg-background hover:bg-accent hover:text-accent-foreground disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
          <span>{loading ? '加载中...' : '刷新'}</span>
        </button>
      </div>

      {/* 统计卡片 */}
      {statistics && (
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          <Card className="p-4">
            <div className="text-sm text-muted-foreground">总请求数</div>
            <div className="text-2xl font-bold mt-1">{statistics.totalRequests}</div>
            <div className="text-xs text-muted-foreground mt-1">
              完成: {statistics.completedRequests}
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-muted-foreground">成功</div>
            <div className="text-2xl font-bold mt-1 text-green-600">
              {statistics.successRequests}
            </div>
            <div className="text-xs text-muted-foreground mt-1">
              成功率: {(statistics.successRate * 100).toFixed(1)}%
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-muted-foreground">失败</div>
            <div className="text-2xl font-bold mt-1 text-red-600">
              {statistics.failedRequests}
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-muted-foreground">进行中</div>
            <div className="text-2xl font-bold mt-1 text-blue-600">
              {statistics.inProgressRequests}
            </div>
          </Card>
          <Card className="p-4">
            <div className="text-sm text-muted-foreground">平均响应时间</div>
            <div className="text-2xl font-bold mt-1">
              {statistics.avgDurationMs
                ? `${(statistics.avgDurationMs / 1000).toFixed(2)}s`
                : '-'}
            </div>
            <div className="text-xs text-muted-foreground mt-1">
              Token: {statistics.totalTokens.toLocaleString()}
            </div>
          </Card>
        </div>
      )}

      {/* 筛选器 */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* 账户筛选 */}
          <div className="space-y-2">
            <Label>账户筛选</Label>
            <select
              className="w-full px-3 py-2 border border-input bg-background rounded-md text-sm"
              value={filters.accountId ?? ''}
              onChange={(e) =>
                handleFilterChange(
                  'accountId',
                  e.target.value ? Number(e.target.value) : undefined
                )
              }
            >
              <option value="">全部账户</option>
              {accounts.map((account) => (
                <option key={account.id} value={account.id}>
                  {account.name || account.email || `账户 ${account.id}`}
                </option>
              ))}
            </select>
          </div>

          {/* 开始时间 */}
          <div className="space-y-2">
            <Label>开始时间</Label>
            <Input
              type="datetime-local"
              value={filters.startTime?.slice(0, 16) ?? ''}
              onChange={(e) =>
                handleFilterChange('startTime', e.target.value + ':00')
              }
            />
          </div>

          {/* 结束时间 */}
          <div className="space-y-2">
            <Label>结束时间</Label>
            <Input
              type="datetime-local"
              value={filters.endTime?.slice(0, 16) ?? ''}
              onChange={(e) => handleFilterChange('endTime', e.target.value + ':00')}
            />
          </div>

          {/* 状态筛选 */}
          <div className="space-y-2">
            <Label>状态筛选</Label>
            <select
              className="w-full px-3 py-2 border border-input bg-background rounded-md text-sm"
              value={
                filters.isSuccess === undefined
                  ? ''
                  : filters.isSuccess
                    ? 'true'
                    : 'false'
              }
              onChange={(e) =>
                handleFilterChange(
                  'isSuccess',
                  e.target.value === '' ? undefined : e.target.value === 'true'
                )
              }
            >
              <option value="">全部</option>
              <option value="true">成功</option>
              <option value="false">失败</option>
            </select>
          </div>
        </div>
      </Card>

      {/* 日志表格 */}
      <Card className="p-0 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full table-fixed">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-3 py-3 text-left text-sm font-medium w-[140px]">时间</th>
                <th className="px-3 py-3 text-left text-sm font-medium w-[120px]">账户</th>
                <th className="px-3 py-3 text-left text-sm font-medium w-[140px]">模型</th>
                <th className="px-3 py-3 text-left text-sm font-medium w-[80px]">状态</th>
                <th className="px-3 py-3 text-left text-sm font-medium w-[100px]">Token</th>
                <th className="px-3 py-3 text-left text-sm font-medium w-[80px]">耗时</th>
                <th className="px-3 py-3 text-left text-sm font-medium">异常信息</th>
                <th className="px-3 py-3 text-left text-sm font-medium w-[80px]">操作</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {loading ? (
                <tr>
                  <td colSpan={8} className="px-3 py-8 text-center text-muted-foreground text-sm">
                    加载中...
                  </td>
                </tr>
              ) : logs.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-3 py-8 text-center text-muted-foreground text-sm">
                    暂无日志数据
                  </td>
                </tr>
              ) : (
                logs.map((log) => (
                  <tr key={log.id} className="hover:bg-muted/50">
                    <td className="px-3 py-3 text-xs whitespace-nowrap overflow-hidden text-ellipsis">
                      <div className="space-y-0.5">
                        <div>{format(new Date(log.requestStartTime), 'MM-dd')}</div>
                        <div className="text-muted-foreground">{format(new Date(log.requestStartTime), 'HH:mm:ss')}</div>
                      </div>
                    </td>
                    <td className="px-3 py-3 text-xs">
                      <div className="truncate font-medium" title={log.accountName || log.accountEmail || '-'}>
                        {log.accountName || log.accountEmail || '-'}
                      </div>
                      {log.provider && (
                        <div className="text-xs text-muted-foreground truncate">
                          {log.provider}
                        </div>
                      )}
                    </td>
                    <td className="px-3 py-3 text-xs">
                      <code className="text-xs bg-muted px-1.5 py-0.5 rounded block truncate" title={log.model}>
                        {log.model}
                      </code>
                    </td>
                    <td className="px-3 py-3 text-xs">
                      {!log.requestEndTime ? (
                        <span className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                          进行中
                        </span>
                      ) : log.isSuccess ? (
                        <span className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
                          成功
                        </span>
                      ) : (
                        <span className="inline-flex items-center px-1.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                          失败
                        </span>
                      )}
                    </td>
                    <td className="px-3 py-3 text-xs">
                      {log.totalTokens ? (
                        <div className="truncate" title={`总计: ${log.totalTokens.toLocaleString()} (${log.promptTokens} + ${log.completionTokens})`}>
                          {log.totalTokens.toLocaleString()}
                        </div>
                      ) : (
                        '-'
                      )}
                    </td>
                    <td className="px-3 py-3 text-xs whitespace-nowrap">
                      {formatDuration(log.durationMs)}
                    </td>
                    <td className="px-3 py-3 text-xs">
                      <div className="truncate text-red-600" title={log.errorMessage || ''}>
                        {log.errorMessage || '-'}
                      </div>
                    </td>
                    <td className="px-3 py-3 text-xs">
                      <button
                        onClick={() => handleViewDetail(log)}
                        className="inline-flex items-center gap-1 px-2 py-1 text-xs rounded hover:bg-muted transition-colors whitespace-nowrap"
                      >
                        <Eye className="h-3 w-3" />
                        详情
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* 分页 */}
        {pagination.totalPages > 1 && (
          <div className="flex items-center justify-between px-3 py-3 border-t">
            <div className="text-xs text-muted-foreground">
              共 {pagination.totalCount} 条，第 {pagination.pageNumber}/{pagination.totalPages} 页
            </div>
            <div className="flex gap-2">
              <button
                className="px-2.5 py-1 text-xs border rounded-md hover:bg-muted disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={pagination.pageNumber === 1}
                onClick={() => handleFilterChange('pageNumber', pagination.pageNumber - 1)}
              >
                上一页
              </button>
              <button
                className="px-2.5 py-1 text-xs border rounded-md hover:bg-muted disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={pagination.pageNumber === pagination.totalPages}
                onClick={() => handleFilterChange('pageNumber', pagination.pageNumber + 1)}
              >
                下一页
              </button>
            </div>
          </div>
        )}
      </Card>

      {/* 详情对话框 */}
      <Dialog open={detailDialogOpen} onOpenChange={setDetailDialogOpen}>
        <DialogContent className="max-w-3xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>日志详情</DialogTitle>
            <DialogDescription>
              请求 ID: {selectedLog?.requestId}
            </DialogDescription>
          </DialogHeader>

          {selectedLog && (
            <div className="space-y-4">
              {/* 基本信息 */}
              <div className="space-y-2">
                <h3 className="text-sm font-semibold">基本信息</h3>
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <span className="text-muted-foreground">请求 ID：</span>
                    <span className="font-mono text-xs">{selectedLog.requestId}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">状态：</span>
                    {!selectedLog.requestEndTime ? (
                      <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        进行中
                      </span>
                    ) : selectedLog.isSuccess ? (
                      <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                        成功
                      </span>
                    ) : (
                      <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                        失败
                      </span>
                    )}
                  </div>
                  <div>
                    <span className="text-muted-foreground">模型：</span>
                    <code className="ml-2 text-xs bg-muted px-1 py-0.5 rounded">
                      {selectedLog.model}
                    </code>
                  </div>
                  <div>
                    <span className="text-muted-foreground">流式传输：</span>
                    <span className="ml-2">{selectedLog.isStreaming ? '是' : '否'}</span>
                  </div>
                  {selectedLog.conversationId && (
                    <div className="col-span-2">
                      <span className="text-muted-foreground">会话 ID：</span>
                      <span className="ml-2 font-mono text-xs">{selectedLog.conversationId}</span>
                    </div>
                  )}
                </div>
              </div>

              {/* 账户信息 */}
              {selectedLog.accountId && (
                <div className="space-y-2">
                  <h3 className="text-sm font-semibold">账户信息</h3>
                  <div className="grid grid-cols-2 gap-2 text-sm">
                    <div>
                      <span className="text-muted-foreground">账户名：</span>
                      <span className="ml-2">{selectedLog.accountName || '-'}</span>
                    </div>
                    <div>
                      <span className="text-muted-foreground">邮箱：</span>
                      <span className="ml-2">{selectedLog.accountEmail || '-'}</span>
                    </div>
                    <div>
                      <span className="text-muted-foreground">提供商：</span>
                      <span className="ml-2">{selectedLog.provider || '-'}</span>
                    </div>
                  </div>
                </div>
              )}

              {/* 时间和性能 */}
              <div className="space-y-2">
                <h3 className="text-sm font-semibold">时间和性能</h3>
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <span className="text-muted-foreground">开始时间：</span>
                    <span className="ml-2">{formatDateTime(selectedLog.requestStartTime)}</span>
                  </div>
                  {selectedLog.requestEndTime && (
                    <div>
                      <span className="text-muted-foreground">结束时间：</span>
                      <span className="ml-2">{formatDateTime(selectedLog.requestEndTime)}</span>
                    </div>
                  )}
                  <div>
                    <span className="text-muted-foreground">总耗时：</span>
                    <span className="ml-2">{formatDuration(selectedLog.durationMs)}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">首字节时间：</span>
                    <span className="ml-2">{formatDuration(selectedLog.timeToFirstByteMs)}</span>
                  </div>
                </div>
              </div>

              {/* Token 使用 */}
              {selectedLog.totalTokens && (
                <div className="space-y-2">
                  <h3 className="text-sm font-semibold">Token 使用</h3>
                  <div className="grid grid-cols-3 gap-2 text-sm">
                    <div>
                      <span className="text-muted-foreground">提示 Token：</span>
                      <span className="ml-2">{selectedLog.promptTokens?.toLocaleString() || '-'}</span>
                    </div>
                    <div>
                      <span className="text-muted-foreground">完成 Token：</span>
                      <span className="ml-2">{selectedLog.completionTokens?.toLocaleString() || '-'}</span>
                    </div>
                    <div>
                      <span className="text-muted-foreground">总计：</span>
                      <span className="ml-2 font-semibold">{selectedLog.totalTokens.toLocaleString()}</span>
                    </div>
                  </div>
                </div>
              )}

              {/* 重试信息 */}
              {selectedLog.retryCount > 0 && (
                <div className="space-y-2">
                  <h3 className="text-sm font-semibold">重试信息</h3>
                  <div className="grid grid-cols-2 gap-2 text-sm">
                    <div>
                      <span className="text-muted-foreground">重试次数：</span>
                      <span className="ml-2">{selectedLog.retryCount}</span>
                    </div>
                    <div>
                      <span className="text-muted-foreground">总尝试次数：</span>
                      <span className="ml-2">{selectedLog.totalAttempts}</span>
                    </div>
                  </div>
                </div>
              )}

              {/* 错误信息 */}
              {selectedLog.errorMessage && (
                <div className="space-y-2">
                  <h3 className="text-sm font-semibold text-red-600">错误信息</h3>
                  <div className="bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 rounded-md p-3">
                    <pre className="text-sm text-red-900 dark:text-red-100 whitespace-pre-wrap break-words">
                      {selectedLog.errorMessage}
                    </pre>
                  </div>
                </div>
              )}

              {/* 消息摘要 */}
              {selectedLog.messageSummary && (
                <div className="space-y-2">
                  <h3 className="text-sm font-semibold">消息摘要</h3>
                  <div className="bg-muted rounded-md p-3">
                    <pre className="text-sm whitespace-pre-wrap break-words">
                      {selectedLog.messageSummary}
                    </pre>
                  </div>
                </div>
              )}

              {/* 响应摘要 */}
              {selectedLog.responseSummary && (
                <div className="space-y-2">
                  <h3 className="text-sm font-semibold">响应摘要</h3>
                  <div className="bg-muted rounded-md p-3">
                    <pre className="text-sm whitespace-pre-wrap break-words">
                      {selectedLog.responseSummary}
                    </pre>
                  </div>
                </div>
              )}

              {/* 其他信息 */}
              <div className="space-y-2">
                <h3 className="text-sm font-semibold">其他信息</h3>
                <div className="grid grid-cols-2 gap-2 text-sm">
                  {selectedLog.statusCode && (
                    <div>
                      <span className="text-muted-foreground">HTTP 状态码：</span>
                      <span className="ml-2">{selectedLog.statusCode}</span>
                    </div>
                  )}
                  {selectedLog.clientIp && (
                    <div>
                      <span className="text-muted-foreground">客户端 IP：</span>
                      <span className="ml-2">{selectedLog.clientIp}</span>
                    </div>
                  )}
                  <div>
                    <span className="text-muted-foreground">限流：</span>
                    <span className="ml-2">{selectedLog.isRateLimited ? '是' : '否'}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">会话粘性：</span>
                    <span className="ml-2">{selectedLog.sessionStickinessUsed ? '是' : '否'}</span>
                  </div>
                </div>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
