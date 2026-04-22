import { api } from './api';
import type { DashboardOverview, FunilConversao, RankingCorretorItem } from '@/types/api';

export interface AlertaReserva {
  id: number;
  clienteId: number;
  clienteNome: string;
  apartamentoId: number;
  numero: string;
  expiraEm: string;
}

export interface AlertaProposta {
  id: number;
  numero: string;
  clienteId: number;
  clienteNome: string;
  status: string;
  dataEnvio: string;
  diasSemResposta: number;
}

export interface AlertasDashboard {
  reservasExpirando: AlertaReserva[];
  propostasSemResposta: AlertaProposta[];
}

export const dashboardService = {
  async overview(empreendimentoId: number): Promise<DashboardOverview> {
    const response = await api.get<DashboardOverview>('/dashboard/overview', { params: { empreendimentoId } });
    return response.data;
  },
  async funil(dias = 90): Promise<FunilConversao> {
    const response = await api.get<FunilConversao>('/dashboard/funil', { params: { dias } });
    return response.data;
  },
  async ranking(): Promise<RankingCorretorItem[]> {
    const response = await api.get<RankingCorretorItem[]>('/dashboard/ranking-corretores');
    return response.data;
  },
  async alertas(): Promise<AlertasDashboard> {
    const response = await api.get<AlertasDashboard>('/dashboard/alertas');
    return response.data;
  },
  async exportVendas(): Promise<Blob> {
    const response = await api.get<Blob>('/dashboard/export/vendas.xlsx', { responseType: 'blob' });
    return response.data;
  }
};
