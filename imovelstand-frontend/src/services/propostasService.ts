import { api } from './api';
import type { CondicaoPagamentoDto, PagedResult, PropostaResponse, StatusProposta } from '@/types/api';

export interface PropostaFilter {
  apartamentoId?: number;
  clienteId?: number;
  status?: StatusProposta;
  page?: number;
  pageSize?: number;
}

export interface PropostaCreateRequest {
  clienteId: number;
  apartamentoId: number;
  corretorId: number;
  valorOferecido: number;
  dataValidade?: string;
  observacoes?: string;
  condicao: CondicaoPagamentoDto;
}

export interface ContrapropostaRequest {
  valorOferecido: number;
  observacoes?: string;
  condicao: CondicaoPagamentoDto;
  vemDoCorretor: boolean;
}

export const propostasService = {
  async list(filter: PropostaFilter = {}): Promise<PagedResult<PropostaResponse>> {
    const response = await api.get<PagedResult<PropostaResponse>>('/propostas', { params: filter });
    return response.data;
  },
  async get(id: number): Promise<PropostaResponse> {
    const response = await api.get<PropostaResponse>(`/propostas/${id}`);
    return response.data;
  },
  async create(data: PropostaCreateRequest): Promise<PropostaResponse> {
    const response = await api.post<PropostaResponse>('/propostas', data);
    return response.data;
  },
  async enviar(id: number): Promise<void> {
    await api.post(`/propostas/${id}/enviar`);
  },
  async contrapropor(id: number, req: ContrapropostaRequest): Promise<PropostaResponse> {
    const response = await api.post<PropostaResponse>(`/propostas/${id}/contrapropor`, req);
    return response.data;
  },
  async alterarStatus(id: number, novoStatus: StatusProposta, motivo?: string): Promise<void> {
    await api.post(`/propostas/${id}/status`, { novoStatus, motivo });
  }
};
