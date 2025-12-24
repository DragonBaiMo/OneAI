import { useState, useEffect } from 'react'
import { motion } from 'motion/react'
import { useLocation } from 'react-router-dom'
import { Lock, Code2, Shield, Zap, ArrowRight, Trash2, AlertCircle, Loader, Search, Plus, MoreVertical, CheckCircle, XCircle } from 'lucide-react'
import {
  Button as AnimateUIButton,
} from '@/components/animate-ui/components/buttons/button'
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from '@/components/animate-ui/components/card'
import { Input } from '@/components/animate-ui/components/input'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/animate-ui/components/radix/dialog'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/animate-ui/components/radix/dropdown-menu'
import { SidebarLayout } from '@/components/sidebar-layout'
import { AddAccountDialog } from '@/components/add-account-dialog'
import { SettingsView as SettingsViewComponent } from '@/components/settings-view'
import { accountService } from '@/services/account'
import type { AIAccountDto, AccountQuotaStatus } from '@/types/account'
import Logs from './Logs'

const features = [
  { icon: Lock, text: '完整的认证系统', description: '安全的令牌管理' },
  { icon: Code2, text: '统一的 API 封装', description: '简化的 HTTP 客户端' },
  { icon: Zap, text: 'animate-ui 组件库', description: '现代化的动画组件' },
  { icon: Shield, text: 'TypeScript 类型安全', description: '完全的类型检查' },
]

const stats = [
  { label: '已完成请求', value: '1,234', icon: Zap },
  { label: '系统运行时间', value: '99.9%', icon: Shield },
  { label: '用户数', value: '42', icon: Lock },
]

export default function Home() {
  const location = useLocation()

  const getActiveMenu = () => {
    const path = location.pathname
    if (path.includes('/accounts')) return 'account'
    if (path.includes('/logs')) return 'logs'
    if (path.includes('/settings')) return 'settings'
    return 'home'
  }

  const renderContent = () => {
    const activeMenu = getActiveMenu()
    switch (activeMenu) {
      case 'account':
        return <AccountManagementView />
      case 'logs':
        return <Logs />
      case 'settings':
        return <SettingsViewComponent />
      default:
        return <HomeView features={features} stats={stats} />
    }
  }

  return (
    <SidebarLayout>
      {renderContent()}
    </SidebarLayout>
  )
}

function HomeView({ features, stats }: { features: any[], stats: any[] }) {
  return (
    <div className="p-6 space-y-6">
      {/* Welcome Section */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="space-y-2"
      >
        <h2 className="text-3xl font-bold">欢迎使用 OneAI</h2>
        <p className="text-muted-foreground">
          这是一个基于 React + TypeScript + animate-ui 构建的现代化前端应用
        </p>
      </motion.div>

      {/* Stats Grid */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ staggerChildren: 0.1, delayChildren: 0.2 }}
        className="grid gap-4 md:grid-cols-3"
      >
        {stats.map((stat, index) => (
          <motion.div
            key={index}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 * index, duration: 0.5 }}
          >
            <Card variant="elevated">
              <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-muted-foreground">{stat.label}</p>
                    <p className="text-2xl font-bold mt-2">{stat.value}</p>
                  </div>
                  <div className="text-primary opacity-20">
                    <stat.icon className="h-8 w-8" />
                  </div>
                </div>
              </CardContent>
            </Card>
          </motion.div>
        ))}
      </motion.div>

      {/* Features Grid */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ staggerChildren: 0.1, delayChildren: 0.4 }}
      >
        <h3 className="text-xl font-semibold mb-4">功能特性</h3>
        <div className="grid gap-4 md:grid-cols-2">
          {features.map((feature, index) => (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 * index, duration: 0.5 }}
            >
              <Card variant="elevated" hoverable>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-3">
                      <div className="p-2 rounded-lg bg-primary/10">
                        <feature.icon className="h-5 w-5 text-primary" />
                      </div>
                      <div>
                        <CardTitle className="text-base">{feature.text}</CardTitle>
                        <CardDescription className="text-xs">
                          {feature.description}
                        </CardDescription>
                      </div>
                    </div>
                  </div>
                </CardHeader>
              </Card>
            </motion.div>
          ))}
        </div>
      </motion.div>

      {/* Quick Start */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, delay: 0.6 }}
      >
        <Card variant="elevated">
          <CardHeader>
            <CardTitle>快速开始</CardTitle>
            <CardDescription>开始探索 OneAI 的强大功能</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col sm:flex-row gap-4">
              <AnimateUIButton variant="default" className="group">
                查看文档
                <ArrowRight className="h-4 w-4 ml-2 group-hover:translate-x-1 transition-transform" />
              </AnimateUIButton>
              <AnimateUIButton variant="outline">
                查看示例
              </AnimateUIButton>
            </div>
          </CardContent>
        </Card>
      </motion.div>
    </div>
  )
}

function AccountManagementView() {
  const [accounts, setAccounts] = useState<AIAccountDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [deletingId, setDeletingId] = useState<number | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [addDialogOpen, setAddDialogOpen] = useState(false)
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false)
  const [deleteConfirmAccountId, setDeleteConfirmAccountId] = useState<number | null>(null)
  const [togglingId, setTogglingId] = useState<number | null>(null)
  const [quotaStatuses, setQuotaStatuses] = useState<Record<number, AccountQuotaStatus>>({})

  useEffect(() => {
    fetchAccounts()
  }, [])

  const fetchAccounts = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await accountService.getAccounts()
      setAccounts(data)

      // 获取配额状态
      if (data.length > 0) {
        try {
          const accountIds = data.map(account => account.id)
          const quotas = await accountService.getAccountQuotaStatuses(accountIds)
          setQuotaStatuses(quotas)
        } catch (quotaErr) {
          console.error('Failed to fetch quota statuses:', quotaErr)
          // 配额获取失败不影响主流程
        }
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : '获取账户列表失败'
      setError(message)
      console.error('Failed to fetch accounts:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleDeleteClick = (id: number) => {
    setDeleteConfirmAccountId(id)
    setDeleteConfirmOpen(true)
  }

  const handleDeleteConfirm = async () => {
    if (!deleteConfirmAccountId) return

    try {
      setDeletingId(deleteConfirmAccountId)
      await accountService.deleteAccount(deleteConfirmAccountId)
      setAccounts(accounts.filter(account => account.id !== deleteConfirmAccountId))
      setDeleteConfirmOpen(false)
      setDeleteConfirmAccountId(null)
    } catch (err) {
      const message = err instanceof Error ? err.message : '删除账户失败'
      setError(message)
      console.error('Failed to delete account:', err)
    } finally {
      setDeletingId(null)
    }
  }

  const handleAccountAdded = async (account: AIAccountDto) => {
    // 刷新账户列表
    await fetchAccounts()
  }

  const handleToggleStatus = async (id: number) => {
    try {
      setTogglingId(id)
      const updatedAccount = await accountService.toggleAccountStatus(id)
      // 更新本地账户列表
      setAccounts(accounts.map(account =>
        account.id === id ? updatedAccount : account
      ))
    } catch (err) {
      const message = err instanceof Error ? err.message : '更新状态失败'
      setError(message)
      console.error('Failed to toggle account status:', err)
    } finally {
      setTogglingId(null)
    }
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  const formatTimeRemaining = (seconds?: number) => {
    if (!seconds || seconds <= 0) return '-'

    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)

    if (hours > 24) {
      const days = Math.floor(hours / 24)
      const remainingHours = hours % 24
      return `${days}天${remainingHours}小时`
    }

    if (hours > 0) {
      return `${hours}小时${minutes}分钟`
    }

    return `${minutes}分钟`
  }

  const filteredAccounts = accounts.filter(account =>
    account.provider.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (account.name && account.name.toLowerCase().includes(searchTerm.toLowerCase()))
  )

  return (
    <div className="p-6 space-y-6">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="space-y-4"
      >
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-3xl font-bold">AI 账户管理</h2>
            <p className="text-muted-foreground mt-1">
              管理你的 AI 服务提供商账户，包括 OpenAI、Claude、Gemini 等
            </p>
          </div>
        </div>

        {/* Search and Action Bar */}
        <div className="flex gap-3 items-center">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground pointer-events-none" />
            <Input
              placeholder="搜索账户名称或提供商..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
          <AnimateUIButton
            variant="default"
            className="gap-2"
            onClick={() => setAddDialogOpen(true)}
          >
            <Plus className="h-4 w-4" />
            添加账户
          </AnimateUIButton>
        </div>
      </motion.div>

      {error && (
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex gap-3 rounded-lg border border-red-200 bg-red-50 p-4 text-red-800 dark:border-red-800 dark:bg-red-950 dark:text-red-200"
        >
          <AlertCircle className="h-5 w-5 mt-0.5 flex-shrink-0" />
          <div>
            <p className="font-medium">出错了</p>
            <p className="text-sm">{error}</p>
          </div>
        </motion.div>
      )}

      {loading ? (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="flex items-center justify-center py-12"
        >
          <Loader className="h-6 w-6 animate-spin text-primary" />
          <span className="ml-2 text-muted-foreground">加载中...</span>
        </motion.div>
      ) : filteredAccounts.length === 0 && accounts.length === 0 ? (
        <Card variant="elevated">
          <CardContent className="pt-12 pb-12 text-center">
            <p className="text-muted-foreground">还没有添加任何账户</p>
          </CardContent>
        </Card>
      ) : filteredAccounts.length === 0 ? (
        <Card variant="elevated">
          <CardContent className="pt-12 pb-12 text-center">
            <p className="text-muted-foreground">未找到匹配的账户</p>
          </CardContent>
        </Card>
      ) : (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ staggerChildren: 0.05, delayChildren: 0.1 }}
        >
          <Card variant="elevated">
            <CardHeader>
              <CardTitle>账户列表</CardTitle>
              <CardDescription>
                共 {filteredAccounts.length} {searchTerm ? '个匹配的' : '个'}账户
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="px-4 py-3 text-left font-medium">提供商</th>
                      <th className="px-4 py-3 text-left font-medium">账户名称</th>
                      <th className="px-4 py-3 text-left font-medium">邮箱</th>
                      <th className="px-4 py-3 text-left font-medium">状态</th>
                      <th className="px-4 py-3 text-left font-medium">健康度</th>
                      <th className="px-4 py-3 text-left font-medium">5小时窗口</th>
                      <th className="px-4 py-3 text-left font-medium">7天窗口</th>
                      <th className="px-4 py-3 text-left font-medium">使用次数</th>
                      <th className="px-4 py-3 text-left font-medium">创建时间</th>
                      <th className="px-4 py-3 text-right font-medium">操作</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredAccounts.map((account, index) => (
                      <motion.tr
                        key={account.id}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: index * 0.05 }}
                        className="border-b"
                      >
                        <td className="px-4 py-3">
                          <span className="inline-flex items-center rounded-full bg-primary/10 px-3 py-1 text-xs font-medium text-primary">
                            {account.provider}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          {account.name || '-'}
                        </td>
                        <td className="px-4 py-3 text-xs text-muted-foreground">
                          {account.email || '-'}
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center gap-2">
                            <div className={`h-2 w-2 rounded-full ${
                              account.isEnabled
                                ? 'bg-green-500'
                                : 'bg-gray-400'
                            }`} />
                            <span>
                              {account.isEnabled ? '启用' : '禁用'}
                              {account.isRateLimited && ' (限流中)'}
                            </span>
                          </div>
                        </td>
                        <td className="px-4 py-3">
                          {quotaStatuses[account.id]?.hasCacheData ? (
                            <div className="flex items-center gap-2">
                              <div className={`h-2 w-2 rounded-full ${
                                (quotaStatuses[account.id].healthScore ?? 0) >= 80
                                  ? 'bg-green-500'
                                  : (quotaStatuses[account.id].healthScore ?? 0) >= 50
                                  ? 'bg-yellow-500'
                                  : 'bg-red-500'
                              }`} />
                              <span>{quotaStatuses[account.id].healthScore ?? 0}</span>
                            </div>
                          ) : (
                            <span className="text-muted-foreground">-</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          {quotaStatuses[account.id]?.hasCacheData ? (
                            <div className="space-y-1 min-w-[120px]">
                              <div className="flex items-center justify-between text-xs">
                                <span className="text-muted-foreground">{quotaStatuses[account.id].primaryUsedPercent ?? 0}%</span>
                              </div>
                              <div className="h-2 w-full bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                                <div
                                  className={`h-full transition-all ${
                                    (quotaStatuses[account.id].primaryUsedPercent ?? 0) >= 90
                                      ? 'bg-red-500'
                                      : (quotaStatuses[account.id].primaryUsedPercent ?? 0) >= 70
                                      ? 'bg-yellow-500'
                                      : 'bg-green-500'
                                  }`}
                                  style={{ width: `${Math.min(quotaStatuses[account.id].primaryUsedPercent ?? 0, 100)}%` }}
                                />
                              </div>
                              <div className="text-xs text-muted-foreground">
                                {formatTimeRemaining(quotaStatuses[account.id].primaryResetAfterSeconds)}后重置
                              </div>
                            </div>
                          ) : (
                            <span className="text-muted-foreground">-</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          {quotaStatuses[account.id]?.hasCacheData ? (
                            <div className="space-y-1 min-w-[120px]">
                              <div className="flex items-center justify-between text-xs">
                                <span className="text-muted-foreground">{quotaStatuses[account.id].secondaryUsedPercent ?? 0}%</span>
                              </div>
                              <div className="h-2 w-full bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                                <div
                                  className={`h-full transition-all ${
                                    (quotaStatuses[account.id].secondaryUsedPercent ?? 0) >= 90
                                      ? 'bg-red-500'
                                      : (quotaStatuses[account.id].secondaryUsedPercent ?? 0) >= 70
                                      ? 'bg-yellow-500'
                                      : 'bg-green-500'
                                  }`}
                                  style={{ width: `${Math.min(quotaStatuses[account.id].secondaryUsedPercent ?? 0, 100)}%` }}
                                />
                              </div>
                              <div className="text-xs text-muted-foreground">
                                {formatTimeRemaining(quotaStatuses[account.id].secondaryResetAfterSeconds)}后重置
                              </div>
                            </div>
                          ) : (
                            <span className="text-muted-foreground">-</span>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          {account.usageCount}
                        </td>
                        <td className="px-4 py-3 text-xs text-muted-foreground">
                          {formatDate(account.createdAt)}
                        </td>
                        <td className="px-4 py-3 text-right">
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <AnimateUIButton
                                variant="ghost"
                                size="sm"
                                className="h-8 w-8 p-0"
                              >
                                <MoreVertical className="h-4 w-4" />
                              </AnimateUIButton>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end" className="w-48">
                              <DropdownMenuItem
                                onClick={() => handleToggleStatus(account.id)}
                                disabled={togglingId === account.id}
                                className="cursor-pointer gap-2 flex"
                              >
                                {togglingId === account.id ? (
                                  <>
                                    <Loader className="h-4 w-4 animate-spin" />
                                    <span>更新中</span>
                                  </>
                                ) : account.isEnabled ? (
                                  <>
                                    <XCircle className="h-4 w-4" />
                                    <span>禁用</span>
                                  </>
                                ) : (
                                  <>
                                    <CheckCircle className="h-4 w-4" />
                                    <span>启用</span>
                                  </>
                                )}
                              </DropdownMenuItem>
                              <DropdownMenuItem
                                onClick={() => handleDeleteClick(account.id)}
                                disabled={deletingId === account.id}
                                className="cursor-pointer gap-2 flex text-red-600 hover:text-red-700"
                              >
                                {deletingId === account.id ? (
                                  <>
                                    <Loader className="h-4 w-4 animate-spin" />
                                    <span>删除中</span>
                                  </>
                                ) : (
                                  <>
                                    <Trash2 className="h-4 w-4" />
                                    <span>删除</span>
                                  </>
                                )}
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </td>
                      </motion.tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </motion.div>
      )}

      {/* Add Account Dialog */}
      <AddAccountDialog
        open={addDialogOpen}
        onOpenChange={setAddDialogOpen}
        onAccountAdded={handleAccountAdded}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirmOpen} onOpenChange={setDeleteConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>删除账户</DialogTitle>
            <DialogDescription>
              确定要删除这个账户吗？此操作不可撤销。
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="flex gap-3 justify-end">
            <AnimateUIButton
              variant="outline"
              onClick={() => setDeleteConfirmOpen(false)}
              disabled={deletingId === deleteConfirmAccountId}
            >
              取消
            </AnimateUIButton>
            <AnimateUIButton
              variant="destructive"
              onClick={handleDeleteConfirm}
              disabled={deletingId === deleteConfirmAccountId}
              className="gap-2"
            >
              {deletingId === deleteConfirmAccountId ? (
                <>
                  <Loader className="h-4 w-4 animate-spin" />
                  删除中
                </>
              ) : (
                '删除'
              )}
            </AnimateUIButton>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}


