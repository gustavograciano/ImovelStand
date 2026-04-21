# Plano de Desenvolvimento — ImovelStand SaaS

> **Documento técnico de roadmap** — última atualização: 2026-04-21
> **Meta:** em 14 semanas part-time (15–20h/semana), transformar o MVP atual num SaaS multi-tenant vendável pra incorporadoras com 50–500 unidades por empreendimento.

---

## Índice

1. [Arquitetura-alvo](#1-arquitetura-alvo)
2. [Modelo de dados completo](#2-modelo-de-dados-completo)
3. [Sprints (14 semanas)](#3-sprints-14-semanas--15-20h-cada)
4. [Fluxos críticos detalhados](#4-fluxos-críticos-detalhados)
5. [Testes — pirâmide alvo](#5-testes--pirâmide-alvo)
6. [CI/CD e ambientes](#6-cicd-e-ambientes)
7. [Segurança — checklist OWASP + LGPD](#7-segurança--checklist-owasp--lgpd)
8. [Checklist pré-lançamento comercial](#8-checklist-pré-lançamento-comercial)
9. [Cronograma e esforço consolidado](#9-cronograma-e-esforço-consolidado)
10. [Próximos passos concretos](#10-próximos-passos-concretos-próxima-semana)

---

## 1. Arquitetura-alvo

### Stack técnica decidida

| Camada | Escolha | Porquê |
|---|---|---|
| **Backend** | .NET 9 + ASP.NET Core Web API | Já é o atual |
| **ORM** | EF Core 9 + SQL Server 2022 | Já é o atual |
| **Validação** | FluentValidation 11 | Padrão da indústria, testável |
| **Mapping** | Mapster 7 | Mais rápido que AutoMapper, menos boilerplate |
| **PDF** | QuestPDF 2024 | Licença free pra <US$1M/ano receita, API declarativa |
| **DOCX** | DocX (Xceed) 3 | Templates de contrato com placeholders |
| **Jobs** | Hangfire 1.8 + SqlServerStorage | Dashboard nativo, reliability |
| **Logs** | Serilog → Seq (dev) / App Insights (prod) | Structured logging |
| **Storage** | MinIO (dev) / Azure Blob (prod) | Compatível S3, migração fácil |
| **Auth** | JWT com refresh token + BCrypt cost 12 | Já é o atual, só adicionar refresh |
| **Frontend** | Vite 7 + React 19 + **TypeScript** + MUI v7 | Consistente com seu NepeCorp |
| **State** | Zustand + TanStack Query v5 | Zustand = estado UI, TanStack = server state |
| **Forms** | React Hook Form + Zod | Validação tipada client+server |
| **Testes backend** | xUnit + FluentAssertions + Moq + Testcontainers | Integration com SQL real |
| **Testes frontend** | Vitest + Testing Library + Playwright (E2E) | Cobertura unitária + E2E |
| **CI/CD** | GitHub Actions + Docker | build/test/lint/deploy |
| **Hosting** | Azure App Service (API) + Azure Static Web Apps (SPA) + Azure SQL | Barato pra começar, escala fácil |
| **Billing** | **Iugu** | Boleto, Pix, cartão — essencial no Brasil; Stripe não tem boleto |
| **Notificação** | MailKit (email) + Z-API (WhatsApp) | Z-API é mais simples que Twilio pra WhatsApp BR |
| **Observabilidade** | Application Insights + Sentry (frontend) | Padrão mercado |

### Estrutura da solução (refatoração)

**Hoje:**
```
ImovelStand/
├── ImovelStand.Api/          # monolito
├── ImovelStand.Tests/
├── imovelstand-frontend/
├── HashGenerator/
└── UpdatePasswords/
```

**Alvo:**
```
ImovelStand/
├── src/
│   ├── ImovelStand.Domain/            # entities, enums, value objects, domain errors
│   ├── ImovelStand.Application/       # services, DTOs, validators, interfaces
│   ├── ImovelStand.Infrastructure/    # DbContext, migrations, repositories, storage, email
│   ├── ImovelStand.Api/               # controllers, middleware, DI, Program.cs
│   └── ImovelStand.Jobs/              # Hangfire jobs (expiração, lembretes)
├── tests/
│   ├── ImovelStand.Domain.Tests/
│   ├── ImovelStand.Application.Tests/
│   └── ImovelStand.Api.IntegrationTests/  # Testcontainers
├── frontend/
│   └── imovelstand-web/              # Vite + React + TS + MUI
├── deploy/
│   ├── docker-compose.yml            # dev
│   ├── docker-compose.prod.yml
│   └── azure/                        # bicep/terraform
├── docs/
│   ├── ARCHITECTURE.md
│   ├── API.md                        # gerado do Swagger
│   └── DATA_MODEL.md
└── .github/workflows/
    ├── backend-ci.yml
    ├── frontend-ci.yml
    └── deploy.yml
```

---

## 2. Modelo de dados completo

### Campos comuns a todas entidades (exceto juntos e auditoria)
```csharp
public abstract class TenantEntity
{
    public long Id { get; set; }                   // bigint — SQL Server IDENTITY
    public Guid TenantId { get; set; }             // isolamento multi-tenant
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long CreatedByUserId { get; set; }
    public long? UpdatedByUserId { get; set; }
    public bool Deleted { get; set; }              // soft-delete global
    public DateTime? DeletedAt { get; set; }
}
```

### Diagrama (17 entidades + 3 tabelas de junção)

| # | Entidade | Campos chave | Relacionamentos |
|---|---|---|---|
| 1 | **Tenant** | Id, Nome, Cnpj, Slug, PlanoId, AtivoAte, TrialAte | 1:N Usuarios, Empreendimentos |
| 2 | **Plano** | Id, Nome, PrecoMensal, MaxEmpreendimentos, MaxUnidades, MaxUsuarios, Features(JSON) | 1:N Tenants |
| 3 | **Usuario** | Email, SenhaHash, Nome, Role, Creci?, PercentualComissao, Ativo, UltimoLogin | FK Tenant |
| 4 | **RefreshToken** | Token, UsuarioId, ExpiraEm, Revogado, IpCriacao | FK Usuario |
| 5 | **Empreendimento** | Nome, Slug, Descricao, Endereco(owned), DataLancamento, DataEntregaPrevista, Status, VgvEstimado, Construtora | FK Tenant · 1:N Torres, TabelasPreco |
| 6 | **Torre** | EmpreendimentoId, Nome, Pavimentos, ApartamentosPorPavimento | FK Empreendimento · 1:N Apartamentos |
| 7 | **Tipologia** | EmpreendimentoId, Nome, AreaPrivativa, AreaTotal, Quartos, Suites, Banheiros, Vagas, PrecoBase, PlantaUrl | FK Empreendimento · 1:N Apartamentos |
| 8 | **Apartamento** | TorreId, TipologiaId, Numero, Pavimento, Orientacao (Enum), PrecoAtual, Status (Enum) | FK Torre, Tipologia · 1:N Fotos, HistoricoPrecos, Reservas, Propostas |
| 9 | **HistoricoPreco** | ApartamentoId, PrecoAnterior, PrecoNovo, Motivo, DataAlteracao | FK Apartamento |
| 10 | **TabelaPreco** | EmpreendimentoId, Versao, DataVigenciaInicio, Ativa | FK Empreendimento · 1:N TabelaPrecoItens |
| 11 | **TabelaPrecoItem** | TabelaPrecoId, ApartamentoId, Preco | FK TabelaPreco, Apartamento |
| 12 | **Cliente** | Nome, Cpf, Rg, DataNascimento, EstadoCivil, RegimeBens, Profissao, Empresa, RendaMensal, Email, Telefone, Whatsapp, Endereco(owned), OrigemLead (Enum), StatusFunil (Enum), CorretorResponsavelId, ConjugeId(self-FK), ConsentimentoLgpd, ConsentimentoLgpdEm | FK Tenant, Usuario · 1:N Dependentes, Propostas, Visitas, Interacoes |
| 13 | **ClienteDependente** | ClienteId, Nome, Cpf, DataNascimento, Parentesco | FK Cliente |
| 14 | **Visita** | ClienteId, CorretorId, EmpreendimentoId, DataHora, DuracaoMin, Observacoes, GerouProposta | FK Cliente, Usuario, Empreendimento |
| 15 | **HistoricoInteracao** | ClienteId, UsuarioId, Tipo (Enum), Conteudo, DataHora | FK Cliente, Usuario |
| 16 | **Proposta** | Numero, ClienteId, ApartamentoId, CorretorId, ValorOferecido, Status (Enum), DataEnvio, DataValidade, Observacoes, CondicaoPagamento(owned) | FK Cliente, Apartamento, Usuario · 1:N PropostaHistoricoStatus · 1:1 Venda |
| 17 | **Venda** | PropostaId, ApartamentoId, ClienteId, CorretorId, DataFechamento, ValorFinal, Status (Enum), ContratoUrl, CondicaoPagamentoFinal(owned) | FK Proposta, Apartamento, Cliente, Usuario · 1:N Comissoes |
| 18 | **Comissao** | VendaId, UsuarioId, TipoComissao (Captacao/Venda/Gerente), Percentual, Valor, Status (Pendente/Paga/Cancelada), DataPagamento | FK Venda, Usuario |
| 19 | **Reserva** | ClienteId, ApartamentoId, CorretorId, DataReserva, DataExpiracao, Status, VirouPropostaId? | FK Cliente, Apartamento, Usuario, Proposta |
| 20 | **Foto** (polymorphic) | EntidadeTipo (Enum), EntidadeId, Url, Ordem, Legenda | — |
| 21 | **AuditLog** | TenantId, UsuarioId, Entidade, EntidadeId, Acao, PayloadAntes(JSON), PayloadDepois(JSON), Ip, UserAgent | FK Tenant, Usuario |

### Value Objects (owned types)

```csharp
public class Endereco        // street, number, complemento, bairro, city, state, cep
public class CondicaoPagamento {
    public decimal Entrada { get; set; }
    public DateTime? EntradaData { get; set; }
    public decimal Sinal { get; set; }
    public int QtdParcelasMensais { get; set; }
    public decimal ValorParcelaMensal { get; set; }
    public DateTime PrimeiraParcelaData { get; set; }
    public int QtdSemestrais { get; set; }
    public decimal ValorSemestral { get; set; }
    public decimal ValorChaves { get; set; }
    public DateTime? ChavesDataPrevista { get; set; }
    public int QtdPosChaves { get; set; }
    public decimal ValorPosChaves { get; set; }
    public IndiceReajuste Indice { get; set; }    // INCC, IPCA, SEM
    public decimal TaxaJurosAnual { get; set; }
}
```

### Decisões arquiteturais (e porquê)

| Decisão | Justificativa |
|---|---|
| **TenantId** em cada entidade + `HasQueryFilter` global no EF Core | Defesa em profundidade. Vazar dados entre tenants = fim do negócio |
| **Soft-delete** via `Deleted` + query filter | CFM não exige (diferente de saúde), mas incorporadora gosta de "desfazer" |
| **bigint** em `Id` | SQL Server suporta `int.MaxValue` = 2B, mas unidades + históricos extrapolam |
| **Audit fields** em toda entidade + `AuditLog` para mudanças sensíveis | Rastreabilidade em disputa comercial |
| **Owned types** para `Endereco` e `CondicaoPagamento` | São entidades sem vida própria; evita JOIN desnecessário |
| **`HistoricoPreco`** separado ao invés de `ModifiedAt` simples | Diretor exige saber "por que e quando o preço mudou" |
| **`Proposta`** separada de `Venda` | Proposta pode ter contrapropostas (N versions); venda é único evento final |
| **Snapshot de `CondicaoPagamentoFinal` em Venda** | Proposta pode ser editada após virar venda — venda tem sua própria cópia congelada |
| **`Comissao` 1:N** em Venda | Uma venda pode ter 2+ comissões (captação + venda + override gerente) |

---

## 3. Sprints (14 semanas × ~15-20h cada)

### Sprint 0 — Setup & plataforma (3-4 dias)
> Base técnica antes de qualquer feature.

| Task | Critério de aceite | Esforço |
|---|---|---|
| Reorganizar solução em 5 projetos (Domain/Application/Infra/Api/Jobs) | `dotnet sln list` mostra os 5 projetos; build sem erros | 4h |
| Configurar Serilog → Seq (docker local) | Log de request chega no Seq em `http://localhost:5341` | 2h |
| Configurar Sentry no frontend | Throw artificial é capturado no dashboard | 1h |
| `docker-compose.yml` dev: API + SQL + Seq + MinIO + Hangfire dashboard | `docker compose up` sobe stack inteira | 3h |
| GitHub Actions: workflow `backend-ci.yml` (restore/build/test) | PR dispara pipeline verde | 2h |
| GitHub Actions: workflow `frontend-ci.yml` (install/lint/build/test) | Idem | 2h |
| Dependabot + CodeQL habilitados | Settings → Security → visíveis | 30min |
| Branch protection `main` (exige PR + CI verde) | Push direto em main bloqueado | 15min |
| README técnico novo (rodar local, env vars, arquitetura) | Novo dev consegue `docker compose up` em <10min | 2h |

### Sprint 1 — Modelo de dados core (1 semana)
> Refatoração que destrava todo o resto.

| Task | Critério de aceite | Esforço |
|---|---|---|
| Criar entidades `Empreendimento`, `Torre`, `Tipologia` em Domain | Classes com invariantes (não permite `Apartamento` sem Torre) | 4h |
| Refatorar `Apartamento`: FK pra Torre e Tipologia, remover campos que migraram | Builda sem warnings | 2h |
| Criar `HistoricoPreco` com trigger/interceptor EF para capturar changes em `PrecoAtual` | Mudar preço → aparece linha em HistoricoPreco | 3h |
| `Foto` (polymorphic table) | Upload pega Empreendimento/Torre/Tipologia/Apartamento | 2h |
| Migration "RefatoracaoDominio" + script de migração de dados do V1 | Rodar migration → dados do MVP continuam acessíveis (apartamentos viram children de empreendimento default) | 4h |
| Seed de demo (1 Empreendimento, 2 Torres, 3 Tipologias, 48 Apartamentos) | `dotnet ef database update` → banco populado | 3h |

### Sprint 2 — Multi-tenant + Auth hardening (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Entidade `Tenant`, `Plano` | Migration + seed de plano Starter/Pro/Business | 2h |
| `TenantProvider` lendo claim `tenantId` do JWT | Injetável via DI em repos | 2h |
| `HasQueryFilter(e => e.TenantId == tenantProvider.TenantId)` em `OnModelCreating` de TODAS entidades tenant-scoped | Teste integration: Tenant A **não** vê dados de Tenant B | 4h |
| `RefreshToken` entity + endpoint `POST /auth/refresh` | Token expira → refresh funciona | 4h |
| `POST /auth/logout` revoga refresh token | Token revogado não consegue renovar | 1h |
| Roles + policies: `[Authorize(Roles = "Admin,Gerente")]` em endpoints sensíveis | Corretor tenta deletar apt → 403 | 2h |
| Password policy: mínimo 8, 1 maiúscula, 1 número, 1 especial + BCrypt cost 12 | Validado no `UsuarioService.CreateAsync` | 1h |
| Rate limiting no `/auth/login` (10 tentativas/IP/5min) | 11ª tentativa → 429 | 1h |
| Testes: `MultiTenantIsolationTests` (5 casos: list, get, update, delete, create cross-tenant) | Todos passam (vermelho antes, verde depois) | 4h |

### Sprint 3 — DTOs, validação, mapeamento, convenções (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| DTOs para todas entidades (`EmpreendimentoRequest/Response`) em Application | Controllers nunca mais expõem `DbModel` | 6h |
| Mapster configs em `MappingRegistry.cs` | `dto.Adapt<Entidade>()` funciona | 2h |
| FluentValidation validators (CPF, CNPJ, email, datas coerentes) | `POST` com CPF inválido → 400 com erro estruturado | 4h |
| Middleware global de erro → retorna `ProblemDetails` (RFC 7807) | Exception não-tratada → JSON bonito com traceId | 3h |
| Padronizar paginação: `PagedResult<T>` com `Items`, `Page`, `PageSize`, `Total` | `GET /empreendimentos?page=1&pageSize=20` | 2h |
| Filtros: query string → expression tree simples (sem OData) | `GET /apartamentos?status=Disponivel&quartos=3` funciona | 4h |

### Sprint 4 — Uploads de fotos/plantas (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Abstração `IFileStorage` + `MinioFileStorage` (dev) + `AzureBlobFileStorage` (prod) | Troca via `appsettings.json` sem mudar código | 4h |
| `POST /empreendimentos/{id}/fotos` multipart | Upload de JPG/PNG/WebP até 10MB | 3h |
| Pipeline de processamento: gerar thumbnail 400px + médio 800px + manter original | `ImageSharp` | 4h |
| CDN URL signing para Azure Blob | Fotos não são públicas; URL tem SAS de 1h | 2h |
| Limite por plano (Starter: 50 fotos total · Pro: 500 · Business: ∞) | Enforcement no service | 2h |
| Virus scan opcional (ClamAV containerizado) ou validação de magic bytes | Upload de `.exe` renomeado → 400 | 3h |

### Sprint 5 — CRM de cliente completo (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Refatorar `Cliente` com todos os campos novos (RG, renda, estado civil, etc) | Migration + seed | 4h |
| `OrigemLead` enum (Facebook, Instagram, Google, Indicacao, Plantao, Site, Outros) | Form no frontend | 1h |
| `StatusFunil` enum (Lead, Contato, Visita, Proposta, Venda, Descarte) + transições válidas | Kanban possível | 2h |
| `ConjugeId` self-FK + tela de vincular | Casal aparece no contrato como co-titulares | 3h |
| `ClienteDependente` 1:N | Dependentes no contrato | 2h |
| `HistoricoInteracao` + `POST /clientes/{id}/interacoes` | Timeline do cliente | 3h |
| `Visita` + `POST /visitas` (registra quem visitou qual empreendimento e quando) | Filtro por corretor | 2h |
| Consentimento LGPD (`ConsentimentoLgpd`, `ConsentimentoLgpdEm`) + endpoint `/clientes/{id}/export` (Art. 18) | JSON com todos dados do cliente | 3h |

### Sprint 6 — Proposta + Condição de Pagamento (1 semana) ⭐

| Task | Critério de aceite | Esforço |
|---|---|---|
| Entidade `Proposta` + `CondicaoPagamento` (owned) | Migration | 3h |
| Status state machine: Rascunho → Enviada → (Contraproposta N) → Aceita/Reprovada/Expirada | Transições inválidas → exception | 4h |
| `PropostaHistoricoStatus` registra cada mudança (quem, quando, por quê) | Visível no frontend | 2h |
| `POST /propostas` (rascunho) + `POST /propostas/{id}/enviar` | Ao enviar, dispara email pro cliente | 3h |
| Contraproposta: `POST /propostas/{id}/contrapropor` cria nova versão mantendo histórico | Versionamento linear | 4h |
| Expiração automática via Hangfire: proposta sem resposta em 7 dias → Expirada | Hangfire job `ExpirarPropostasAntigasJob` | 3h |
| Calculadora: dado `valorTotal + entrada + sinal + chaves + prazoMensal + indice`, gerar parcelas projetadas | Service `CalculadoraFinanceira` testada | 4h |
| Conflito: se Apartamento tem Proposta `Enviada`, bloquear nova Proposta `Enviada` | Retorna 409 Conflict | 2h |

### Sprint 7 — Venda + comissão + workflow de aprovação (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Entidade `Venda` + `CondicaoPagamentoFinal` (snapshot) | Ao aceitar proposta, cria venda copiando condição | 3h |
| Status state machine venda: Negociada → EmContrato → Assinada → (Cancelada/Distratada) | Idem proposta | 3h |
| Entidade `Comissao` 1:N em Venda | 1 venda pode ter 3 comissões (corretor captação, corretor venda, gerente override) | 3h |
| Workflow de aprovação: Corretor cria venda status `Negociada` → Gerente aprova → `EmContrato` | RBAC no endpoint `/vendas/{id}/aprovar` | 3h |
| Ao aprovar: muda status de Apartamento pra `Vendido`, cancela reservas/propostas concorrentes | Transação atômica | 3h |
| Cálculo automático de comissão baseado em `Usuario.PercentualComissao` + override manual | Gerente pode editar antes de fechar | 3h |
| `PUT /comissoes/{id}/pagar` → status `Paga` + `DataPagamento` | Financeiro fecha comissão | 1h |
| Relatório: comissões em aberto por corretor | Endpoint + teste | 2h |

### Sprint 8 — Espelho de vendas PDF (QuestPDF) (1 semana) ⭐⭐⭐
> **Diferencial #1 do produto.** Isto fecha venda com diretor comercial.

| Task | Critério de aceite | Esforço |
|---|---|---|
| Configurar QuestPDF, tema visual (cores, logo slot) | Licença Community aceita | 2h |
| Template **Espelho de Vendas por Torre**: grid visual por andar mostrando cor do status (Disponível/Reservado/Proposta/Vendido) | PDF A3 landscape | 8h |
| **Espelho Executivo**: 1ª página com KPIs (VGV vendido, % vendido, velocidade, preço médio m²), 2ª+ págs com grid | PDF em 1 click | 6h |
| **Espelho Comercial** (pra corretor): lista plana com preço, condição padrão, resumo de 1 linha por apt | Ordenável por preço/andar | 4h |
| Endpoint `GET /empreendimentos/{id}/espelho?tipo=torre|executivo|comercial&formato=pdf` | Download direto | 2h |
| Job Hangfire: gera espelho executivo toda sexta 18h e envia pro email do Admin | Cron `0 18 * * 5` | 2h |
| Watermark "CONFIDENCIAL" + data/hora de geração + nome do usuário que gerou | Auditoria | 1h |
| Testes de snapshot do PDF (golden file) | PDF gerado = PDF esperado byte-a-byte (excluindo timestamp) | 3h |

### Sprint 9 — Geração de contrato DOCX (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Tela de **Templates de Contrato** (Admin upload `.docx` com placeholders `{{cliente.nome}}`, `{{apt.numero}}`, `{{condicao.valorTotal}}`) | Armazenado em Blob por Tenant | 4h |
| Motor de substituição: `DocX` + regex de placeholders com path (`{{cliente.conjuge.nome}}`) | Template + Venda = DOCX final | 6h |
| `POST /vendas/{id}/gerar-contrato?templateId=xxx` | Retorna DOCX | 2h |
| Converter DOCX → PDF via LibreOffice headless (container) ou SyncFusion (opcional) | PDF assinável | 4h |
| Upload de contrato assinado + `venda.ContratoUrl` + `venda.Status = Assinada` | Status atualiza automaticamente | 2h |
| Histórico: `ContratoVersao` se regenerar | Auditoria | 2h |

### Sprint 10 — Dashboard & relatórios (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Endpoint `GET /dashboard/overview?empreendimentoId=x` retorna KPIs | `vgvTotal`, `vgvVendido`, `pctVendido`, `unidadesDisponiveis`, `velocidadeVendaSemanal`, `precoMedioM2`, `conversaoFunilPct` | 4h |
| Gráfico funil: Lead → Visita → Proposta → Venda (contagem + conversão) | Dados últimos 90 dias | 3h |
| Ranking de corretores (vendas × VGV × comissão) | Paginado + filtros por período | 2h |
| Mapa de calor de vendas por pavimento/face do prédio | Heatmap SVG | 4h |
| Relatório Excel: todas vendas do período com condição de pagamento expandida | ClosedXML | 4h |
| Relatório Excel: funil detalhado por origem de lead | Idem | 2h |
| Endpoint `GET /dashboard/alertas`: reservas expirando em 24h, propostas sem resposta 5+ dias | Usado no sino de notificações | 2h |

### Sprint 11 — Hangfire jobs & notificações (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| `ExpirarReservasJob` (a cada 10min) | Reserva vencida → `Expirada` + libera apt + email pro corretor | 3h |
| `ExpirarPropostasJob` (diário) | Idem | 2h |
| `LembreteReservaVencendoJob` (diário 8h): reserva vence em 24h → email/WhatsApp cliente | Template configurável | 3h |
| `EspelhoSemanalJob` (sex 18h) | Dispara espelho pro Admin | 2h |
| Integração **MailKit** + configuração SMTP por Tenant ou SMTP NepeCorp padrão | Email sai | 3h |
| Integração **Z-API** (WhatsApp) | Mensagem chega no celular do teste | 4h |
| Sistema de templates de email/WhatsApp: `{{cliente.nome}}`, `{{empreendimento.nome}}`, `{{apt.numero}}` | Editor na UI do admin | 3h |
| Preferência de contato do cliente (aceita Email? WhatsApp? SMS?) | Respeitado em todos os jobs | 2h |

### Sprint 12 — Frontend refactor completo (1-2 semanas)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Migrar `App.jsx` (268 linhas) para TS + React Router v7 com layout/outlet | Rotas navegáveis + deep link | 4h |
| MUI v7 theme + componentes: AppBar, Drawer, DataGrid, Dialog, Snackbar | Look profissional | 6h |
| Zustand store: `authStore`, `uiStore` | User persistido em localStorage | 3h |
| TanStack Query: fetch + cache + invalidation + optimistic updates | `useEmpreendimentos()`, `useApartamentos(filtros)` | 6h |
| React Hook Form + Zod em TODOS os forms (cliente, proposta, etc) | Validação client espelha FluentValidation server | 8h |
| Telas CRM: Lista clientes com kanban por `StatusFunil` | Drag & drop entre colunas | 6h |
| **Tela de empreendimento com mapa interativo**: grid 2D do prédio, cada apt clicável, cor = status | Diferencial visual forte | 10h |
| PWA: `vite-plugin-pwa`, service worker, manifest, instalável em tablet | Corretor instala como app | 3h |
| Offline-first nos reads (Apartamentos + Clientes): TanStack Query + IndexedDB | Corretor sem internet consegue consultar | 6h |
| Error boundaries + Sentry + skeleton loaders em todas as telas | UX polido | 4h |
| Tradução i18n (pt-BR inicial, en opcional) | `react-i18next` | 2h |
| Responsividade mobile (breakpoints MUI) | Testa Safari iOS e Chrome Android | 4h |

### Sprint 13 — Importação/exportação + integrações (1 semana)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Importar tabela de preços Excel (upload → parse → validação → preview → confirmar) | ClosedXML + wizard 3 passos | 8h |
| Importar clientes Excel (CSV padrão) | Preview + detecção de duplicatas por CPF | 4h |
| Exportar qualquer lista (clientes, vendas, apts) em Excel/CSV | Botão em toda tabela | 3h |
| Webhook: venda fechada → dispara POST configurável (Zapier/n8n friendly) | Retry exponential se falhar | 4h |
| Integração opcional com **CRECI/CRECI-SP**: validar CRECI do corretor (se disponibilizar API) | Fallback manual | 2h |
| Integração opcional com **Receita**: validar CNPJ do Tenant no cadastro | Via BrasilAPI | 2h |

### Sprint 14 — Billing + onboarding + landing + deploy prod (1-2 semanas)

| Task | Critério de aceite | Esforço |
|---|---|---|
| Integração **Iugu** (criar subscription, webhook de pagamento) | Trial 14 dias → cobra automaticamente | 6h |
| Billing: downgrade bloqueia features fora do plano (`[RequiresPlan("Pro")]`) | Starter tenta importar > 50 unidades → 402 | 4h |
| Onboarding: wizard inicial (cria 1º empreendimento demo com 48 apts) | Tempo até primeiro valor < 5min | 4h |
| Tour guiado no app (react-joyride ou similar) | 5 passos cobrindo principais telas | 3h |
| Landing page (Astro + Tailwind ou Next) em `imovelstand.com.br` | SEO + CTA trial | 8h |
| Deploy prod: Azure App Service + Azure SQL + Azure Blob + Static Web App | URL oficial | 6h |
| Monitoring: Application Insights + alertas (erros 5xx > 10/min, latência p95 > 2s) | Email pro admin se disparar | 2h |
| Backup diário automático Azure SQL + teste de restore | Documentado | 2h |
| Documentação pública (docs.imovelstand.com.br) | Docusaurus/Mintlify | 6h |
| Política de privacidade + termos de uso (advogado revisou) | Publicado | 4h |

---

## 4. Fluxos críticos detalhados

### Fluxo de venda (happy path)
```
[Corretor] POST /clientes (novo lead)
         → Cliente.StatusFunil = Lead
         → HistoricoInteracao "Cadastro"

[Corretor] POST /visitas
         → Visita registrada
         → Cliente.StatusFunil = Visita

[Corretor] POST /propostas (rascunho)
         → Proposta.Status = Rascunho
         → valida: Apt.Status = Disponível (ou Reservado por este cliente)

[Corretor] POST /propostas/{id}/enviar
         → Proposta.Status = Enviada
         → Apt.Status = Disponível (não muda — só muda na venda)
         → Email/WhatsApp pro cliente
         → Hangfire agenda ExpirarPropostaJob em 7 dias

[Cliente responde: aceito]
[Corretor] POST /propostas/{id}/aceitar
         → cria Venda (Status = Negociada)
         → copia CondicaoPagamento → CondicaoPagamentoFinal
         → calcula Comissoes (corretor = %)
         → Apt.Status AINDA = Disponível (trava no aprovar)

[Gerente] POST /vendas/{id}/aprovar
         → Transação atômica:
           - Venda.Status = EmContrato
           - Apt.Status = Vendido
           - cancela todas Reservas/Propostas concorrentes do apt
           - AuditLog

[Admin] POST /vendas/{id}/gerar-contrato?templateId=x
         → DOCX gerado e armazenado
         → Email pro cliente com link de download + tutorial de assinatura

[Cliente assina e devolve]
[Admin] POST /vendas/{id}/contrato-assinado (upload)
         → Venda.Status = Assinada
         → Comissoes disponíveis pra pagamento
         → Notificação pro corretor: "Venda X fechada!"
```

### Fluxo de reserva (trava apartamento por 7 dias)
```
POST /reservas
  → Apt.Status = Reservado
  → Reserva.DataExpiracao = now + 7d
  → Bloqueia outras reservas/propostas/vendas no mesmo apt

Se cliente quer seguir:
  POST /propostas (converte reserva em proposta formal)
  → Reserva.Status = Convertida, VirouPropostaId = X

Se cliente some:
  Hangfire ExpirarReservasJob roda a cada 10min:
    - Reservas ativas com DataExpiracao < now
    - Reserva.Status = Expirada
    - Apt.Status = Disponível
    - Email pro corretor: "Reserva X expirou"
    - Email pro cliente: "Sua reserva expirou, ainda temos interesse?"
```

### Isolamento multi-tenant
```csharp
// EF Core OnModelCreating — aplicado a TODAS entidades TenantEntity via reflection
modelBuilder.Entity<Cliente>().HasQueryFilter(c =>
    c.TenantId == _tenantProvider.TenantId && !c.Deleted);

// Test obrigatório:
[Fact]
public async Task GetCliente_DoOutroTenant_RetornaNotFound()
{
    var tenantA = await SeedTenant("A");
    var tenantB = await SeedTenant("B");
    var clienteA = await CreateClienteInTenant(tenantA);

    UseToken(tenantB);  // simula request do Tenant B

    var resp = await _client.GetAsync($"/clientes/{clienteA.Id}");
    resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

---

## 5. Testes — pirâmide alvo

| Camada | Ferramenta | Cobertura alvo | Volume |
|---|---|---|---|
| Unit (Domain) | xUnit + FluentAssertions | 90% | ~150 testes |
| Unit (Application) | xUnit + FluentAssertions + Moq | 80% | ~200 testes |
| Integration (API) | Testcontainers (SQL real) + `WebApplicationFactory` | 60% dos endpoints | ~80 testes |
| E2E crítico | Playwright | 5 jornadas (login, criar apt, criar proposta, aprovar venda, gerar espelho) | 5 suites |
| Snapshot PDF | QuestPDF + `Verify` | 3 templates de espelho | 3 testes |

Total estimado ao final: **~430 testes automatizados**, rodando em <5min no CI.

---

## 6. CI/CD e ambientes

| Ambiente | URL | Banco | Deploy trigger |
|---|---|---|---|
| **Dev local** | localhost | SQL container | `docker compose up` |
| **PR preview** | pr-123.imovelstand.dev | Azure SQL serverless | abrir PR |
| **Staging** | staging.imovelstand.com.br | Azure SQL S0 | merge em `main` |
| **Prod** | app.imovelstand.com.br | Azure SQL S2 | tag `v*.*.*` + manual approval |

### Pipeline GitHub Actions
```yaml
# backend-ci.yml (em cada PR)
- Checkout
- Setup .NET 9
- Restore (com cache de NuGet)
- Build em Release
- Test (com cobertura em XML)
- Upload coverage → Codecov
- SAST: CodeQL
- Dependabot: automático

# deploy.yml (em tag ou main)
- Build Docker images (API + Web)
- Push Azure Container Registry
- Terraform apply (idempotente)
- EF Core migrations (idempotente)
- Restart App Service
- Smoke test (curl em /health)
- Se falhar: rollback automático
```

---

## 7. Segurança — checklist OWASP + LGPD

| Controle | Status esperado |
|---|---|
| **A01 — Broken Access Control** | Testes multi-tenant + roles obrigatórios nos endpoints sensíveis |
| **A02 — Cryptographic Failures** | BCrypt cost 12; HTTPS+HSTS; SQL em TDE (Azure SQL tem by default); `Secrets` via Azure Key Vault |
| **A03 — Injection** | EF Core (parametrizado); FluentValidation em toda entrada; CSP no frontend |
| **A04 — Insecure Design** | Threat model documentado em `docs/THREAT_MODEL.md`; aprovação de venda requer Gerente |
| **A05 — Security Misconfig** | CORS lista whitelist (não `AllowAll`); debug desligado em prod; headers de segurança (via middleware) |
| **A06 — Vulnerable Components** | Dependabot + GHAS Secret Scanning + `dotnet list package --vulnerable` no CI |
| **A07 — Auth Failures** | Rate limit em login; lockout após 10 falhas em 5min; MFA para Admin (fase 2) |
| **A08 — Data Integrity** | Audit log imutável + hash-chain opcional (emprestar do NepeCorp) |
| **A09 — Logging Failures** | Serilog estruturado; nunca logar senhas/tokens (regra no `Destructure.By`) |
| **A10 — SSRF** | Nenhum endpoint aceita URL de usuário (upload é multipart) |
| **LGPD — Consentimento** | `Cliente.ConsentimentoLgpd` + timestamp ao cadastro |
| **LGPD — Portabilidade (Art. 18)** | `GET /clientes/{id}/export` → JSON completo |
| **LGPD — Anonimização (Art. 16)** | `DELETE /clientes/{id}` → anonimiza (mantém FK de vendas pra integridade fiscal) |

---

## 8. Checklist pré-lançamento comercial

| Categoria | Item |
|---|---|
| **Legal** | ☐ CNPJ da empresa · ☐ Contrato social · ☐ Política de privacidade revisada por advogado · ☐ Termos de uso · ☐ DPO designado (pode ser você) |
| **Produto** | ☐ Trial 14 dias funcionando · ☐ Onboarding < 5min · ☐ Export/delete LGPD funcionando · ☐ Espelho PDF polido |
| **Técnico** | ☐ Backup diário automático + restore testado · ☐ Alertas 5xx/latência · ☐ SSL A+ no SSLLabs · ☐ Headers segurança A+ no securityheaders.com · ☐ Disaster recovery documentado |
| **Financeiro** | ☐ Iugu conta aprovada · ☐ NF-e automática configurada · ☐ Conta bancária PJ separada |
| **Suporte** | ☐ Email de suporte + SLA público · ☐ WhatsApp business · ☐ Base de conhecimento público · ☐ Status page (UptimeRobot grátis) |
| **Marketing** | ☐ Landing page · ☐ 3 casos de uso documentados · ☐ 1 depoimento em vídeo · ☐ LinkedIn da empresa · ☐ Google My Business |

---

## 9. Cronograma e esforço consolidado

| Sprint | Semana | Tema | Horas estimadas |
|---|---|---|---|
| 0 | 1 (parcial) | Setup plataforma | 15h |
| 1 | 1-2 | Modelo core | 18h |
| 2 | 3 | Multi-tenant + Auth | 20h |
| 3 | 4 | DTOs/validação | 21h |
| 4 | 5 | Uploads | 18h |
| 5 | 6 | CRM completo | 20h |
| 6 | 7 | Proposta ⭐ | 25h |
| 7 | 8 | Venda + comissão | 21h |
| 8 | 9 | Espelho PDF ⭐⭐⭐ | 26h |
| 9 | 10 | Contrato DOCX | 20h |
| 10 | 11 | Dashboard | 21h |
| 11 | 12 | Jobs + notificações | 22h |
| 12 | 13-14 | Frontend refactor | 62h |
| 13 | 15 | Import/export | 23h |
| 14 | 16-17 | Billing + deploy | 45h |
| | | **Total** | **~377h** |

> **Em 20h/semana:** 19 semanas ≈ 4-4,5 meses
> **Em 15h/semana:** 25 semanas ≈ 6 meses
> **Colchão de 20% pra imprevistos:** 22-30 semanas

---

## 10. Próximos passos concretos (próxima semana)

Ordem de ataque da **Semana 1 (Sprint 0 + início Sprint 1)**:

1. Criar branch `feat/refactor-architecture` no ImovelStand
2. Extrair `src/ImovelStand.Domain/` e mover entidades (`Apartamento`, `Cliente`, etc) pra lá
3. Criar `src/ImovelStand.Infrastructure/` com `ApplicationDbContext`
4. Criar `src/ImovelStand.Application/` com primeiros DTOs (`ApartamentoResponse`, `ApartamentoCreateRequest`)
5. Adicionar Serilog ao `Program.cs` (substituir `ILogger<T>` por `Log.ForContext<T>()`)
6. Subir `docker-compose.yml` dev com SQL + Seq
7. Fechar PR. CI verde.
