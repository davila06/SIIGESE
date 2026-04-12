import { AfterViewInit, Directive, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';

@Directive({
  selector: '[appCountUp]',
  standalone: true
})
export class CountUpDirective implements AfterViewInit, OnChanges, OnDestroy {
  @Input('appCountUp') value: number | null | undefined = 0;
  @Input() countUpDuration = 600;
  @Input() countUpDecimals = 0;
  @Input() countUpPrefix = '';
  @Input() countUpSuffix = '';

  private animationFrameId: number | null = null;
  private currentValue = 0;
  private hasViewInitialized = false;

  constructor(private readonly elementRef: ElementRef<HTMLElement>) {}

  ngAfterViewInit(): void {
    this.hasViewInitialized = true;
    this.animateTo(this.toNumber(this.value), true);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.hasViewInitialized || !changes['value']) {
      return;
    }

    this.animateTo(this.toNumber(this.value), false);
  }

  ngOnDestroy(): void {
    this.cancelAnimation();
  }

  private animateTo(target: number, immediate: boolean): void {
    this.cancelAnimation();

    const prefersReducedMotion =
      typeof window !== 'undefined' &&
      window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

    if (immediate || prefersReducedMotion || this.countUpDuration <= 0) {
      this.currentValue = target;
      this.render(target);
      return;
    }

    const startValue = this.currentValue;
    const change = target - startValue;
    const startedAt = performance.now();
    const duration = this.countUpDuration;

    const tick = (now: number): void => {
      const elapsed = now - startedAt;
      const progress = Math.min(elapsed / duration, 1);

      // Ease out cubic for a smoother finish.
      const eased = 1 - Math.pow(1 - progress, 3);
      const frameValue = startValue + change * eased;
      this.currentValue = frameValue;
      this.render(frameValue);

      if (progress < 1) {
        this.animationFrameId = requestAnimationFrame(tick);
      } else {
        this.currentValue = target;
        this.render(target);
      }
    };

    this.animationFrameId = requestAnimationFrame(tick);
  }

  private render(value: number): void {
    const safeDecimals = Math.max(0, Math.min(6, this.countUpDecimals));
    const formatted = new Intl.NumberFormat('es-CR', {
      minimumFractionDigits: safeDecimals,
      maximumFractionDigits: safeDecimals
    }).format(value);

    this.elementRef.nativeElement.textContent = `${this.countUpPrefix}${formatted}${this.countUpSuffix}`;
  }

  private cancelAnimation(): void {
    if (this.animationFrameId !== null) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }
  }

  private toNumber(input: number | null | undefined): number {
    if (input === null || input === undefined) {
      return 0;
    }

    const parsed = Number(input);
    return Number.isFinite(parsed) ? parsed : 0;
  }
}