import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { TokenPair } from '@/types/api';

interface AuthUser {
  usuarioId: number;
  nome: string;
  email: string;
  role: string;
  tenantId: string;
}

interface AuthState {
  accessToken: string | null;
  accessExpira: string | null;
  refreshToken: string | null;
  user: AuthUser | null;
  setSession: (pair: TokenPair) => void;
  clearSession: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      accessExpira: null,
      refreshToken: null,
      user: null,
      setSession: (pair) =>
        set({
          accessToken: pair.accessToken,
          accessExpira: pair.accessTokenExpiraEm,
          refreshToken: pair.refreshToken,
          user: {
            usuarioId: pair.usuarioId,
            nome: pair.nome,
            email: pair.email,
            role: pair.role,
            tenantId: pair.tenantId
          }
        }),
      clearSession: () =>
        set({ accessToken: null, accessExpira: null, refreshToken: null, user: null }),
      isAuthenticated: () => !!get().accessToken
    }),
    {
      name: 'imovelstand-auth'
    }
  )
);
