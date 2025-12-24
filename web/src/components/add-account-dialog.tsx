import { useState } from 'react'
import { motion } from 'motion/react'
import { AlertCircle, Loader, ExternalLink, Copy, Check } from 'lucide-react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/animate-ui/components/radix/dialog'
import { Button } from '@/components/animate-ui/components/buttons/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/animate-ui/components/card'
import { openaiOAuthService } from '@/services/account'
import type { AccountType, GenerateOAuthUrlResponse, AIAccountDto } from '@/types/account'

interface AddAccountDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onAccountAdded?: (account: AIAccountDto) => void
}

export function AddAccountDialog({ open, onOpenChange, onAccountAdded }: AddAccountDialogProps) {
  const [accountType, setAccountType] = useState<AccountType>('openai')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [authUrl, setAuthUrl] = useState<string | null>(null)
  const [sessionId, setSessionId] = useState<string | null>(null)
  const [authCode, setAuthCode] = useState('')
  const [processingCode, setProcessingCode] = useState(false)
  const [copied, setCopied] = useState(false)

  const handleStartOAuth = async () => {
    try {
      setLoading(true)
      setError(null)
      const response = await openaiOAuthService.generateOAuthUrl()
      setAuthUrl(response.authUrl)
      setSessionId(response.sessionId)

      // 自动打开授权链接到新标签页
      window.open(response.authUrl, '_blank')
    } catch (err) {
      const message = err instanceof Error ? err.message : '生成授权链接失败'
      setError(message)
      console.error('Failed to generate auth URL:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleExchangeCode = async () => {
    if (!authCode.trim() || !sessionId) {
      setError('请输入授权码')
      return
    }

    try {
      setProcessingCode(true)
      setError(null)
      const account = await openaiOAuthService.exchangeOAuthCode({
        sessionId,
        authorizationCode: authCode,
      })

      onAccountAdded?.(account)
      resetForm()
      onOpenChange(false)
    } catch (err) {
      const message = err instanceof Error ? err.message : '处理授权码失败'
      setError(message)
      console.error('Failed to exchange code:', err)
    } finally {
      setProcessingCode(false)
    }
  }

  const handleCopyAuthUrl = () => {
    if (authUrl) {
      navigator.clipboard.writeText(authUrl)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  const resetForm = () => {
    setAuthUrl(null)
    setSessionId(null)
    setAuthCode('')
    setError(null)
    setCopied(false)
  }

  const handleOpenChange = (newOpen: boolean) => {
    if (!newOpen) {
      resetForm()
    }
    onOpenChange(newOpen)
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="max-w-4xl w-full">
        <DialogHeader>
          <DialogTitle>添加 AI 账户</DialogTitle>
          <DialogDescription>
            选择账户类型并按照步骤进行授权
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Account Type Tabs */}
          <div className="flex gap-2 border-b">
            <button
              onClick={() => {
                setAccountType('openai')
                resetForm()
              }}
              className={`px-4 py-2 font-medium border-b-2 transition-colors ${
                accountType === 'openai'
                  ? 'border-primary text-primary'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              OpenAI
            </button>
            <button
              disabled
              className="px-4 py-2 font-medium text-muted-foreground opacity-50 cursor-not-allowed"
            >
              Claude (敬请期待)
            </button>
            <button
              disabled
              className="px-4 py-2 font-medium text-muted-foreground opacity-50 cursor-not-allowed"
            >
              Gemini (敬请期待)
            </button>
          </div>

          {/* Error Message */}
          {error && (
            <motion.div
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              className="flex gap-3 rounded-lg border border-red-200 bg-red-50 p-4 text-red-800 dark:border-red-800 dark:bg-red-950 dark:text-red-200"
            >
              <AlertCircle className="h-5 w-5 mt-0.5 flex-shrink-0" />
              <p className="text-sm">{error}</p>
            </motion.div>
          )}

          {/* OpenAI OAuth Flow */}
          {accountType === 'openai' && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="space-y-4"
            >
              {!authUrl ? (
                <Card>
                  <CardHeader>
                    <CardTitle className="text-base">OpenAI OAuth 授权</CardTitle>
                    <CardDescription>
                      使用 OpenAI 官方账户安全授权
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    <p className="text-sm text-muted-foreground">
                      点击下方按钮，我们将引导您到 OpenAI 官网进行安全授权。授权后，您将获得一个授权码，请复制该授权码并粘贴到下方。
                    </p>
                    <Button
                      onClick={handleStartOAuth}
                      disabled={loading}
                      className="w-full gap-2"
                    >
                      {loading ? (
                        <>
                          <Loader className="h-4 w-4 animate-spin" />
                          生成授权链接中...
                        </>
                      ) : (
                        <>
                          <ExternalLink className="h-4 w-4" />
                          前往 OpenAI 授权
                        </>
                      )}
                    </Button>
                  </CardContent>
                </Card>
              ) : (
                <div className="space-y-4">
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-base">步骤 1: 复制授权链接</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-3">
                      <p className="text-sm text-muted-foreground">
                        下方是您的授权链接，您可以复制后在浏览器中打开，或点击"打开链接"按钮直接打开。
                      </p>
                      <div className="flex flex-col gap-2">
                        <div className="rounded-md border border-input bg-background px-3 py-2 text-sm max-h-24 overflow-y-auto break-all">
                          <span className="text-muted-foreground text-xs leading-relaxed">{authUrl}</span>
                        </div>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={handleCopyAuthUrl}
                          className="gap-2 w-full"
                        >
                          {copied ? (
                            <>
                              <Check className="h-4 w-4" />
                              已复制
                            </>
                          ) : (
                            <>
                              <Copy className="h-4 w-4" />
                              复制链接
                            </>
                          )}
                        </Button>
                      </div>
                      <Button
                        variant="outline"
                        onClick={() => window.open(authUrl, '_blank')}
                        className="w-full gap-2"
                      >
                        <ExternalLink className="h-4 w-4" />
                        打开授权链接
                      </Button>
                    </CardContent>
                  </Card>

                  <Card>
                    <CardHeader>
                      <CardTitle className="text-base">步骤 2: 获取授权码</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-3">
                      <p className="text-sm text-muted-foreground">
                        在浏览器中完成授权后，系统会提示您一个授权码。请复制该授权码并粘贴到下方输入框。
                      </p>
                      <div className="space-y-2">
                        <label className="text-sm font-medium">授权码</label>
                        <input
                          type="text"
                          placeholder="粘贴您的授权码..."
                          value={authCode}
                          onChange={(e) => setAuthCode(e.target.value)}
                          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary"
                        />
                      </div>
                    </CardContent>
                  </Card>
                </div>
              )}
            </motion.div>
          )}

          {/* Action Buttons */}
          <div className="flex gap-3 justify-end">
            <Button
              variant="outline"
              onClick={() => handleOpenChange(false)}
              disabled={loading || processingCode}
            >
              取消
            </Button>
            {authUrl && (
              <Button
                onClick={handleExchangeCode}
                disabled={!authCode.trim() || processingCode}
                className="gap-2"
              >
                {processingCode ? (
                  <>
                    <Loader className="h-4 w-4 animate-spin" />
                    处理中...
                  </>
                ) : (
                  '完成授权'
                )}
              </Button>
            )}
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
