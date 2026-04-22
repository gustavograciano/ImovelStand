# ImovelStand

SaaS multi-tenant de gestão comercial para incorporadoras — espelho de vendas visual, CRM com funil, propostas com state machine, contratos DOCX, dashboard com KPIs.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)
![PWA](https://img.shields.io/badge/PWA-Ready-5A0FC8?logo=pwa&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

---

## 🎯 O que é

O ImovelStand é um SaaS completo para **incorporadoras de 50 a 500 unidades por empreendimento** venderem imóveis sem depender de planilha. Cada cliente (incorporadora) é um **tenant isolado** com:

- Espelho de vendas visual por torre/pavimento (grid colorido por status)
- CRM com funil Kanban, timeline de interações, consentimento LGPD com export
- Propostas com **state machine** + contraproposta versionada + calculadora financeira
- Venda com **workflow de aprovação** de Gerente + comissão 1:N + transação atômica
- Dashboard de KPIs, funil, ranking de corretores, heatmap
- Geração de **espelho PDF** (3 templates) e **contrato DOCX**
- Upload de fotos/plantas com thumbnail automático
- Automação 24/7 via Hangfire (expiração de reservas, lembretes, espelho semanal)
- Integração Iugu para cobrança recorrente + trial 14 dias
- Webhooks com HMAC pra Zapier/n8n
- Import/export Excel (tabela de preços, clientes, vendas, funil)

## 🏗️ Arquitetura

Clean Architecture com 5 projetos .NET + SPA React/TypeScript:

```
ImovelStand/
├── src/
│   ├── ImovelStand.Domain              # entidades, enums, value objects
│   ├── ImovelStand.Application         # services, DTOs, validators, Mapster
│   ├── ImovelStand.Infrastructure      # DbContext, migrations, integrações
│   └── ImovelStand.Jobs                # Hangfire jobs
├── ImovelStand.Api                     # controllers, middlewares, DI
├── tests/
│   ├── ImovelStand.Tests               # unit tests (132)
│   └── ImovelStand.IntegrationTests    # Testcontainers SQL real
├── imovelstand-frontend/               # Vite + React 19 + TypeScript + MUI v7
├── deploy/azure/main.bicep             # IaC produção
└── docs/                               # docs técnicas + legais
```

**Stack completa**
- Backend: ASP.NET Core 9, EF Core 9, SQL Server 2022, JWT+Refresh, BCrypt 12, FluentValidation, Mapster, Serilog→Seq/AppInsights, Sentry (opcional)
- Background jobs: Hangfire com SQL storage
- PDF: QuestPDF · DOCX: DocX (Xceed) + Gotenberg (LibreOffice) · Excel: ClosedXML
- Storage: MinIO (dev) / Azure Blob (prod)
- Notificações: MailKit (SMTP) + Z-API (WhatsApp)
- Billing: Iugu
- Frontend: TypeScript + MUI v7 + Zustand + TanStack Query + React Hook Form + Zod + PWA
- Testes: xUnit + FluentAssertions + Testcontainers + Playwright

## 🚀 Começando

Ver **[docs/content/getting-started.md](docs/content/getting-started.md)** para passo a passo.

```bash
git clone https://github.com/gustavograciano/ImovelStand.git
cd ImovelStand
docker compose up -d           # SQL Server + Seq + MinIO
dotnet run --project ImovelStand.Api    # API em :5000
cd imovelstand-frontend && npm install && npm run dev  # SPA em :5173
```

### Login padrão (seed)

- **Admin**: `admin@imovelstand.com` / `Admin@123`
- **Corretor**: `corretor@imovelstand.com` / `Corretor@123`

### Trial em 2 minutos

Abra `http://localhost:5173/landing` → clique em **"Testar grátis por 14 dias"** → preencha o wizard → você terá tenant novo com empreendimento demo de 48 apartamentos.

## 🧪 Testes

```bash
dotnet test ImovelStand.sln                    # 132 unit tests
npm run test:e2e --prefix imovelstand-frontend # E2E Playwright
```

## 🚢 Deploy em produção

Ver **[deploy/azure/README.md](deploy/azure/README.md)**. Resumo:

```bash
az deployment group create \
  --resource-group imovelstand-prod \
  --template-file deploy/azure/main.bicep \
  --parameters environmentName=prod sqlAdminPassword='...' jwtSecret='...'
```

Custo estimado: **~R$ 200/mês** (B1 + SQL S0 + Storage + App Insights).

## 📚 Documentação

- [Getting started](docs/content/getting-started.md)
- [Arquitetura](docs/content/architecture.md)
- [Deploy Azure](deploy/azure/README.md)
- [Changelog](CHANGELOG.md)
- [Plano de desenvolvimento](PLANO-DESENVOLVIMENTO.md) (histórico)

## 📄 Legal

- [Política de Privacidade (template)](docs/legal/politica-privacidade.md) — revisar com advogado antes de publicar
- [Termos de Uso (template)](docs/legal/termos-uso.md) — revisar com advogado antes de publicar

## 📜 Licença

[Definir com o dono do projeto — MIT / proprietary]
