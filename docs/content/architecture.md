# Arquitetura

## Visão geral

```
┌──────────────────┐         ┌──────────────────┐         ┌──────────────────┐
│  React + MUI     │  HTTPS  │  ASP.NET Core 9  │   TCP   │  SQL Server 2022 │
│  (SPA PWA)       │────────▶│  API + Hangfire  │────────▶│  (TDE, backup)   │
└──────────────────┘         └──────────────────┘         └──────────────────┘
         │                            │
         │  PWA cache                 │  Storage API
         ▼                            ▼
┌──────────────────┐         ┌──────────────────┐
│  Service Worker  │         │  Azure Blob      │
│  (offline reads) │         │  (fotos, docs)   │
└──────────────────┘         └──────────────────┘
```

## Camadas (Clean Architecture)

```
src/
├── ImovelStand.Domain           (entidades, enums, value objects, interfaces)
├── ImovelStand.Application      (services, DTOs, validators, Mapster config)
├── ImovelStand.Infrastructure   (DbContext, migrations, interceptors, integrações)
└── ImovelStand.Jobs             (jobs Hangfire)

ImovelStand.Api                  (controllers, middlewares, DI)
tests/
├── ImovelStand.Tests            (unit: 132)
└── ImovelStand.IntegrationTests (Testcontainers SQL)
imovelstand-frontend             (Vite + React + TS)
```

**Regra de dependência**:
`Api → Jobs → Infrastructure → Application → Domain`

Domain não depende de nada; Application só da Domain; Infrastructure implementa interfaces de Application; Api apenas compõe.

## Multi-tenant

Todas as entidades que implementam `ITenantEntity` ganham:

1. **Coluna `TenantId`** (Guid)
2. **HasQueryFilter global** aplicado via reflection no `OnModelCreating` — filtra automaticamente toda query pelo tenant da request
3. **`TenantAssignmentInterceptor`** — auto-preenche `TenantId` em entidades `Added`

Isso garante **defesa em profundidade**: mesmo que o developer esqueça o `TenantId`, o filtro global impede vazamento.

## State machines

- **[PropostaStateMachine](../../src/ImovelStand.Application/Services/PropostaStateMachine.cs)** — transições permitidas; terminal states (Aceita/Reprovada/Expirada/Cancelada) não voltam
- **Venda** — transações atômicas no `VendasController.Aprovar` garantem consistência quando o apartamento muda para Vendido

## Isolamento de plano

Endpoints de criação têm `[RequiresPlan(limit: "unidades")]` etc, que retornam 402 se o tenant excedeu o limite do plano atual (Starter/Pro/Business).

## Observability

- **Serilog** → Seq (dev) ou Application Insights (prod)
- **Sentry** (opcional) captura erros do frontend + exceptions do backend
- **Health checks**:
  - `/api/health/live` — liveness probe
  - `/api/health/ready` — readiness (inclui SQL)
