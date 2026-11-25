import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, of, switchMap } from 'rxjs';
import { ProductsApi } from '../../../core/catalog/products.api';
import { I18N } from '../../../core/i18n/translate.pipes';
import { PricePoint } from '../../../core/models/catalog.models';

const W = 400; // drawing size in SVG units; the SVG scales to the box width
const H = 110;
const PAD = 8;

/**
 * A small step chart of the product's base price over time.
 * The data comes from ProductPriceHistory, which the SQL trigger trg_Products_PriceHistory fills
 * whenever a manager changes a price. Shown only when the price has changed at least once.
 */
@Component({
  selector: 'app-price-history-chart',
  imports: [I18N, CurrencyPipe],
  template: `
    @if (chart(); as c) {
      <section class="box" aria-labelledby="price-history-title">
        <div class="head">
          <h2 id="price-history-title">{{ 'priceHistory.title' | t }}</h2>
          <span class="change" [class.up]="c.change > 0">
            {{ c.change > 0 ? '+' : '−' }}{{ abs(c.change) | currency: 'GEL' : 'symbol-narrow' : '1.0-2' }}
            ({{ c.change > 0 ? '+' : '−' }}{{ abs(c.percent) }}%)
          </span>
        </div>

        <svg [attr.viewBox]="'0 0 ' + width + ' ' + height" role="img"
             [attr.aria-label]="'priceHistory.label' | t: { from: (c.first | currency: 'GEL' : 'symbol-narrow' : '1.0-2'), to: (c.last | currency: 'GEL' : 'symbol-narrow' : '1.0-2') }">
          <defs>
            <linearGradient id="price-history-fill" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0" stop-color="#f5b82e" stop-opacity="0.4" />
              <stop offset="1" stop-color="#f5b82e" stop-opacity="0" />
            </linearGradient>
          </defs>
          <path [attr.d]="c.area" fill="url(#price-history-fill)" />
          <path [attr.d]="c.line" fill="none" style="stroke: var(--os-ink)" stroke-width="2.5" stroke-linejoin="round" />
          @for (dot of c.dots; track $index) {
            <circle [attr.cx]="dot.x" [attr.cy]="dot.y" r="4" style="fill: var(--os-surface); stroke: var(--os-ink)" stroke-width="2" />
          }
          <circle [attr.cx]="c.endX" [attr.cy]="c.endY" r="5" fill="#f5b82e" style="stroke: var(--os-ink)" stroke-width="2" />
        </svg>

        <div class="axis">
          <span>{{ c.startDate | localDate: 'd MMM y' }}</span>
          <span>{{ 'priceHistory.today' | t }}</span>
        </div>
        <p class="legend">
          {{ 'priceHistory.first' | t }} <b>{{ c.first | currency: 'GEL' : 'symbol-narrow' : '1.0-2' }}</b>
          <span aria-hidden="true">→</span>
          {{ 'priceHistory.now' | t }} <b>{{ c.last | currency: 'GEL' : 'symbol-narrow' : '1.0-2' }}</b>
        </p>
        <p class="note">{{ 'priceHistory.note' | t }}</p>
      </section>
    }
  `,
  styles: `
    .box { margin-top: 24px; padding: 18px 18px 14px; border: 1px solid var(--os-line); border-radius: var(--os-radius-lg); background: var(--os-surface); }
    .head { display: flex; align-items: baseline; justify-content: space-between; gap: 12px; margin-bottom: 10px; }
    h2 { font-size: 1.125rem; font-weight: 700; margin: 0; }
    .change { font-weight: 700; color: var(--os-stock); font-variant-numeric: tabular-nums; white-space: nowrap; }
    .change.up { color: var(--os-sale); }
    svg { display: block; width: 100%; height: auto; overflow: visible; }
    .axis { display: flex; justify-content: space-between; color: var(--os-muted); font-size: 0.8125rem; margin-top: 6px; }
    .legend { margin: 10px 0 0; }
    .legend span { margin: 0 6px; color: var(--os-muted); }
    .note { margin: 2px 0 0; color: var(--os-muted); font-size: 0.8125rem; }
  `,
})
export class PriceHistoryChart {
  readonly productId = input.required<number>();

  private readonly api = inject(ProductsApi);

  protected readonly width = W;
  protected readonly height = H;
  protected readonly abs = Math.abs;

  /** Reloads when the product changes (for example after clicking a similar product). */
  private readonly points = toSignal(
    toObservable(this.productId).pipe(
      switchMap((id) => this.api.getPriceHistory(id).pipe(catchError(() => of([] as PricePoint[])))),
    ),
    { initialValue: [] as PricePoint[] },
  );

  /** Turns the points into SVG paths. null = the price never changed, so no chart. */
  protected readonly chart = computed(() => {
    const points = this.points();
    if (points.length < 2) {
      return null;
    }

    const times = points.map((p) => Date.parse(p.date));
    const prices = points.map((p) => p.price);
    const start = times[0];
    const end = Math.max(Date.now(), times[times.length - 1]);
    const min = Math.min(...prices);
    const max = Math.max(...prices);

    // Time goes left to right, price bottom to top.
    const x = (t: number) => PAD + ((t - start) / Math.max(end - start, 1)) * (W - 2 * PAD);
    const y = (price: number) => (max === min ? H / 2 : PAD + ((max - price) / (max - min)) * (H - 2 * PAD));

    // A step line: the price stays flat until the next change, then jumps to the new price.
    let line = `M ${x(times[0])} ${y(prices[0])}`;
    for (let i = 1; i < points.length; i++) {
      line += ` H ${x(times[i])} V ${y(prices[i])}`;
    }
    line += ` H ${x(end)}`;

    const first = prices[0];
    const last = prices[prices.length - 1];
    return {
      line,
      area: `${line} V ${H} H ${x(times[0])} Z`, // the same line, closed along the bottom for the fill
      dots: points.slice(1, -1).map((p, i) => ({ x: x(times[i + 1]), y: y(p.price) })),
      endX: x(end),
      endY: y(last),
      first,
      last,
      change: last - first,
      percent: Math.round(((last - first) / first) * 100),
      startDate: points[0].date,
    };
  });
}
