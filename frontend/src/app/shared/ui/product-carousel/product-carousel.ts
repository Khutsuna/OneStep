import {
  Component, DestroyRef, ElementRef, afterNextRender, computed, inject, input, signal, viewChild,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { I18N } from '../../../core/i18n/translate.pipes';
import { ProductListItem } from '../../../core/models/catalog.models';
import { ProductCard } from '../product-card/product-card';

/** How fast the row glides, in pixels per second. */
const SPEED = 45;
/** How long an arrow click takes to slide, in milliseconds. */
const ARROW_MS = 500;

/**
 * A row of product cards that glides continuously to the left, like a ticker.
 *
 * How the endless loop works: the products are shown twice in a row (A B C A B C).
 * The row moves left with a CSS transform; when it has moved exactly one full copy,
 * we jump back by that width. The second copy is then where the first one was, so the jump is invisible.
 *
 * - Stops while the mouse is over it (or a card has keyboard focus), and has a pause button.
 * - Never moves by itself for people who turned on "reduce motion" in their system settings.
 * - The arrows slide one screen of cards, smoothly, in either direction.
 */
@Component({
  selector: 'app-product-carousel',
  imports: [I18N, MatIconModule, ProductCard],
  template: `
    <section
      class="carousel"
      aria-roledescription="carousel"
      [attr.aria-label]="label()"
      (mouseenter)="hovering.set(true)"
      (mouseleave)="hovering.set(false)"
      (focusin)="hovering.set(true)"
      (focusout)="hovering.set(false)"
    >
      <div #viewport class="viewport">
        <ul #track class="track">
          @for (product of loop(); track $index; let i = $index) {
            <!-- The second copy is only for the loop: screen readers and the Tab key skip it (inert). -->
            <li class="slide" [attr.inert]="i >= products().length ? '' : null"
                [attr.aria-hidden]="i >= products().length ? 'true' : null">
              <app-product-card [product]="product" />
            </li>
          }
        </ul>
      </div>

      <button type="button" class="nav prev" (click)="slide(-1)" [attr.aria-label]="'carousel.previous' | t">
        <mat-icon aria-hidden="true">chevron_left</mat-icon>
      </button>
      <button type="button" class="nav next" (click)="slide(1)" [attr.aria-label]="'carousel.next' | t">
        <mat-icon aria-hidden="true">chevron_right</mat-icon>
      </button>

      @if (!reducedMotion) {
        <button type="button" class="toggle" (click)="stopped.set(!stopped())"
                [attr.aria-label]="(stopped() ? 'carousel.play' : 'carousel.pause') | t">
          <mat-icon aria-hidden="true">{{ stopped() ? 'play_arrow' : 'pause' }}</mat-icon>
        </button>
      }
    </section>
  `,
  styleUrl: './product-carousel.scss',
})
export class ProductCarousel {
  readonly products = input.required<ProductListItem[]>();
  /** What the carousel shows, for screen readers ("On sale now"). */
  readonly label = input('');

  private readonly viewport = viewChild.required<ElementRef<HTMLElement>>('viewport');
  private readonly track = viewChild.required<ElementRef<HTMLElement>>('track');

  /** The products twice, for the endless loop. */
  protected readonly loop = computed(() => [...this.products(), ...this.products()]);

  protected readonly hovering = signal(false);
  /** Paused with the button. */
  protected readonly stopped = signal(false);
  protected readonly reducedMotion =
    typeof matchMedia === 'function' && matchMedia('(prefers-reduced-motion: reduce)').matches;

  /** How far the row has moved to the left, in pixels. */
  private offset = 0;
  /** An arrow click in progress: slide from `from` to `to`. */
  private glide: { from: number; to: number; start: number } | null = null;
  private lastTime = 0;
  private frame = 0;

  constructor() {
    // Start the animation once the cards are on screen (in the browser only).
    afterNextRender(() => {
      this.frame = requestAnimationFrame((time) => this.tick(time));
    });
    inject(DestroyRef).onDestroy(() => cancelAnimationFrame(this.frame));
  }

  /** Arrows: slide one screen of cards to the left (1) or right (-1). */
  protected slide(direction: 1 | -1): void {
    const distance = this.viewport().nativeElement.clientWidth;
    this.glide = { from: this.offset, to: this.offset + direction * distance, start: performance.now() };
  }

  /** Runs about 60 times a second, just before the browser paints. */
  private tick(time: number): void {
    // Seconds since the last frame (capped, so coming back to the tab doesn't make it jump).
    const seconds = Math.min(time - (this.lastTime || time), 50) / 1000;
    this.lastTime = time;

    const copyWidth = this.copyWidth();
    const enoughToScroll = copyWidth > this.viewport().nativeElement.clientWidth;

    if (this.glide) {
      const progress = Math.min(1, (time - this.glide.start) / ARROW_MS);
      const eased = 1 - Math.pow(1 - progress, 3); // ease-out: fast start, gentle stop
      this.offset = this.glide.from + (this.glide.to - this.glide.from) * eased;
      if (progress === 1) this.glide = null;
    } else if (enoughToScroll && !this.hovering() && !this.stopped() && !this.reducedMotion) {
      this.offset += SPEED * seconds;
    }

    // Keep the offset inside one copy: this is the invisible jump that makes the loop endless.
    if (copyWidth > 0 && !this.glide) {
      this.offset = ((this.offset % copyWidth) + copyWidth) % copyWidth;
    }
    const shown = copyWidth > 0 ? ((this.offset % copyWidth) + copyWidth) % copyWidth : 0;
    // translate3d lets the graphics card move the row, which keeps the motion smooth.
    this.track().nativeElement.style.transform = `translate3d(${-shown}px, 0, 0)`;

    this.frame = requestAnimationFrame((next) => this.tick(next));
  }

  /** The width of one full copy of the products, gaps included. */
  private copyWidth(): number {
    const items = this.track().nativeElement.children;
    const count = this.products().length;
    if (count === 0 || items.length < count * 2) return 0;
    return (items[count] as HTMLElement).offsetLeft - (items[0] as HTMLElement).offsetLeft;
  }
}
