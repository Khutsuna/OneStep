import { Injectable, effect, inject, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterStateSnapshot, TitleStrategy } from '@angular/router';
import { TranslationKey } from './en';
import { LanguageStore } from './language.store';

/**
 * Route titles are translation keys (title: 'title.cart'). This turns them into
 * "Your cart · OneStep" in the current language, and updates the tab when the language changes.
 */
@Injectable({ providedIn: 'root' })
export class TranslatedTitleStrategy extends TitleStrategy {
  private readonly title = inject(Title);
  private readonly language = inject(LanguageStore);
  private readonly key = signal<TranslationKey | null>(null);

  constructor() {
    super();
    effect(() => {
      const key = this.key();
      const name = key ? this.language.t(key) : null;
      this.title.setTitle(name ? `${name} · OneStep` : `OneStep · ${this.language.t('title.default')}`);
    });
  }

  override updateTitle(snapshot: RouterStateSnapshot): void {
    this.key.set((this.buildTitle(snapshot) as TranslationKey | undefined) ?? null);
  }
}
