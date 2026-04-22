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
export type StatusFunil = 'Lead' | 'Contato' | 'Visita' | 'Proposta' | 'Negociacao' | 'Venda' | 'Descarte';
export type OrigemLead = 'Indicacao' | 'Facebook' | 'Instagram' | 'Google' | 'Plantao' | 'Site' | 'WhatsApp' | 'Evento' | 'Outros';
export type StatusProposta = 'Rascunho' | 'Enviada' | 'ContrapropostaCliente' | 'ContrapropostaCorretor' | 'Aceita' | 'Reprovada' | 'Expirada' | 'Cancelada';
export type StatusVenda = 'Negociada' | 'EmContrato' | 'Assinada' | 'Cancelada' | 'Distratada';
export type IndiceReajuste = 'SemReajuste' | 'Incc' | 'Ipca' | 'Igpm' | 'Tr' | 'Selic';
export type TipoInteracao = 'Ligacao' | 'Whatsapp' | 'Email' | 'ReuniaoPresencial' | 'ReuniaoVideo' | 'Visita' | 'MensagemInterna';

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

export interface EnderecoDto {
  logradouro: string;
  numero: string;
  complemento?: string | null;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
}

export interface ClienteResponse {
  id: number;
  nome: string;
  cpf: string;
  rg?: string | null;
  dataNascimento?: string | null;
  estadoCivil?: string | null;
  profissao?: string | null;
  empresa?: string | null;
  rendaMensal?: number | null;
  email: string;
  telefone: string;
  whatsapp?: string | null;
  endereco?: EnderecoDto | null;
  origemLead?: OrigemLead | null;
  statusFunil: StatusFunil;
  corretorResponsavelId?: number | null;
  conjugeId?: number | null;
  consentimentoLgpd: boolean;
  consentimentoLgpdEm?: string | null;
  dataCadastro: string;
}

export interface ClienteCreateRequest {
  nome: string;
  cpf: string;
  rg?: string;
  dataNascimento?: string;
  estadoCivil?: string;
  profissao?: string;
  empresa?: string;
  rendaMensal?: number;
  email: string;
  telefone: string;
  whatsapp?: string;
  endereco?: EnderecoDto;
  origemLead?: OrigemLead;
  corretorResponsavelId?: number;
  consentimentoLgpd: boolean;
}

export interface InteracaoResponse {
  id: number;
  clienteId: number;
  usuarioId?: number | null;
  usuarioNome?: string | null;
  tipo: TipoInteracao;
  conteudo: string;
  dataHora: string;
}

export interface CondicaoPagamentoDto {
  valorTotal: number;
  entrada: number;
  entradaData?: string | null;
  sinal: number;
  sinalData?: string | null;
  qtdParcelasMensais: number;
  valorParcelaMensal: number;
  primeiraParcelaData?: string | null;
  qtdSemestrais: number;
  valorSemestral: number;
  valorChaves: number;
  chavesDataPrevista?: string | null;
  qtdPosChaves: number;
  valorPosChaves: number;
  indice: IndiceReajuste;
  taxaJurosAnual: number;
}

export interface PropostaResponse {
  id: number;
  numero: string;
  clienteId: number;
  clienteNome?: string | null;
  apartamentoId: number;
  apartamentoNumero?: string | null;
  corretorId: number;
  corretorNome?: string | null;
  versao: number;
  propostaOriginalId?: number | null;
  valorOferecido: number;
  status: StatusProposta;
  dataEnvio?: string | null;
  dataValidade?: string | null;
  dataRespostaCliente?: string | null;
  observacoes?: string | null;
  condicao: CondicaoPagamentoDto;
  createdAt: string;
}

export interface ComissaoResponse {
  id: number;
  vendaId: number;
  usuarioId: number;
  usuarioNome?: string | null;
  tipo: string;
  percentual: number;
  valor: number;
  status: string;
  dataAprovacao?: string | null;
  dataPagamento?: string | null;
}

export interface VendaResponse {
  id: number;
  numero: string;
  propostaId?: number | null;
  clienteId: number;
  clienteNome?: string | null;
  apartamentoId: number;
  apartamentoNumero?: string | null;
  corretorId: number;
  corretorNome?: string | null;
  corretorCaptacaoId?: number | null;
  gerenteAprovadorId?: number | null;
  dataFechamento: string;
  dataAprovacao?: string | null;
  valorFinal: number;
  status: StatusVenda;
  contratoUrl?: string | null;
  observacoes?: string | null;
  condicaoFinal: CondicaoPagamentoDto;
  comissoes: ComissaoResponse[];
}

export interface ReservaResponse {
  id: number;
  clienteId: number;
  apartamentoId: number;
  dataReserva: string;
  dataExpiracao?: string | null;
  status: string;
  observacoes?: string | null;
}

export interface VisitaResponse {
  id: number;
  clienteId: number;
  clienteNome?: string | null;
  corretorId: number;
  corretorNome?: string | null;
  empreendimentoId: number;
  empreendimentoNome?: string | null;
  dataHora: string;
  duracaoMinutos?: number | null;
  observacoes?: string | null;
  gerouProposta: boolean;
}

export interface DashboardOverview {
  empreendimentoId: number;
  empreendimentoNome?: string;
  unidadesTotal: number;
  unidadesDisponiveis: number;
  unidadesReservadas: number;
  unidadesEmProposta: number;
  unidadesVendidas: number;
  vgvTotal: number;
  vgvVendido: number;
  pctVendido: number;
  precoMedioM2: number;
  velocidadeVendaSemanal: number;
  vendasUltimos30Dias: number;
  vendasUltimos90Dias: number;
}

export interface FunilConversao {
  diasAnalisados: number;
  leads: number;
  visitas: number;
  propostas: number;
  vendas: number;
  conversaoLeadParaVisita: number;
  conversaoVisitaParaProposta: number;
  conversaoPropostaParaVenda: number;
  conversaoGlobal: number;
}

export interface RankingCorretorItem {
  corretorId: number;
  nome: string;
  vendasFechadas: number;
  vgvVendido: number;
  comissaoTotal: number;
  visitas: number;
  ticketMedio: number;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
