import { api } from './api';

export interface SimulacaoCompletaRequest {
  valorImovel: number;
  entrada: number;
  prazoAnos?: number;
  taxaSfhAnual?: number;
  uf?: string;
  rendaMensal?: number;
  outrasDividas?: number;
  aluguelAtual?: number;
  qtdParcelasDireto?: number;
}

export interface FinanciamentoResult {
  sistema: string;
  valorImovel: number;
  entrada: number;
  valorFinanciado: number;
  pctEntrada: number;
  prazoMeses: number;
  taxaAnualPct: number;
  primeiraParcela: number;
  ultimaParcela: number;
  jurosTotais: number;
  custoTotal: number;
  cet: number;
}

export interface ImpostosCompraResult {
  uf: string;
  valorImovel: number;
  itbiPct: number;
  itbi: number;
  cartorio: number;
  total: number;
  pctSobreImovel: number;
}

export interface CapacidadePagamentoResult {
  rendaMensal: number;
  outrasDividas: number;
  parcelaMaxima30Pct: number;
  parcelaMaxima35Pct: number;
  imovelAproximadoSFH: number;
  alerta?: string;
}

export interface AluguelVsCompraResult {
  prazoAnos: number;
  valorImovelInicial: number;
  valorImovelFinal: number;
  entrada: number;
  aluguelInicial: number;
  aluguelFinal: number;
  gastoTotalComprar: number;
  gastoTotalAlugar: number;
  patrimonioFinalComprar: number;
  patrimonioFinalAlugar: number;
  saldoLiquidoComprar: number;
  saldoLiquidoAlugar: number;
  diferencaAbsoluta: number;
  recomendacao: string;
}

export interface ParcelamentoDiretoResult {
  valorTotal: number;
  entrada: number;
  qtdParcelas: number;
  taxaReajusteAnualPct: number;
  parcelaInicial: number;
  parcelaFinal: number;
  custoTotal: number;
  jurosTotais: number;
}

export interface SimulacaoCompletaResult {
  valorImovel: number;
  entrada: number;
  sfh: FinanciamentoResult;
  sfi: FinanciamentoResult;
  impostos: ImpostosCompraResult;
  capacidade?: CapacidadePagamentoResult;
  aluguelVsCompra?: AluguelVsCompraResult;
  parcelamentoDireto?: ParcelamentoDiretoResult;
  parcelaCabe?: boolean | null;
  resumoExecutivo: string;
}

export const simuladorService = {
  async simular(req: SimulacaoCompletaRequest): Promise<SimulacaoCompletaResult> {
    const response = await api.post<SimulacaoCompletaResult>('/simulador', req);
    return response.data;
  }
};
