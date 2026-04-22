import { api } from './api';
import type {
  ClienteCreateRequest,
  ClienteResponse,
  InteracaoResponse,
  OrigemLead,
  PagedResult,
  StatusFunil,
  TipoInteracao
} from '@/types/api';

export interface ClienteFilter {
  statusFunil?: StatusFunil;
  origemLead?: OrigemLead;
  corretorId?: number;
  page?: number;
  pageSize?: number;
}

export const clientesService = {
  async list(filter: ClienteFilter = {}): Promise<PagedResult<ClienteResponse>> {
    const response = await api.get<PagedResult<ClienteResponse>>('/clientes', { params: filter });
    return response.data;
  },
  async get(id: number): Promise<ClienteResponse> {
    const response = await api.get<ClienteResponse>(`/clientes/${id}`);
    return response.data;
  },
  async create(data: ClienteCreateRequest): Promise<ClienteResponse> {
    const response = await api.post<ClienteResponse>('/clientes', data);
    return response.data;
  },
  async update(id: number, data: Partial<ClienteResponse>): Promise<void> {
    await api.put(`/clientes/${id}`, data);
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/clientes/${id}`);
  },
  async listInteracoes(id: number): Promise<InteracaoResponse[]> {
    const response = await api.get<InteracaoResponse[]>(`/clientes/${id}/interacoes`);
    return response.data;
  },
  async addInteracao(id: number, tipo: TipoInteracao, conteudo: string): Promise<InteracaoResponse> {
    const response = await api.post<InteracaoResponse>(`/clientes/${id}/interacoes`, { tipo, conteudo });
    return response.data;
  },
  async setConsentimentoLgpd(id: number, aceitou: boolean): Promise<void> {
    await api.post(`/clientes/${id}/consentimento-lgpd`, { aceitou });
  },
  async exportLgpd(id: number): Promise<unknown> {
    const response = await api.get(`/clientes/${id}/export`);
    return response.data;
  }
};
