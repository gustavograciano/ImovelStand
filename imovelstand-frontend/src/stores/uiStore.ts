import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type ThemeMode = 'dark' | 'light';

interface UiState {
  mode: ThemeMode;
  sidebarCollapsed: boolean;
  toggleMode: () => void;
  setMode: (mode: ThemeMode) => void;
  toggleSidebar: () => void;
}

export const useUiStore = create<UiState>()(
  persist(
    (set) => ({
      mode: 'dark',
      sidebarCollapsed: false,
      toggleMode: () => set((s) => ({ mode: s.mode === 'dark' ? 'light' : 'dark' })),
      setMode: (mode) => set({ mode }),
      toggleSidebar: () => set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed }))
    }),
    { name: 'imovelstand-ui' }
  )
);
