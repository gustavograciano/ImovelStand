import { api } from './api';

export interface AnaliseCredito {
  id: number;
  clienteId: number;
  status: 'Pendente' | 'EmProcessamento' | 'Concluida' | 'Revogada' | 'Falhou';
  provedor: string;
  rendaMediaComprovada?: number;
  volatilidadeRenda?: number;
  dividasRecorrentes?: number;
  capacidadePagamento?: number;
  score?: number;
  alertas: string[];
  consentimentoLgpd: boolean;
  consentimentoLgpdEm?: string;
  expiraEm: string;
  concluidaEm?: string;
  mensagemErro?: string;
  createdAt: string;
}

export const analiseCreditoService = {
  async solicitar(clienteId: number): Promise<{ id: number; token: string; connectUrl: string; status: string }> {
    const r = await api.post(`/analise-credito/clientes/${clienteId}/solicitar`);
    return r.data;
  },
  async listarDoCliente(clienteId: number): Promise<AnaliseCredito[]> {
    const r = await api.get<AnaliseCredito[]>(`/analise-credito/clientes/${clienteId}`);
    return r.data;
  },
  async obter(id: number): Promise<AnaliseCredito> {
    const r = await api.get<AnaliseCredito>(`/analise-credito/${id}`);
    return r.data;
  },
  async autorizarStub(id: number): Promise<AnaliseCredito> {
    const r = await api.post<AnaliseCredito>(`/analise-credito/${id}/autorizar-stub`);
    return r.data;
  },
  async revogar(id: number): Promise<void> {
    await api.post(`/analise-credito/${id}/revogar`);
  }
};
