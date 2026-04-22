import { api } from './api';
import type { ComissaoResponse, CondicaoPagamentoDto, PagedResult, StatusVenda, VendaResponse } from '@/types/api';

export interface VendaFilter {
  status?: StatusVenda;
  corretorId?: number;
  clienteId?: number;
  page?: number;
  pageSize?: number;
}

export interface VendaCreateRequest {
  propostaId?: number;
  clienteId: number;
  apartamentoId: number;
  corretorId: number;
  corretorCaptacaoId?: number;
  valorFinal: number;
  condicaoFinal: CondicaoPagamentoDto;
  observacoes?: string;
}

export const vendasService = {
  async list(filter: VendaFilter = {}): Promise<PagedResult<VendaResponse>> {
    const response = await api.get<PagedResult<VendaResponse>>('/vendas', { params: filter });
    return response.data;
  },
  async get(id: number): Promise<VendaResponse> {
    const response = await api.get<VendaResponse>(`/vendas/${id}`);
    return response.data;
  },
  async create(data: VendaCreateRequest): Promise<VendaResponse> {
    const response = await api.post<VendaResponse>('/vendas', data);
    return response.data;
  },
  async aprovar(id: number): Promise<void> {
    await api.post(`/vendas/${id}/aprovar`);
  },
  async cancelar(id: number, motivo: string): Promise<void> {
    await api.post(`/vendas/${id}/cancelar`, { motivo });
  },
  async marcarContratoAssinado(id: number, contratoUrl: string): Promise<void> {
    await api.post(`/vendas/${id}/contrato-assinado`, { contratoUrl });
  },
  async comissoesAbertas(corretorId?: number): Promise<ComissaoResponse[]> {
    const response = await api.get<ComissaoResponse[]>('/vendas/comissoes/abertas', { params: { corretorId } });
    return response.data;
  },
  async pagarComissao(comissaoId: number): Promise<void> {
    await api.put(`/vendas/comissoes/${comissaoId}/pagar`);
  }
};
