'use client';

import { useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { LogOut, Sun, Moon } from 'lucide-react';
import { useTheme } from 'next-themes';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/animate-ui/components/radix/popover';
import {
  Button as AnimateUIButton,
} from '@/components/animate-ui/components/buttons/button';
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/animate-ui/components/animate/tooltip';
import {
  Switch,
} from '@/components/animate-ui/components/radix/switch';

export interface UserMenuProps {
  userName?: string;
  onLogout?: () => void;
}

export function UserMenu({ userName = 'User', onLogout }: UserMenuProps) {
  const { theme, setTheme } = useTheme();
  const [isOpen, setIsOpen] = useState(false);
  const [mounted, setMounted] = useState(false);

  // Prevent hydration mismatch
  useState(() => {
    setMounted(true);
  });

  if (!mounted) {
    return null;
  }

  const isDark = theme === 'dark';
  const userInitial = userName.charAt(0).toUpperCase();

  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        <motion.button
          className="flex items-center gap-3 rounded-lg p-2 hover:bg-accent transition-colors w-full text-left"
          whileHover={{ scale: 1.02 }}
          whileTap={{ scale: 0.98 }}
        >
          {/* User Avatar */}
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground font-semibold text-sm flex-shrink-0">
            {userInitial}
          </div>

          {/* User Info */}
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium truncate">{userName}</p>
            <p className="text-xs text-muted-foreground truncate">
              {isDark ? '暗色模式' : '亮色模式'}
            </p>
          </div>
        </motion.button>
      </PopoverTrigger>

      <PopoverContent className="w-56 p-0" align="end" sideOffset={8}>
        <AnimatePresence>
          {isOpen && (
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              transition={{ duration: 0.2 }}
              className="overflow-hidden"
            >
              {/* Menu Header */}
              <div className="border-b px-4 py-3">
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary font-semibold text-sm">
                    {userInitial}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold truncate">{userName}</p>
                    <p className="text-xs text-muted-foreground">账户</p>
                  </div>
                </div>
              </div>

              {/* Menu Items */}
              <div className="p-2 space-y-2">
                {/* Theme Toggle */}
                <Tooltip>
                  <TooltipTrigger asChild>
                    <motion.div
                      initial={{ opacity: 0, x: -10 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: 0.05 }}
                      className="flex items-center justify-between px-3 py-2 rounded-md hover:bg-accent cursor-pointer transition-colors"
                      onClick={() => setTheme(isDark ? 'light' : 'dark')}
                    >
                      <div className="flex items-center gap-2">
                        <motion.div
                          animate={{ rotate: isDark ? 180 : 0 }}
                          transition={{ duration: 0.3 }}
                        >
                          {isDark ? (
                            <Moon className="h-4 w-4 text-muted-foreground" />
                          ) : (
                            <Sun className="h-4 w-4 text-muted-foreground" />
                          )}
                        </motion.div>
                        <span className="text-sm">主题</span>
                      </div>
                      <motion.div
                        animate={{ scale: isDark ? 1.1 : 0.9 }}
                        transition={{ duration: 0.2 }}
                      >
                        <div className="text-xs text-muted-foreground">
                          {isDark ? '暗' : '亮'}
                        </div>
                      </motion.div>
                    </motion.div>
                  </TooltipTrigger>
                  <TooltipContent>
                    <p>切换到{isDark ? '亮色' : '暗色'}模式</p>
                  </TooltipContent>
                </Tooltip>

                {/* Divider */}
                <div className="my-1 h-px bg-border" />

                {/* Logout Button */}
                <motion.div
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.1 }}
                >
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <AnimateUIButton
                        variant="ghost"
                        className="w-full justify-start text-destructive hover:text-destructive hover:bg-destructive/10"
                        onClick={() => {
                          setIsOpen(false);
                          onLogout?.();
                        }}
                      >
                        <LogOut className="h-4 w-4 mr-2" />
                        <span>退出登录</span>
                      </AnimateUIButton>
                    </TooltipTrigger>
                    <TooltipContent>
                      <p>登出账户并返回登录页面</p>
                    </TooltipContent>
                  </Tooltip>
                </motion.div>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </PopoverContent>
    </Popover>
  );
}
