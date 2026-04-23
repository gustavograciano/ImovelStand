import { api } from './api';
import type { TipologiaResponse } from '@/types/api';

export interface TipologiaCreateRequest {
  empreendimentoId: number;
  nome: string;
  areaPrivativa: number;
  areaTotal: number;
  quartos: number;
  suites: number;
  banheiros: number;
  vagas: number;
  precoBase: number;
  plantaUrl?: string;
}

export type TipologiaUpdateRequest = TipologiaCreateRequest;

export const tipologiasService = {
  async list(empreendimentoId?: number): Promise<TipologiaResponse[]> {
    const response = await api.get<TipologiaResponse[]>('/tipologias', {
      params: empreendimentoId ? { empreendimentoId } : undefined
    });
    return response.data;
  },
  async create(data: TipologiaCreateRequest): Promise<TipologiaResponse> {
    const response = await api.post<TipologiaResponse>('/tipologias', data);
    return response.data;
  },
  async update(id: number, data: TipologiaUpdateRequest): Promise<void> {
    await api.put(`/tipologias/${id}`, data);
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/tipologias/${id}`);
  }
};
