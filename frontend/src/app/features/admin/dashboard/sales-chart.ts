import { CurrencyPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { DailySales } from '../../../core/models/admin.models';
import { I18N } from '../../../core/i18n/translate.pipes';

const WIDTH = 720;
const HEIGHT = 220;
const PAD_LEFT = 8;
const PAD_BOTTOM = 24;

/**
 * Revenue per day as a bar chart, drawn with plain SVG (no chart library to install or explain).
 * viewBox makes it scale to any width; each bar has a <title>, so hovering shows the exact numbers.
 */
@Component({
  selector: 'app-sales-chart',
  imports: [I18N, CurrencyPipe],
  template: `
    <svg [attr.viewBox]="'0 0 ' + width + ' ' + height" preserveAspectRatio="none" role="img"
         [attr.aria-label]="'chart.label' | t: { amount: (max() | currency: 'GEL' : 'symbol-narrow' : '1.0-0') }">
      <!-- guide lines at 50% and 100% of the best day -->
      <line [attr.x1]="padLeft" [attr.x2]="width" [attr.y1]="top" [attr.y2]="top" class="guide" />
      <line [attr.x1]="padLeft" [attr.x2]="width" [attr.y1]="(top + baseline) / 2" [attr.y2]="(top + baseline) / 2" class="guide" />
      @for (bar of bars(); track bar.day) {
        <rect [attr.x]="bar.x" [attr.y]="bar.y" [attr.width]="bar.w" [attr.height]="bar.h" rx="2"
              [class.empty]="bar.revenue === 0">
          <title>{{ 'chart.bar' | t: { day: (bar.day | localDate: 'EEE d MMM'), amount: (bar.revenue | currency: 'GEL' : 'symbol-narrow'), count: bar.orders } }}</title>
        </rect>
      }
      <line [attr.x1]="padLeft" [attr.x2]="width" [attr.y1]="baseline" [attr.y2]="baseline" class="axis" />
    </svg>
    <div class="scale">
      <span>{{ data()[0]?.day | localDate: 'd MMM' }}</span>
      <span>{{ 'chart.best' | t: { amount: (max() | currency: 'GEL' : 'symbol-narrow' : '1.0-0') } }}</span>
      <span>{{ data()[data().length - 1]?.day | localDate: 'd MMM' }}</span>
    </div>
  `,
  styles: `
    :host { display: block; }
    svg { width: 100%; height: 220px; display: block; }
    rect { fill: var(--os-ink); }
    rect:hover { fill: var(--os-step); }
    rect.empty { fill: var(--os-line); }
    .guide { stroke: var(--os-line); stroke-dasharray: 4 4; vector-effect: non-scaling-stroke; }
    .axis { stroke: var(--os-muted); vector-effect: non-scaling-stroke; }
    .scale { display: flex; justify-content: space-between; color: var(--os-muted); font-size: 0.8125rem; margin-top: 6px; font-variant-numeric: tabular-nums; }
  `,
})
export class SalesChart {
  readonly data = input.required<DailySales[]>();

  protected readonly width = WIDTH;
  protected readonly height = HEIGHT;
  protected readonly padLeft = PAD_LEFT;
  protected readonly top = 8;
  protected readonly baseline = HEIGHT - PAD_BOTTOM + 16;

  protected readonly max = computed(() => Math.max(0, ...this.data().map((d) => d.revenue)));

  protected readonly bars = computed(() => {
    const data = this.data();
    const max = this.max() || 1;
    const slot = (WIDTH - PAD_LEFT) / Math.max(1, data.length);
    const gap = Math.min(4, slot * 0.25);
    const usable = this.baseline - this.top;
    return data.map((d, i) => {
      // Empty days get a 2px stub, so the timeline stays readable.
      const h = d.revenue === 0 ? 2 : Math.max(3, (d.revenue / max) * usable);
      return {
        day: d.day,
        revenue: d.revenue,
        orders: d.orderCount,
        x: PAD_LEFT + i * slot + gap / 2,
        w: slot - gap,
        y: this.baseline - h,
        h,
      };
    });
  });
}
