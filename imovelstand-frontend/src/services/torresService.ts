import { api } from './api';
import type { TorreResponse } from '@/types/api';

export interface TorreCreateRequest {
  empreendimentoId: number;
  nome: string;
  pavimentos: number;
  apartamentosPorPavimento: number;
}

export interface TorreUpdateRequest {
  nome: string;
  pavimentos: number;
  apartamentosPorPavimento: number;
}

export const torresService = {
  async list(empreendimentoId?: number): Promise<TorreResponse[]> {
    const response = await api.get<TorreResponse[]>('/torres', {
      params: empreendimentoId ? { empreendimentoId } : undefined
    });
    return response.data;
  },
  async create(data: TorreCreateRequest): Promise<TorreResponse> {
    const response = await api.post<TorreResponse>('/torres', data);
    return response.data;
  },
  async update(id: number, data: TorreUpdateRequest): Promise<void> {
    await api.put(`/torres/${id}`, data);
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/torres/${id}`);
  }
};
