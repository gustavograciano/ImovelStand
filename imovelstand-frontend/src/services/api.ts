import axios, { AxiosError, type AxiosRequestConfig, type AxiosResponse } from 'axios';
import { useAuthStore } from '@/stores/authStore';
import type { TokenPair } from '@/types/api';

const API_URL: string = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

export const api = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' }
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Interceptor de resposta: em 401, tenta refresh uma vez; se der certo, retry original.
 * Concorrência: enquanto uma tentativa de refresh está em voo, outros 401s esperam
 * a mesma promise (evita N refresh em paralelo).
 */
let refreshing: Promise<string | null> | null = null;

async function doRefresh(): Promise<string | null> {
  const state = useAuthStore.getState();
  if (!state.refreshToken) return null;

  try {
    const response = await axios.post<TokenPair>(`${API_URL}/auth/refresh`, {
      refreshToken: state.refreshToken
    });
    state.setSession(response.data);
    return response.data.accessToken;
  } catch {
    state.clearSession();
    return null;
  }
}

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as (AxiosRequestConfig & { _retry?: boolean }) | undefined;
    if (!original || error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }
    original._retry = true;

    refreshing ??= doRefresh().finally(() => {
      refreshing = null;
    });
    const newToken = await refreshing;
    if (!newToken) return Promise.reject(error);

    original.headers = original.headers ?? {};
    (original.headers as Record<string, string>).Authorization = `Bearer ${newToken}`;
    return api(original) as unknown as AxiosResponse;
  }
);
