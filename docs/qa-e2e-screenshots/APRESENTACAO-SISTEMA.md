# ImovelStand — Apresentação do Sistema

**Versão:** Sprint 31 (pós-fix QA)
**Data:** 23/04/2026
**Público-alvo:** Diretores comerciais de incorporadoras médias (50-300 unidades)

---

## O que é

ImovelStand é uma plataforma SaaS de gestão comercial para incorporadoras. Integra num único produto:

1. **CRM completo** — funil lead→venda, Kanban, timeline de interações
2. **Espelho de vendas** visual por torre e pavimento
3. **Copiloto IA** (briefing de cliente, fila priorizada, extrator de proposta, análise de objeções)
4. **Simulador financeiro** embarcável no site do lançamento (captura leads)
5. **WhatsApp Business oficial** com templates aprovados e webhook
6. **Precificação dinâmica** baseada em velocidade + benchmarks de mercado
7. **Análise de crédito via Open Finance** (Pluggy/Belvo) em minutos

---

## Login e Home

### Tela 1 — Login com tema dark profissional
![Login](./01-login.png)

- Autenticação JWT com refresh token rotation
- BCrypt cost 12 para hash de senhas
- Rate limit: 10 tentativas/IP/5min
- Link para "Criar trial grátis" (onboarding gratuito 14 dias)

### Tela 2 — Home personalizada por role
![Home Admin](./02-home-admin.png)

- Saudação dinâmica (bom dia/boa tarde) + nome do usuário
- **Widget "Sua fila de hoje"** (IA) — botão "Gerar fila" chama Claude para priorizar as ações do dia baseado em SLA e histórico
- 4 stats cards: Unidades ativas, Disponíveis, Em negociação, VGV total
- Atalhos contextuais para os módulos principais

**Sidebar**: Itens dinâmicos por role (Usuários só para Admin).

---

## Dashboard

### Tela 3 — KPIs operacionais
![Dashboard](./03-dashboard.png)

- **8 KPIs em destaque**: Unidades totais/vendidas, VGV total/vendido, % vendido, preço médio/m², vendas 30d, velocidade semanal
- **Funil de conversão (90 dias)**: Leads → Visitas → Propostas → Vendas com barras de % de conversão em cada estágio
- **Ranking de corretores por VGV**: tabela com vendas fechadas, VGV, ticket médio
- Filtro por empreendimento (para incorporadoras multi-projeto)

---

## Empreendimentos, Torres e Tipologias

### Tela 4 — Empreendimentos com CRUD + tabs
![Empreendimentos](./04-empreendimentos-cruds.png)

- Tabela com nome, slug, construtora, status (Pré-lançamento/Lançamento/Em obra/Entregue), VGV estimado
- Dialog "Novo empreendimento" com endereço completo (CEP, UF), data lançamento/entrega
- Ao clicar num empreendimento: tabs **Torres** + **Tipologias** inline

### Tela 5 — Torres
- CRUD de torres: nome, número de pavimentos, apartamentos por pavimento, total calculado
- Validação de unicidade do nome da torre dentro do empreendimento

### Tela 6 — Tipologias
- CRUD com área privativa, área total, quartos/suítes/banheiros/vagas, preço base, URL da planta
- Cada tipologia é reutilizada em múltiplos apartamentos

---

## Apartamentos

### Tela 7 — Lista com filtros em cascata
![Apartamentos Lista](./05-apartamentos-lista.png)

- Paginada (20/página, ajustável)
- Filtros: Empreendimento → Torre (cascata) + Status
- Colunas: Número, Torre, Tipologia, Pavimento, Preço, Status colorido

### Tela 8 — Espelho de vendas visual
![Apartamentos Espelho](./06-apartamentos-espelho.png)

- Grid visual tipo espelho real de vendas
- **Agrupado por Torre** (cada torre em bloco separado) — corrigido no QA
- Cada pavimento é uma linha (topo do prédio no topo da tela)
- Cores por status: Verde=Disponível, Amarelo=Reservado, Laranja=Proposta, Vermelho=Vendido, Cinza=Bloqueado
- Hover mostra tooltip com tipologia, preço, status
- Legenda única no rodapé

### Tela 9 — Dialog Novo Apartamento com cascata
- Dropdown Empreendimento → carrega Torres + Tipologias do empreendimento selecionado
- Validação de número único por torre

---

## Clientes (CRM + Kanban de funil)

### Tela 10 — Kanban por estágio do funil
![Clientes Kanban](./07-clientes-kanban.png)

- 6 colunas: Lead, Contato, Visita, Proposta, Negociação, Venda
- Cada cliente é um card clicável (nome, telefone, origem do lead)
- Filtro por Origem do lead (Facebook, Indicação, Google, WhatsApp, etc)
- Botão "Novo cliente" abre dialog completo

### Tela 11 — Dialog Novo Cliente (20+ campos em seções)
- **Pessoais**: Nome, CPF, RG, data nascimento, estado civil, regime de bens
- **Profissional**: profissão, empresa, renda mensal
- **Contato**: email, telefone, WhatsApp
- **Endereço**: CEP, UF, cidade, etc (opcional)
- **Funil/Atribuição**: origem do lead, corretor responsável (dropdown de usuários ativos)
- Checkbox de consentimento LGPD obrigatório

### Tela 12 — Cliente Detail com 4 cards de IA + dados
![Cliente Detail](./08-cliente-detail-ia.png)

**No topo**, 3 cards de IA/diferencial:
1. **Briefing do Cliente (IA)** — botão "Gerar briefing" chama Claude Sonnet com contexto completo (cliente + interações + visitas + propostas) e retorna resumo de 3-5 linhas + sugestão de próxima ação
2. **Objeções detectadas (IA)** — analisa histórico e identifica padrões recorrentes ("preço alto" 3x, "prazo entrega" 2x) com sugestão de contorno
3. **WhatsApp** — timeline estilo WhatsApp (bolhas verdes/brancas), status de entrega/leitura, envio de template + texto livre

**Abaixo**, **Análise de Crédito (Open Finance)** — card dedicado (ver seção específica)

**Perfil do cliente**: todas as informações em grid + botão "Exportar dados (JSON)" para compliance LGPD Art. 18

**Timeline de interações**: Registro cronológico com filtro por tipo (Ligação, WhatsApp, Email, Visita...)

---

## Simulador Financeiro

### Tela 13 — Simulador completo
![Simulador](./09-simulador.png)

Formulário superior:
- Valor do imóvel, Entrada, UF, Prazo (anos), Parcelas direto
- Renda mensal, Outras dívidas (para capacidade), Aluguel atual (para comparativo)

Resultado em 6 cards:
1. **Financiamento SFH (Caixa - SAC)** — primeira/última parcela, juros totais, CET
2. **SFI comparativo** — spread de juros vs SFH explícito
3. **Impostos de compra (UF específica)** — ITBI + cartório com % sobre imóvel
4. **Capacidade de pagamento** — regra dos 30% da renda, com chip verde/vermelho se cabe ou não, imóvel máximo sustentável
5. **Aluguel vs Compra em 30 anos** — duas colunas com patrimônio final em cada cenário + recomendação fundamentada
6. **Parcelamento direto com incorporadora** — com reajuste INCC aplicado

Widget standalone (`simulador-widget/`) embedável em sites de incorporadoras — 3.98 KB gzipped.

---

## Propostas

### Tela 14 — Lista com ações contextuais
![Propostas Lista](./10-propostas-lista.png)

- Filtro por status (Rascunho, Enviada, Aceita, Reprovada, etc)
- Botões de ação dinâmicos: Enviar (se Rascunho), Aceitar/Reprovar (se ativa)
- Linha expansível com **condição de pagamento completa**: valor total, entrada, sinal, parcelas mensais/semestrais, chaves, pós-chaves, índice de reajuste

### Tela 15 — Dialog Nova Proposta com extrator IA
![Nova Proposta](./11-nova-proposta.png)

- Dropdowns: Cliente, Apartamento (só disponíveis), Corretor
- **Botão "Colar conversa (extrair com IA)"** — abre dialog secundário com textarea grande. Cliente cola conversa do WhatsApp/email e IA extrai automaticamente todos os campos de valor, entrada, parcelas, etc
- Condição de pagamento com 14 campos detalhados
- Chips de warning marcam "campos incertos" identificados pela IA

---

## Vendas e Contrato

### Tela 16 — Nova Venda com auto-preenchimento
![Nova Venda](./12-nova-venda.png)

- Dropdown "Proposta (aceita)" — ao selecionar, **auto-preenche** cliente, apartamento, corretor, valor final e toda condição financeira (corrigido no QA)
- Campo de Corretor de captação (opcional) para split de comissão

### Tela 17 — Lista de Vendas com contrato e comissões
![Vendas Lista](./13-vendas-contrato.png)

- Coluna de status: Negociada → EmContrato (após aprovar) → Assinada (após contrato)
- Botão "Aprovar" visível para Admin/Gerente quando status = Negociada
- Botão "Assinar contrato" abre dialog pedindo URL do contrato
- Ao aprovar, o apartamento vira Vendido automaticamente e propostas concorrentes são canceladas
- **Linha expansível mostra comissões geradas** automaticamente (Corretor Teste 3% = R$ 19.500 sobre venda de R$ 650.000)
- URL do contrato exibida com link clicável quando venda está Assinada

---

## Precificação Dinâmica (Diferencial)

### Tela 18 — Dashboard de sugestões com "Dinheiro Potencial"
![Precificacao](./14-precificacao-ia.png)

- **Hero com valor agregado**: "R$ XXX.XXX em dinheiro potencial" — soma dos aumentos sugeridos pendentes
- Botão "Recalcular tudo" roda o motor para todos os apartamentos do tenant
- Filtros por status (pendente, aceita, rejeitada, expirada)
- Cards coloridos por sugestão:
  - Borda esquerda verde (aumento) ou amarela (desconto)
  - Preço atual riscado + novo destacado
  - Chip de variação percentual (+3% ou -3%)
  - Justificativa em linguagem natural (ex: "Unidade disponível há 477 dias com baixa velocidade. Considere desconto de 3%")
  - Barra de confiança 0-100
  - Chips de motivo e velocidade semanal
  - Botões Aceitar (verde) e Rejeitar (vermelho)

### Tela 19 — Aceitar sugestão
- Ao aceitar, o preço do apartamento é atualizado automaticamente
- Sugestão fica registrada como "aceita" com usuário e data (audit trail)
- Rejeitar pede motivo para feedback loop do algoritmo

---

## Open Finance — Análise de Crédito (Diferencial)

### Tela 20 — Solicitar análise
![Open Finance Link](./15-open-finance-link.png)

- Corretor clica "Solicitar análise" no cliente
- Sistema gera link único com token
- Corretor copia e envia ao cliente via WhatsApp/email
- Badge "Pendente" indica aguardando autorização do cliente

### Tela 21 — Análise concluída com score e alertas
![Open Finance Score](./16-open-finance-score.png)

- **Score ImovelStand 0-1000** em destaque com barra colorida (verde >=700, azul 500-699, amarelo <500)
- **Métricas financeiras**:
  - Renda média comprovada (últimos 6 meses)
  - Volatilidade da renda (estabilidade)
  - Dívidas recorrentes/mês (cartão, financiamento, aluguel detectados)
  - Capacidade de pagamento (regra 30% da renda líquida - dívidas)
- **Alertas detectados** em Alert laranja:
  - Renda com alta variabilidade
  - Dívidas >30% da renda
  - Transações de apostas (Betano/Stake detectadas)
- Informa expiração automática em 12 meses (Bacen/LGPD)
- Botão "Revogar (LGPD)" apaga dados financeiros imediatamente com confirmação

### No exemplo QA (João da Silva)
- Score: **505** (faixa azul — crédito regular)
- Renda média comprovada: R$ 8.581,67
- Dívidas recorrentes: R$ 3.941,33
- Capacidade: R$ 0 (dívidas maiores que limite 30%)
- Alerta: "Dívidas recorrentes representam 45,9% da renda"

---

## Copiloto IA — Análises e Insights

### Briefing de Cliente (exemplo gerado pelo Claude)
> **João da Silva, profissional liberal**, 42 anos aproximados, renda estimada via OF R$ 8.580/mês.
> **Histórico**: cadastro há 30 dias, sem visitas ainda, sem propostas ativas. Cliente em estágio Lead do funil.
> **Interesses detectados**: dados de contato completos, origem não-informada — provavelmente interesse casual.
> **Objeções**: nenhuma detectada ainda (sem histórico de interações).
> **→ Próxima ação**: Ligar para qualificar interesse e agendar visita. Cliente tem capacidade financeira limitada (45% da renda em dívidas), considere tipologias mais acessíveis (2Q Garden).

### Fila priorizada do corretor
- 8 ações no máximo por fila
- Prioridades: Urgente (vermelho), Alta (amarelo), Média (cinza)
- Cada item: ação acionável + justificativa + link direto ao cliente
- Empty state: "Tudo em dia! Nenhuma ação crítica detectada agora."

---

## WhatsApp Business (Meta Cloud API)

### Tela 22 — Configuração de números por corretor
- Admin/Gerente cadastra N números no `/api/whatsapp/numeros`
- Cada número mapeado a um corretor específico (ou compartilhado em round-robin)
- Templates aprovados pelo Meta (registrados em `/api/whatsapp/templates`)

### Tela 23 — Envio de template e timeline
- Card WhatsApp no cliente: timeline estilo WhatsApp com bolhas
- Status de entrega: ✓ enviada, ✓✓ entregue, ✓✓ azul lida
- Envio de texto livre dentro da janela 24h (Meta regra)
- Auto-resposta IA opcional quando corretor offline

### Webhook inbound
- Meta Cloud API → `/api/webhooks/whatsapp`
- Handshake GET com verify_token
- Mensagens recebidas criam **Lead automaticamente** se número desconhecido
- Distribuição round-robin por menor carga recente

---

## Gestão de Usuários (apenas Admin)

### Tela 24 — CRUD de usuários
![Usuarios](./17-usuarios-admin.png)

- Tabela com nome, email, role, CRECI, % comissão, status, último login
- Dialog "Novo usuário": nome, email, senha (min 8 chars), role (Admin/Gerente/Corretor), CRECI, percentual de comissão
- Botão "Inativar" (soft delete) — preserva FK em Vendas/Propostas históricas
- Troca de senha revoga todos os refresh tokens do usuário

---

## RBAC e Segurança

### Tela 25 — Corretor sem acesso a Usuários
![Corretor Block](./18-corretor-blocked.png)

- Sidebar do Corretor não mostra "Usuários"
- Tentar `/usuarios` direto: Alert "Somente administradores podem acessar esta área"
- Backend retorna HTTP 403 em `/api/usuarios` para Corretor

### Validações de autorização testadas
| Endpoint | Admin | Gerente | Corretor |
|---|---|---|---|
| POST /apartamentos | ✅ | ✅ | 403 |
| POST /empreendimentos | ✅ | ✅ | 403 |
| GET /usuarios | ✅ | ✅ | 403 |
| POST /vendas/{id}/aprovar | ✅ | ✅ | 403 |
| POST /copiloto/briefing | ✅ | ✅ | ✅ |
| GET /precificacao/sugestoes | ✅ | ✅ | 403 |
| POST /analise-credito/... | ✅ | ✅ | ✅ |

---

## Multi-tenant e LGPD

- Filtro global automático por `TenantId` no DbContext (não vaza entre clientes)
- Cada tenant com seu próprio Plano (Starter/Pro/Business) e Assinatura ativa
- `[RequiresPlan(limit: "unidades")]` bloqueia criação quando atinge limite
- Dados de Open Finance: expurgo automático 12 meses (Bacen/LGPD)
- Export de dados do cliente em JSON (Art. 18 LGPD)
- Consentimento LGPD explícito em cadastro + timestamp

---

## Infraestrutura

- **Backend**: ASP.NET Core 9, SQL Server 2022, EF Core 9.0.15, Hangfire para jobs
- **Frontend**: React 19, MUI v7, TanStack Query, Zustand, PWA
- **Clean Architecture**: Domain → Application → Infrastructure → API
- **Observabilidade**: Serilog (console + Seq), Application Insights opcional, Sentry para erros
- **Health checks**: `/api/health/live` e `/api/health/ready`
- **Deploy**: IaC Azure Bicep pronto (`deploy/azure/`)
- **CI/CD**: GitHub Actions com testes unitários + integration tests (Testcontainers)

---

## Bugs corrigidos no último ciclo QA (PR #62)

| # | Severidade | Descrição | Correção |
|---|---|---|---|
| 1 | Crítico | Migrations podiam falhar silenciosamente no startup | Log detalhado + fail-fast |
| 2 | Crítico | Seed não criava Assinatura ativa → 402 em tudo | HasData de Assinatura no seed |
| 3 | Importante | Stack trace em prod — já mascarado | Confirmado no middleware |
| 4+5 | Importante | NovaVendaDialog não auto-preenchia/submetia | Selects controlados + reset() |
| 6 | Menor | Comissões não geradas | Corretor seed com 3% de comissão |
| 7 | Menor | Vite dev server silencioso com port ocupada | `strictPort: true` |
| 8 | Menor | Espelho mostrava só uma torre | Agrupamento por Torre (blocos separados) |

---

## Roadmap próximo

- **Integração Pluggy produção** (hoje stub) — R$ 500/mês + R$ 2 por conexão
- **Integração Meta Cloud API oficial** — via 360dialog ou direto
- **Integração FIPE-ZAP** para benchmark de mercado real na Precificação Dinâmica
- **Copiloto IA com API Key configurada** — hoje funcionando em stub mode
- **DocuSign/ClickSign** para assinatura eletrônica de contrato
- **Análise de rentabilidade** — dashboards financeiros para CFO

---

## Credenciais de demonstração

| Usuário | Role | Senha |
|---|---|---|
| admin@imovelstand.com | Admin | Admin@123 |
| corretor@imovelstand.com | Corretor | Corretor@123 |

**URLs**:
- Frontend: `http://localhost:5173`
- Backend Swagger: `http://localhost:5082/swagger`
- Seq (logs): `http://localhost:5341`

---

🤖 _Gerado via sessão de desenvolvimento orientada a Claude Code. 11 PRs mergeadas cobrindo 5 sprints (27-31) implementando os diferenciais competitivos, + 1 PR de fixes pós-QA._
