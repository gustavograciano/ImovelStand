import { api } from './api';
import type { ApartamentoResponse, PagedResult, StatusApartamento } from '@/types/api';

export interface ApartamentoFilter {
  status?: StatusApartamento;
  torreId?: number;
  tipologiaId?: number;
  pavimentoMin?: number;
  pavimentoMax?: number;
  precoMin?: number;
  precoMax?: number;
  page?: number;
  pageSize?: number;
}

export const apartamentosService = {
  async list(filter: ApartamentoFilter = {}): Promise<PagedResult<ApartamentoResponse>> {
    const response = await api.get<PagedResult<ApartamentoResponse>>('/apartamentos', { params: filter });
    return response.data;
  },
  async get(id: number): Promise<ApartamentoResponse> {
    const response = await api.get<ApartamentoResponse>(`/apartamentos/${id}`);
    return response.data;
  }
};
