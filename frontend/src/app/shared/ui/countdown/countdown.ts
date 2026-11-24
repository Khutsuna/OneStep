import { Component, DestroyRef, computed, inject, input, signal } from '@angular/core';
import { I18N } from '../../../core/i18n/translate.pipes';

/**
 * "2 days 05:12:33" until a moment in time, ticking every second.
 * Used on the home page for the campaign that ends first (campaigns end on a schedule).
 */
@Component({
  selector: 'app-countdown',
  imports: [I18N],
  template: `
    @if (left(); as t) {
      <span class="label">{{ label() }}</span>
      <span class="clock" role="timer" aria-live="off">
        @if (t.days > 0) {
          <span class="box days">{{ 'countdown.days' | t: { count: t.days } }}</span>
        }
        <span class="box">{{ t.hours }}</span><span class="sep">:</span>
        <span class="box">{{ t.minutes }}</span><span class="sep">:</span>
        <span class="box">{{ t.seconds }}</span>
      </span>
    }
  `,
  styles: `
    :host { display: inline-flex; align-items: center; flex-wrap: wrap; gap: 8px 12px; }
    .label { font-weight: 600; }
    .clock { display: inline-flex; align-items: center; gap: 4px; font-variant-numeric: tabular-nums; }
    .box {
      min-width: 2.2em; padding: 6px 8px; border-radius: var(--os-radius-sm);
      background: var(--os-step); color: var(--os-ink);
      font-family: var(--os-display); font-weight: 800; text-align: center;
    }
    .box.days { margin-right: 4px; }
    .sep { font-weight: 800; }
  `,
})
export class Countdown {
  /** The end time, as an ISO date from the API. */
  readonly endsAt = input.required<string>();
  /** Text in front of the clock, e.g. "Audio Week ends in". */
  readonly label = input('');

  private readonly now = signal(Date.now());

  constructor() {
    // Tick once a second; stop when the component is removed from the page.
    const timer = setInterval(() => this.now.set(Date.now()), 1000);
    inject(DestroyRef).onDestroy(() => clearInterval(timer));
  }

  /** Time left split into parts, or null once it has ended (then nothing is shown). */
  protected readonly left = computed(() => {
    // The API sends UTC dates without a time zone; adding "Z" makes the browser read them as UTC.
    const iso = this.endsAt();
    const end = Date.parse(/[zZ]|[+-]\d\d:\d\d$/.test(iso) ? iso : iso + 'Z');
    const ms = end - this.now();
    if (!(ms > 0)) {
      return null;
    }
    const total = Math.floor(ms / 1000);
    const pad = (n: number) => String(n).padStart(2, '0');
    return {
      days: Math.floor(total / 86400),
      hours: pad(Math.floor((total % 86400) / 3600)),
      minutes: pad(Math.floor((total % 3600) / 60)),
      seconds: pad(total % 60),
    };
  });
}
