import { api } from './api';
import type { EmpreendimentoResponse, EnderecoDto } from '@/types/api';

export interface EmpreendimentoCreateRequest {
  nome: string;
  slug: string;
  descricao?: string;
  construtora?: string;
  endereco: EnderecoDto;
  dataLancamento?: string;
  dataEntregaPrevista?: string;
  status?: string;
  vgvEstimado?: number;
}

export type EmpreendimentoUpdateRequest = EmpreendimentoCreateRequest;

export const empreendimentosService = {
  async list(): Promise<EmpreendimentoResponse[]> {
    const response = await api.get<EmpreendimentoResponse[]>('/empreendimentos');
    return response.data;
  },
  async get(id: number): Promise<EmpreendimentoResponse> {
    const response = await api.get<EmpreendimentoResponse>(`/empreendimentos/${id}`);
    return response.data;
  },
  async create(data: EmpreendimentoCreateRequest): Promise<EmpreendimentoResponse> {
    const response = await api.post<EmpreendimentoResponse>('/empreendimentos', data);
    return response.data;
  },
  async update(id: number, data: EmpreendimentoUpdateRequest): Promise<void> {
    await api.put(`/empreendimentos/${id}`, data);
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/empreendimentos/${id}`);
  }
};
