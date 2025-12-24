'use client';

import * as React from 'react';
import * as LabelPrimitive from '@radix-ui/react-label';
import { motion } from 'motion/react';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const labelVariants = cva('text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70', {
  variants: {
    variant: {
      default: 'text-foreground',
      muted: 'text-muted-foreground',
      required: 'text-foreground',
    },
  },
  defaultVariants: {
    variant: 'default',
  },
});

export interface LabelProps
  extends React.ComponentPropsWithoutRef<typeof LabelPrimitive.Root>,
    VariantProps<typeof labelVariants> {
  animated?: boolean;
  required?: boolean;
  error?: boolean;
}

const Label = React.forwardRef<
  React.ElementRef<typeof LabelPrimitive.Root>,
  LabelProps
>(
  (
    { className, variant, animated = true, required = false, error = false, children, ...props },
    ref
  ) => {
    const errorVariant = error ? 'text-destructive' : undefined;

    return (
      <motion.div
        initial={animated ? { opacity: 0 } : {}}
        animate={animated ? { opacity: 1 } : {}}
        transition={{ type: 'spring', stiffness: 300, damping: 30, delay: 0.05 }}
      >
        <LabelPrimitive.Root
          ref={ref}
          className={cn(
            labelVariants({ variant }),
            errorVariant,
            className
          )}
          {...props}
        >
          <div className="flex items-center gap-1">
            <span>{children}</span>
            {required && (
              <motion.span
                initial={animated ? { scale: 0, opacity: 0 } : {}}
                animate={animated ? { scale: 1, opacity: 1 } : {}}
                transition={{
                  type: 'spring',
                  stiffness: 400,
                  damping: 30,
                  delay: 0.1,
                }}
                className="text-destructive"
              >
                *
              </motion.span>
            )}
          </div>
        </LabelPrimitive.Root>
      </motion.div>
    );
  }
);
Label.displayName = LabelPrimitive.Root.displayName;

export { Label, labelVariants };
