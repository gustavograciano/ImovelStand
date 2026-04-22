# Changelog

Formato [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/), versionamento [SemVer](https://semver.org/lang/pt-BR/).

## [1.0.0] - 2026-04 — Lançamento inicial

### Infraestrutura
- Clean Architecture em 5 projetos (`Domain`, `Application`, `Infrastructure`, `Jobs`, `Api`)
- Serilog → Seq/Application Insights
- CI: backend + frontend + E2E Playwright
- IaC Bicep para Azure (App Service + SQL + Storage + Insights)
- PWA com service worker e cache `NetworkFirst` de `/api/*`

### Domínio
- **Multi-tenant** por `TenantId` com `HasQueryFilter` global e interceptor de auto-atribuição
- **17 entidades**: Tenant, Plano, Usuario, RefreshToken, Empreendimento, Torre, Tipologia, Apartamento, Foto, Cliente, ClienteDependente, HistoricoInteracao, Visita, Reserva, Proposta (+histórico), Venda, Comissao, ContratoTemplate, WebhookSubscription, Assinatura, TemplateNotificacao
- Owned types: `Endereco`, `CondicaoPagamento` (snapshot em Proposta e Venda)

### Features principais
- **Espelho de vendas PDF** (QuestPDF, 3 templates)
- **Contrato DOCX** com template engine + conversão para PDF via Gotenberg
- **CRM completo** com funil Kanban, timeline de interações, LGPD export
- **Proposta com state machine** + contraproposta versionada + calculadora financeira
- **Venda com workflow de aprovação** (Gerente), comissão 1:N, transação atômica
- **Dashboard** com 8 KPIs + funil + ranking + heatmap
- **Jobs Hangfire**: expirar reservas (10min), expirar propostas (diário), lembrete (8h BRT), espelho semanal (sex 18h BRT)
- **Notificações** email (MailKit) + WhatsApp (Z-API) respeitando preferência do cliente
- **Import/export Excel** (tabela de preços, clientes, vendas, funil)
- **Webhooks** com HMAC-SHA256 e retry exponencial
- **Billing Iugu** com trial 14 dias
- **Onboarding wizard** em 3 passos criando tenant + empreendimento demo

### Segurança
- JWT + refresh token com rotação
- BCrypt cost 12, password policy (8 chars, maiúscula/minúscula/dígito/especial)
- Rate limiting em `/auth/login` (10 tentativas/5min/IP)
- `[RequiresPlan]` atributo retorna 402 em limite do plano excedido
- ProblemDetails (RFC 7807) com traceId correlacionável

### Frontend
- TypeScript estrito + MUI v7 + Zustand + TanStack Query
- Páginas: Login, Home, Dashboard, Apartamentos (lista + espelho visual), Clientes (Kanban + detalhe), Propostas, Vendas, Onboarding, Landing
- Code-splitting por rota + manual chunks MUI/React/Query/Forms
- ErrorBoundary + Sentry (opcional)
- PWA instalável

### Testes
- **132 unit tests** (Domain/Application/Api)
- **2 integration tests** (Testcontainers SQL, skip por default)
- **4 E2E smoke tests** (Playwright)

## Releases futuros

Próximas features planejadas (pós-lançamento):
- Tour guiado no app (react-joyride)
- Offline-first completo com IndexedDB e sync
- Integração CRECI (validar registro de corretor)
- Templates de contrato com editor WYSIWYG
- App mobile nativo (React Native) compartilhando API
- Relatórios customizáveis
- BI integração (Metabase/Power BI)
