import { computed, effect } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { EN, TranslationKey } from './en';
import { KA } from './ka';
import { KA_TERMS } from './ka-terms';

export type Language = 'ka' | 'en';

const STORAGE_KEY = 'onestep.lang';
const DICTIONARIES: Record<Language, Record<TranslationKey, string>> = { en: EN, ka: KA };

function savedLanguage(): Language {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved === 'en' || saved === 'ka' ? saved : 'ka';
  } catch {
    return 'ka';
  }
}

/**
 * The site's language (Georgian or English), switchable at runtime.
 * Every translated text on screen reads the `lang` signal, so switching re-renders them all at once,
 * without reloading the page. The choice is remembered in localStorage.
 */
export const LanguageStore = signalStore(
  { providedIn: 'root' },
  withState({ lang: savedLanguage() }),

  withComputed(({ lang }) => ({
    /** Locale for dates: Georgian month names, or English ones. */
    locale: computed(() => (lang() === 'ka' ? 'ka' : 'en-GB')),
  })),

  withMethods((store) => ({
    setLanguage(lang: Language): void {
      patchState(store, { lang });
      try {
        localStorage.setItem(STORAGE_KEY, lang);
      } catch {
        // private mode: the choice just isn't remembered
      }
    },

    /**
     * Translates a key, e.g. t('cart.title') -> "Your cart" / "შენი კალათა".
     * {name} placeholders are filled from params. "one|other" picks a form by params.count.
     */
    t(key: TranslationKey, params?: Record<string, string | number | null | undefined>): string {
      let text = DICTIONARIES[store.lang()][key] ?? EN[key] ?? key;
      if (params && 'count' in params && text.includes('|')) {
        const [one, other] = text.split('|');
        text = Number(params['count']) === 1 ? one : other;
      }
      if (params) {
        for (const [name, value] of Object.entries(params)) {
          text = text.replaceAll(`{${name}}`, String(value ?? ''));
        }
      }
      return text;
    },

    /** Names that come from the database (categories, attributes, delivery methods). English stays as stored. */
    term(name: string | null | undefined): string {
      if (!name) return '';
      return store.lang() === 'ka' ? (KA_TERMS[name] ?? name) : name;
    },
  })),

  withHooks({
    onInit(store) {
      // Screen readers and the browser's spell-checker need the right lang attribute.
      effect(() => document.documentElement.setAttribute('lang', store.lang()));
    },
  }),
);
