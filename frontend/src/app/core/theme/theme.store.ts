import { effect } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';

export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'onestep.theme';

/** The saved choice; day mode until the visitor picks night mode with the moon button. */
function initialTheme(): Theme {
  try {
    return localStorage.getItem(STORAGE_KEY) === 'dark' ? 'dark' : 'light';
  } catch {
    return 'light'; // private mode
  }
}

/**
 * Day or night mode. The store only toggles class="dark" on <html>;
 * styles.scss redefines the colour tokens under html.dark, so every component follows without changes.
 * Same pattern as LanguageStore: a signal, a method, and localStorage to remember the choice.
 */
export const ThemeStore = signalStore(
  { providedIn: 'root' },
  withState({ theme: initialTheme() }),

  withMethods((store) => ({
    toggle(): void {
      const theme: Theme = store.theme() === 'dark' ? 'light' : 'dark';
      patchState(store, { theme });
      try {
        localStorage.setItem(STORAGE_KEY, theme);
      } catch {
        // private mode: the choice just isn't remembered
      }
    },
  })),

  withHooks({
    onInit(store) {
      effect(() => {
        const dark = store.theme() === 'dark';
        document.documentElement.classList.toggle('dark', dark);
        // The browser's address bar colour on phones.
        document.querySelector('meta[name="theme-color"]')?.setAttribute('content', dark ? '#0e172d' : '#101a33');
      });
    },
  }),
);
