import { api } from './api';

export interface BriefingResponse {
  sucesso: boolean;
  mensagemErro?: string;
  briefing: string;
  geradoEm: string;
  custoUsd: number;
  doCache: boolean;
  interacaoId: number;
}

export interface AcaoSugerida {
  clienteId: number;
  prioridade: 'urgente' | 'alta' | 'media';
  acao: string;
  justificativa: string;
}

export interface ProximasAcoesResponse {
  sucesso: boolean;
  mensagemErro?: string;
  acoes: AcaoSugerida[];
  geradoEm: string;
  custoUsd: number;
  interacaoId: number;
}

export interface IAConsumoResumo {
  tenantId: string;
  tenantNome?: string;
  chamadas30d: number;
  chamadas24h: number;
  custoUsd30d: number;
  custoUsd24h: number;
  chamadasComErro30d: number;
  pctCacheHit30d: number;
  chamadasPorOperacao: Record<string, number>;
}

export interface CondicaoExtraida {
  valorTotal: number;
  entrada: number;
  sinal: number;
  qtdParcelasMensais: number;
  valorParcelaMensal: number;
  qtdSemestrais: number;
  valorSemestral: number;
  valorChaves: number;
  qtdPosChaves: number;
  valorPosChaves: number;
  indice: string;
  taxaJurosAnual: number;
}

export interface PropostaExtraida {
  valorOferecido: number;
  observacoes?: string;
  condicao: CondicaoExtraida;
  camposFaltantes: string[];
}

export interface ExtrairPropostaResponse {
  sucesso: boolean;
  mensagemErro?: string;
  proposta?: PropostaExtraida;
  custoUsd: number;
  interacaoId: number;
}

export interface Objecao {
  tema: string;
  ocorrencias: number;
  ultimaMencao: string;
  sugestaoContorno: string;
}

export interface AnaliseObjecoesResponse {
  sucesso: boolean;
  mensagemErro?: string;
  objecoes: Objecao[];
  custoUsd: number;
  doCache: boolean;
  interacaoId: number;
}

export const copilotoService = {
  async briefing(clienteId: number): Promise<BriefingResponse> {
    const response = await api.get<BriefingResponse>(`/copiloto/briefing/${clienteId}`);
    return response.data;
  },
  async proximasAcoes(corretorId?: number): Promise<ProximasAcoesResponse> {
    const response = await api.get<ProximasAcoesResponse>('/copiloto/proximas-acoes', {
      params: corretorId ? { corretorId } : undefined
    });
    return response.data;
  },
  async extrairProposta(apartamentoId: number, conversa: string): Promise<ExtrairPropostaResponse> {
    const response = await api.post<ExtrairPropostaResponse>('/copiloto/extrair-proposta', { apartamentoId, conversa });
    return response.data;
  },
  async analisarObjecoes(clienteId: number): Promise<AnaliseObjecoesResponse> {
    const response = await api.get<AnaliseObjecoesResponse>(`/copiloto/objecoes/${clienteId}`);
    return response.data;
  },
  async consumoIA(): Promise<IAConsumoResumo> {
    const response = await api.get<IAConsumoResumo>('/ia/consumo');
    return response.data;
  }
};
