# Começando com ImovelStand

Guia em 5 passos para rodar o ImovelStand localmente ou em produção.

## 1. Rodar em dev (local)

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Clonar e subir infra

```bash
git clone https://github.com/gustavograciano/ImovelStand.git
cd ImovelStand
docker compose up -d  # SQL Server + Seq + MinIO
```

### Configurar segredos locais

Edite `ImovelStand.Api/appsettings.Development.json` com:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ImovelStandDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "um-secret-de-pelo-menos-32-caracteres-aqui"
  }
}
```

### Rodar backend

```bash
dotnet run --project ImovelStand.Api
# API em http://localhost:5000, Swagger em /swagger
# Seq logs em http://localhost:5341
# Hangfire em http://localhost:5000/hangfire
```

### Rodar frontend

```bash
cd imovelstand-frontend
npm install
npm run dev
# Acesse http://localhost:5173
```

## 2. Login padrão (seed)

- **Admin**: `admin@imovelstand.com` / `Admin@123`
- **Corretor**: `corretor@imovelstand.com` / `Corretor@123`

## 3. Criar trial (fluxo comercial real)

1. Abra http://localhost:5173/landing
2. Clique em **"Testar grátis por 14 dias"**
3. Preencha os 3 passos do wizard
4. Pronto — você estará logado no app com um empreendimento demo de 48 apartamentos

## 4. Usar as features principais

| Feature | URL | Descrição |
|---|---|---|
| Espelho visual | `/apartamentos` → toggle **Espelho** | Grid por torre/pavimento, cores por status |
| Kanban de funil | `/clientes` | Drag-friendly (futuro) entre Lead → Venda |
| Propostas | `/propostas` | State machine: Rascunho → Enviada → Aceita |
| Dashboard | `/dashboard` | KPIs + funil + ranking |
| Espelho PDF | `GET /api/empreendimentos/{id}/espelho?tipo=Executivo` | 3 templates |

## 5. Deploy em produção

Ver [deploy/azure/README.md](../../deploy/azure/README.md) para provisionar no Azure via Bicep.
