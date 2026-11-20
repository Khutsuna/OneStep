import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { CategoryNode } from '../models/catalog.models';
import { CategoriesApi } from './categories.api';

interface CategoryState {
  tree: CategoryNode[];
  loaded: boolean;
}

/** The category tree, loaded once and shared by the header menu, home page and filters. */
export const CategoryStore = signalStore(
  { providedIn: 'root' },
  withState<CategoryState>({ tree: [], loaded: false }),

  withComputed(({ tree }) => ({
    /** Every category by id, at any depth, for quick look-ups. */
    byId: computed(() => {
      const map = new Map<number, CategoryNode>();
      const visit = (nodes: CategoryNode[]) =>
        nodes.forEach((node) => {
          map.set(node.id, node);
          visit(node.children);
        });
      visit(tree());
      return map;
    }),
  })),

  withMethods((store, api = inject(CategoriesApi)) => ({
    load(): void {
      if (store.loaded()) {
        return;
      }
      api.getTree().subscribe({
        next: (tree) => patchState(store, { tree, loaded: true }),
        error: () => patchState(store, { loaded: false }),
      });
    },

    /** The chain from the top level down to this category, for breadcrumbs. */
    pathTo(categoryId: number): CategoryNode[] {
      const path: CategoryNode[] = [];
      let current = store.byId().get(categoryId);
      while (current) {
        path.unshift(current);
        current = current.parentId ? store.byId().get(current.parentId) : undefined;
      }
      return path;
    },
  })),

  withHooks({
    onInit(store) {
      store.load();
    },
  }),
);

/** A Material Symbols icon for each top-level category. */
export function categoryIcon(slug: string): string {
  const icons: Record<string, string> = {
    'phones-tablets': 'smartphone',
    smartphones: 'smartphone',
    tablets: 'tablet',
    computers: 'computer',
    laptops: 'laptop',
    monitors: 'monitor',
    audio: 'headphones',
    headphones: 'headphones',
    speakers: 'speaker',
    accessories: 'cable',
    'chargers-cables': 'cable',
    smartwatches: 'watch',
  };
  return icons[slug] ?? 'devices';
}
