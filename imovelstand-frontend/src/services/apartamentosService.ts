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

export interface ApartamentoCreateRequest {
  torreId: number;
  tipologiaId: number;
  numero: string;
  pavimento: number;
  orientacao?: string;
  precoAtual: number;
  observacoes?: string;
}

export interface ApartamentoUpdateRequest {
  tipologiaId: number;
  numero: string;
  pavimento: number;
  orientacao?: string;
  precoAtual: number;
  status: StatusApartamento;
  observacoes?: string;
}

export const apartamentosService = {
  async list(filter: ApartamentoFilter = {}): Promise<PagedResult<ApartamentoResponse>> {
    const response = await api.get<PagedResult<ApartamentoResponse>>('/apartamentos', { params: filter });
    return response.data;
  },
  async get(id: number): Promise<ApartamentoResponse> {
    const response = await api.get<ApartamentoResponse>(`/apartamentos/${id}`);
    return response.data;
  },
  async create(data: ApartamentoCreateRequest): Promise<ApartamentoResponse> {
    const response = await api.post<ApartamentoResponse>('/apartamentos', data);
    return response.data;
  },
  async update(id: number, data: ApartamentoUpdateRequest): Promise<void> {
    await api.put(`/apartamentos/${id}`, data);
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/apartamentos/${id}`);
  }
};
