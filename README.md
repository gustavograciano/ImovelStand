# ImovelStand - Sistema de Vendas de Apartamentos

Sistema completo para gerenciamento de vendas e reservas de apartamentos em um stand de vendas imobiliário.

## Descrição do Projeto

O ImovelStand é uma aplicação full-stack desenvolvida para gerenciar o processo de vendas de apartamentos, desde o cadastro de clientes até a finalização da compra. O sistema permite que corretores realizem cadastros, consultem disponibilidade, criem reservas e efetuem vendas de forma integrada e segura.

## Tecnologias Utilizadas

### Backend (API)
- **.NET 9** - Framework principal
- **ASP.NET Core Web API** - Para construção da API RESTful
- **Entity Framework Core 9** - ORM para acesso ao banco de dados
- **SQL Server Express** - Banco de dados relacional
- **JWT (JSON Web Token)** - Autenticação e autorização
- **BCrypt.Net** - Hash de senhas
- **Swagger/OpenAPI** - Documentação da API
- **xUnit** - Framework de testes unitários
- **Moq** - Framework para mocking em testes

### Frontend
- **React 18** - Biblioteca JavaScript para construção da interface
- **Vite** - Build tool e dev server
- **Axios** - Cliente HTTP para comunicação com a API
- **CSS3** - Estilização da interface

### DevOps e Infraestrutura
- **Docker** - Containerização da aplicação
- **Docker Compose** - Orquestração de containers
- **Nginx** - Servidor web para o frontend

## Arquitetura do Sistema

```
ImovelStand/
├── ImovelStand.Api/              # API .NET 9
│   ├── Controllers/              # Controllers da API
│   ├── Models/                   # Modelos de dados
│   ├── Data/                     # DbContext e configurações do EF
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Services/                 # Serviços (TokenService, etc)
│   ├── Migrations/               # Migrations do EF Core
│   └── Dockerfile                # Dockerfile da API
├── ImovelStand.Tests/            # Projeto de testes unitários
│   └── Controllers/              # Testes dos controllers
├── imovelstand-frontend/         # Frontend React
│   ├── src/
│   │   ├── services/             # Serviços de API
│   │   ├── App.jsx               # Componente principal
│   │   └── App.css               # Estilos da aplicação
│   ├── Dockerfile                # Dockerfile do frontend
│   └── nginx.conf                # Configuração do Nginx
└── docker-compose.yml            # Orquestração de containers
```

## Estrutura do Banco de Dados

### Tabela: Clientes
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Chave primária (auto-incremento) |
| Nome | varchar(200) | Nome completo do cliente |
| Cpf | varchar(14) | CPF do cliente (único) |
| Email | varchar(100) | Email do cliente (único) |
| Telefone | varchar(15) | Telefone de contato |
| DataCadastro | datetime | Data de cadastro do cliente |

### Tabela: Apartamentos
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Chave primária (auto-incremento) |
| Numero | varchar(100) | Número do apartamento (único) |
| Andar | int | Andar do apartamento |
| Quartos | int | Quantidade de quartos |
| Banheiros | int | Quantidade de banheiros |
| AreaMetrosQuadrados | decimal(10,2) | Área em m² |
| Preco | decimal(18,2) | Preço do apartamento |
| Status | varchar(20) | Status (Disponível, Reservado, Vendido) |
| Descricao | varchar(500) | Descrição do apartamento |
| DataCadastro | datetime | Data de cadastro |

### Tabela: Reservas
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Chave primária (auto-incremento) |
| ClienteId | int | FK para Clientes |
| ApartamentoId | int | FK para Apartamentos |
| DataReserva | datetime | Data da reserva |
| DataExpiracao | datetime | Data de expiração da reserva |
| Status | varchar(20) | Status (Ativa, Expirada, Cancelada, Confirmada) |
| Observacoes | varchar(500) | Observações sobre a reserva |

### Tabela: Vendas
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Chave primária (auto-incremento) |
| ClienteId | int | FK para Clientes |
| ApartamentoId | int | FK para Apartamentos |
| DataVenda | datetime | Data da venda |
| ValorVenda | decimal(18,2) | Valor total da venda |
| ValorEntrada | decimal(18,2) | Valor da entrada paga |
| FormaPagamento | varchar(50) | Forma de pagamento |
| Status | varchar(20) | Status (Concluída, Cancelada) |
| Observacoes | varchar(500) | Observações sobre a venda |

### Tabela: Usuarios
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Chave primária (auto-incremento) |
| Nome | varchar(100) | Nome do usuário |
| Email | varchar(100) | Email (único) |
| SenhaHash | varchar(max) | Hash da senha (BCrypt) |
| Role | varchar(50) | Perfil (Admin, Corretor) |
| DataCadastro | datetime | Data de cadastro |
| Ativo | bit | Indica se o usuário está ativo |

## Como Rodar o Projeto Localmente

### Pré-requisitos
- Docker Desktop instalado e em execução
- Git (opcional, para clonar o repositório)

### Passo a Passo

1. **Clone ou baixe o projeto**
```bash
git clone <url-do-repositório>
cd ImovelStand
```

2. **Inicie os containers com Docker Compose**
```bash
docker-compose up --build
```

Este comando irá:
- Construir as imagens do backend, frontend e banco de dados
- Iniciar o SQL Server Express na porta 1433
- Iniciar a API na porta 5000
- Iniciar o frontend na porta 3000
- Aplicar as migrations automaticamente no banco de dados

3. **Acesse a aplicação**
- Frontend: http://localhost:3000
- API (Swagger): http://localhost:5000/swagger

4. **Para parar os containers**
```bash
docker-compose down
```

## Usuários de Teste

O sistema vem com 2 usuários pré-cadastrados:

| Email | Senha | Role |
|-------|-------|------|
| admin@imovelstand.com | Admin@123 | Admin |
| corretor@imovelstand.com | Corretor@123 | Corretor |

## Endpoints da API

### Autenticação
- `POST /api/auth/login` - Realizar login e obter token JWT

**Request Body:**
```json
{
  "email": "corretor@imovelstand.com",
  "senha": "Corretor@123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "nome": "Corretor Teste",
  "email": "corretor@imovelstand.com",
  "role": "Corretor",
  "expiresAt": "2025-10-12T10:30:00Z"
}
```

### Clientes
- `GET /api/clientes` - Listar todos os clientes
- `GET /api/clientes/{id}` - Buscar cliente por ID
- `POST /api/clientes` - Cadastrar novo cliente
- `PUT /api/clientes/{id}` - Atualizar cliente
- `DELETE /api/clientes/{id}` - Excluir cliente

**Exemplo de Request Body (POST):**
```json
{
  "nome": "João Silva",
  "cpf": "12345678901",
  "email": "joao@email.com",
  "telefone": "11999999999"
}
```

### Apartamentos
- `GET /api/apartamentos` - Listar todos os apartamentos
- `GET /api/apartamentos?status=Disponível` - Filtrar por status
- `GET /api/apartamentos/{id}` - Buscar apartamento por ID
- `POST /api/apartamentos` - Cadastrar novo apartamento
- `PUT /api/apartamentos/{id}` - Atualizar apartamento
- `DELETE /api/apartamentos/{id}` - Excluir apartamento

**Exemplo de Request Body (POST):**
```json
{
  "numero": "301",
  "andar": 3,
  "quartos": 3,
  "banheiros": 2,
  "areaMetrosQuadrados": 85.5,
  "preco": 350000.00,
  "descricao": "Apartamento com 3 quartos, sala ampla e varanda"
}
```

### Reservas
- `GET /api/reservas` - Listar todas as reservas
- `GET /api/reservas/{id}` - Buscar reserva por ID
- `POST /api/reservas` - Criar nova reserva
- `PUT /api/reservas/{id}` - Atualizar reserva
- `DELETE /api/reservas/{id}` - Excluir reserva

**Exemplo de Request Body (POST):**
```json
{
  "clienteId": 1,
  "apartamentoId": 2,
  "observacoes": "Cliente interessado, aguardando análise de crédito"
}
```

### Vendas
- `GET /api/vendas` - Listar todas as vendas
- `GET /api/vendas/{id}` - Buscar venda por ID
- `POST /api/vendas` - Registrar nova venda
- `PUT /api/vendas/{id}` - Atualizar venda
- `DELETE /api/vendas/{id}` - Excluir venda

**Exemplo de Request Body (POST):**
```json
{
  "clienteId": 1,
  "apartamentoId": 2,
  "valorVenda": 350000.00,
  "valorEntrada": 50000.00,
  "formaPagamento": "Financiamento Bancário",
  "observacoes": "Entrada paga, aguardando aprovação do financiamento"
}
```

## Autenticação JWT

Todos os endpoints (exceto `/api/auth/login`) requerem autenticação via JWT Bearer Token.

### Como Gerar o Token

1. Faça login através do endpoint `/api/auth/login`
2. Copie o token retornado no campo `token`
3. Adicione o token no header das requisições:
```
Authorization: Bearer {seu-token-aqui}
```

### Exemplo com cURL
```bash
curl -X GET "http://localhost:5000/api/clientes" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Exemplo com Swagger
1. Acesse http://localhost:5000/swagger
2. Clique no botão "Authorize" no topo
3. Digite: `Bearer {seu-token-aqui}`
4. Clique em "Authorize"

## Testes Unitários

O projeto inclui testes unitários cobrindo os principais fluxos da aplicação.

### Executar os testes
```bash
cd ImovelStand
dotnet test
```

### Cobertura de Testes
- **AuthController**: Testes de login com credenciais válidas e inválidas
- **ClientesController**: Testes de CRUD de clientes, validação de CPF duplicado
- Todos os 8 testes estão passando com sucesso

## Cenário de Uso - Fluxo Completo

### 1. Corretor realiza login
O corretor acessa o sistema e faz login com suas credenciais:
- Email: `corretor@imovelstand.com`
- Senha: `Corretor@123`

### 2. Cadastro do cliente
O corretor cadastra um novo cliente que chegou ao stand:
- Nome: João Silva
- CPF: 12345678901
- Email: joao@email.com
- Telefone: 11999999999

### 3. Consulta de apartamentos disponíveis
O corretor mostra ao cliente os apartamentos disponíveis, filtrando por status "Disponível".

### 4. Cliente se interessa por um apartamento
O corretor verifica que o apartamento 102 está disponível e mostra as características ao cliente.

### 5. Criação de reserva
O cliente decide reservar enquanto analisa o financiamento. O corretor cria uma reserva:
- Cliente: João Silva
- Apartamento: 102
- Observação: "Cliente aguardando aprovação de financiamento"
- O sistema automaticamente:
  - Muda o status do apartamento para "Reservado"
  - Define data de expiração para 7 dias

### 6. Cliente aprova financiamento e decide comprar
Após aprovação do financiamento, o cliente decide finalizar a compra. O corretor registra a venda:
- Valor da venda: R$ 350.000,00
- Valor da entrada: R$ 50.000,00
- Forma de pagamento: Financiamento Bancário
- O sistema automaticamente:
  - Muda o status do apartamento para "Vendido"
  - Cancela todas as reservas ativas deste apartamento
  - Registra a venda como "Concluída"

## Considerações Técnicas e Decisões de Desenvolvimento

### Segurança
1. **Autenticação JWT**: Implementada com token de 8 horas de validade
2. **Hash de Senhas**: Utilização do BCrypt para hash seguro de senhas
3. **CORS Configurado**: Permite comunicação entre frontend e backend
4. **Validação de Dados**: Validações em todos os endpoints da API
5. **TrustServerCertificate**: Configurado para ambiente de desenvolvimento

### Performance e Escalabilidade
1. **Entity Framework Core**: Utilização de índices únicos para CPF, Email e Número do Apartamento
2. **Eager Loading**: Uso de `.Include()` para evitar N+1 queries
3. **DTOs**: Separação de modelos de domínio e transferência de dados
4. **Docker**: Facilita deploy e escalabilidade horizontal

### Boas Práticas
1. **Clean Code**: Código limpo e organizado seguindo princípios SOLID
2. **Separation of Concerns**: Separação clara entre camadas (Controllers, Services, Data)
3. **Testes Unitários**: Cobertura de testes para fluxos críticos
4. **Documentação API**: Swagger/OpenAPI para documentação interativa
5. **Logging**: Sistema de logs configurado para rastreamento de erros
6. **Migrations**: Controle de versão do banco de dados

### Regras de Negócio Implementadas
1. Um apartamento pode ter múltiplas reservas, mas apenas uma ativa por vez
2. Quando uma venda é criada, o status do apartamento muda automaticamente para "Vendido"
3. Reservas têm data de expiração de 7 dias
4. Não é possível excluir clientes ou apartamentos com vendas/reservas associadas
5. CPF e Email de clientes devem ser únicos
6. Número de apartamento deve ser único

## Futuras Melhorias

- Implementação de paginação nos endpoints de listagem
- Sistema de notificações por email
- Relatórios e dashboards analíticos
- Sistema de documentos e contratos
- Integração com sistemas de pagamento
- Histórico de alterações (audit log)
- Upload de fotos dos apartamentos
- Sistema de permissões mais granular
- Testes de integração
- CI/CD pipeline

## Troubleshooting

### Problema: Erro ao conectar no SQL Server
**Solução**: Aguarde alguns segundos após iniciar o docker-compose. O SQL Server demora para inicializar completamente.

### Problema: Porta 5000 ou 3000 já em uso
**Solução**: Altere as portas no `docker-compose.yml` ou pare os serviços que estão utilizando essas portas.

### Problema: Migrations não foram aplicadas
**Solução**: O sistema aplica migrations automaticamente ao iniciar. Se houver problemas, execute manualmente:
```bash
cd ImovelStand.Api
dotnet ef database update
```

### Problema: Token JWT expirado
**Solução**: Faça login novamente para obter um novo token. Os tokens expiram após 8 horas.

## Suporte e Contato

Para dúvidas, sugestões ou reportar bugs, por favor abra uma issue no repositório do projeto.

## Licença

Este projeto foi desenvolvido como parte de um teste técnico e está disponível para fins educacionais.

---

Desenvolvido com .NET 9, React e Docker
