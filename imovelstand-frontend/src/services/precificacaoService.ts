import { api } from './api';

export interface SugestaoPreco {
  id: number;
  apartamentoId: number;
  apartamentoNumero?: string;
  torreNome?: string;
  tipologiaNome?: string;
  precoAtual: number;
  precoSugerido: number;
  variacaoPct: number;
  motivo: string;
  justificativa: string;
  confianca: number;
  velocidadeVendaSemanal: number;
  status: 'pendente' | 'aceita' | 'rejeitada' | 'expirada';
  createdAt: string;
}

export const precificacaoService = {
  async listar(status: string = 'pendente'): Promise<{ items: SugestaoPreco[]; dinheiroPotencial: number }> {
    const r = await api.get<SugestaoPreco[]>('/precificacao/sugestoes', { params: { status } });
    const dinheiro = Number(r.headers['x-dinheiro-potencial'] ?? 0);
    return { items: r.data, dinheiroPotencial: dinheiro };
  },
  async aceitar(id: number): Promise<{ novoPreco: number }> {
    const r = await api.post<{ novoPreco: number }>(`/precificacao/sugestoes/${id}/aceitar`);
    return r.data;
  },
  async rejeitar(id: number, motivo: string): Promise<void> {
    await api.post(`/precificacao/sugestoes/${id}/rejeitar`, { motivo });
  },
  async calcular(apartamentoId: number): Promise<SugestaoPreco | null> {
    const r = await api.post<SugestaoPreco | null>(`/precificacao/calcular/${apartamentoId}`);
    return r.data;
  },
  async recalcularTenant(): Promise<{ geradas: number }> {
    const r = await api.post<{ geradas: number }>('/precificacao/recalcular-tenant');
    return r.data;
  }
};
