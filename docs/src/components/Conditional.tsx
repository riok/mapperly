import { PropsWithChildren, ReactNode } from 'react';

// can be used to render conditional blocks in markdown
export default function Conditional({
  condition,
  children,
}: PropsWithChildren<{ condition: boolean }>): ReactNode {
  return condition ? children : null;
}
