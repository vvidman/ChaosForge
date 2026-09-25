import { useEffect, useRef, type ReactNode } from 'react'
import { useLocation } from 'react-router-dom'

interface PageTransitionProps {
  children: ReactNode
}

const FADE_IN_MS = 150

// Fades the page in on every navigation. Driven by the Web Animations API on a ref,
// so no state is set inside the effect and children are not remounted.
export function PageTransition({ children }: PageTransitionProps) {
  const location = useLocation()
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const element = containerRef.current
    if (!element || typeof element.animate !== 'function') return
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)').matches) return

    const animation = element.animate([{ opacity: 0 }, { opacity: 1 }], {
      duration: FADE_IN_MS,
      easing: 'ease-out',
    })
    return () => animation.cancel()
  }, [location.key])

  return <div ref={containerRef}>{children}</div>
}
