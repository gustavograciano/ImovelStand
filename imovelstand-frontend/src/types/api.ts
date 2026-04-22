export interface TokenPair {
  accessToken: string;
  accessTokenExpiraEm: string;
  refreshToken: string;
  refreshTokenExpiraEm: string;
  usuarioId: number;
  nome: string;
  email: string;
  role: string;
  tenantId: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export type StatusApartamento = 'Disponivel' | 'Reservado' | 'Proposta' | 'Vendido' | 'Bloqueado';

export interface ApartamentoResponse {
  id: number;
  torreId: number;
  torreNome?: string | null;
  tipologiaId: number;
  tipologiaNome?: string | null;
  numero: string;
  pavimento: number;
  orientacao?: string | null;
  precoAtual: number;
  status: StatusApartamento;
  observacoes?: string | null;
  dataCadastro: string;
}

export interface ClienteResponse {
  id: number;
  nome: string;
  cpf: string;
  email: string;
  telefone: string;
  dataCadastro: string;
  statusFunil: string;
  consentimentoLgpd?: boolean;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
