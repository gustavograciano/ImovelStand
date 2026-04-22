import { api } from './api';
import { useAuthStore } from '@/stores/authStore';
import type { TokenPair } from '@/types/api';

export const authService = {
  async login(email: string, senha: string): Promise<TokenPair> {
    const response = await api.post<TokenPair>('/auth/login', { email, senha });
    useAuthStore.getState().setSession(response.data);
    return response.data;
  },

  async logout(): Promise<void> {
    const refreshToken = useAuthStore.getState().refreshToken;
    try {
      if (refreshToken) {
        await api.post('/auth/logout', { refreshToken });
      }
    } finally {
      useAuthStore.getState().clearSession();
    }
  }
};
