# ImovelStand

Sistema full-stack para gestão de vendas de apartamentos em stand imobiliário — cadastro de clientes, disponibilidade, reservas e fechamento de vendas.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=black)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Express-CC2927?logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)

## Stack

**Backend** — .NET 9 · ASP.NET Core Web API · Entity Framework Core 9 · SQL Server · JWT Bearer · BCrypt.Net · Swagger/OpenAPI
**Frontend** — React 18 · Vite · Axios · CSS3
**Testes** — xUnit · Moq · EF Core InMemory · Coverlet
**Infra** — Docker · Docker Compose · Nginx (servindo o build do front)

## Arquitetura

```
ImovelStand/
├── ImovelStand.Api/          API REST em .NET 9 (Controllers, Models, DbContext, Services)
├── ImovelStand.Tests/        Testes unitários (xUnit + Moq + EF InMemory)
├── imovelstand-frontend/     SPA React 18 (Vite)
├── HashGenerator/            Utilitário console para gerar hashes BCrypt
├── UpdatePasswords/          Script utilitário de migração de senhas
└── docker-compose.yml        Orquestração API + SQL Server + Frontend
```

### Domínio

`Apartamento`, `Cliente`, `Reserva`, `Venda`, `Usuario` — com controllers RESTful correspondentes e autenticação por JWT no `AuthController`.

## Como rodar localmente

### Pré-requisitos
- .NET 9 SDK
- Node.js 20+
- Docker Desktop (ou SQL Server Express local)

### Subindo tudo via Docker
```bash
docker-compose up --build
```

### Rodando separado (dev)
```bash
# API
cd ImovelStand.Api
dotnet restore && dotnet ef database update && dotnet run

# Frontend
cd imovelstand-frontend
npm install && npm run dev
```

A API expõe Swagger em `/swagger`. O front consome via Axios na URL configurada em `src/services/api.js`.

## Testes

```bash
dotnet test
```

Cobertura via `coverlet.collector` — testes em `ImovelStand.Tests/` usam `Microsoft.EntityFrameworkCore.InMemory` para isolar o DbContext.

## Status

Projeto desenvolvido como exercício full-stack com foco em arquitetura em camadas, autenticação segura (JWT + BCrypt) e empacotamento via containers.
