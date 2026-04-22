# Deploy ImovelStand no Azure

## Pré-requisitos
- Azure subscription ativa
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) logado (`az login`)
- Resource group criado: `az group create -n imovelstand-prod -l brazilsouth`

## Deploy inicial

```bash
az deployment group create \
  --resource-group imovelstand-prod \
  --template-file deploy/azure/main.bicep \
  --parameters \
      environmentName=prod \
      sqlAdminPassword='SENHA_FORTE' \
      jwtSecret='$(openssl rand -base64 48)' \
      iuguApiToken='SEU_TOKEN_IUGU'
```

## O que é provisionado

- **App Service Plan** B1 Linux (US$ ~12/mês)
- **App Service API** (.NET 9) com health check em `/api/health` + alwaysOn
- **App Service Web** (Node 20) para o SPA
- **Azure SQL** Server + Database (S0, US$ ~15/mês em produção real)
- **Storage Account** + container `imovelstand` (fotos/contratos), sem public access
- **Application Insights** + **Log Analytics Workspace** (30 dias retention)
- **Alerta** HTTP 5xx > 10 em 5min → notifica Admin (configure action group depois)

## Backup

Azure SQL tem backup automático por default (PITR 7 dias, weekly full 4 semanas). Para restore:

```bash
az sql db restore \
  --dest-name ImovelStandDb-restored \
  --resource-group imovelstand-prod \
  --server imovelstand-sql-prod \
  --name ImovelStandDb \
  --time '2026-04-22T10:00:00'
```

## Ambientes

Rode com `environmentName=staging` ou `dev` para provisionar ambientes separados — cada um fica num conjunto de recursos isolados.

## Deploy do código

Depois da infra provisionada, `deploy.yml` (GHA) builda imagens Docker e publica no App Service via container (ou via zip deploy com `az webapp deployment source config-zip`).

## Health checks

- `GET /api/health` — versão + status (compat com frontend existente)
- `GET /api/health/live` — probe liveness Kubernetes (só checa se o processo responde)
- `GET /api/health/ready` — probe readiness (inclui ping no SQL Server)

## Custos mensais estimados

| Recurso | SKU | Custo/mês (BRL aprox) |
|---|---|---|
| App Service Plan B1 | Basic | R$ 70 |
| Azure SQL S0 | Standard | R$ 80 |
| Storage Standard LRS | Hot | R$ 10 (para < 100GB) |
| App Insights + Logs | PerGB2018 | R$ 30 (para ~5GB/mês) |
| Tráfego de saída | Primeiros 100GB grátis | R$ 0-30 |
| **Total** | | **~R$ 200/mês** |

Escala pra Standard S2 + P1V3 quando o cliente for 50+ usuários ativos.
