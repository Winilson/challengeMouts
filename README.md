# Ambev Developer Evaluation — Sales API

API REST para gestão de vendas (DeveloperStore), implementando CRUD completo com regras de desconto progressivo, padrão DDD + CQRS, arquitetura hexagonal e eventos de domínio.

## Stack

- .NET 8 / ASP.NET Core
- PostgreSQL 13 (via Docker)
- Entity Framework Core 8
- MediatR (CQRS), AutoMapper, FluentValidation, Serilog
- xUnit, FluentAssertions, NSubstitute, Testcontainers

## Como rodar

Pré-requisitos: Docker Desktop instalado e em execução.

```bash
git clone https://github.com/Winilson/challengeMouts.git
cd challengeMouts/template/backend
docker-compose up --build
```

Aguarde 1-2 minutos. Quando aparecer `Now listening on: http://[::]:8080` no log, abra no navegador:

**http://localhost:8080/swagger**

As migrations são aplicadas automaticamente no startup pela API. A extensão `pgcrypto` (necessária para `gen_random_uuid()`) é criada pelo `init.sql` na primeira inicialização do banco.

### Plano B (caso o build falhe baixando imagens da Microsoft)

Alguns provedores bloqueiam `mcr.microsoft.com`. Como alternativa, sobe só o banco em container e roda a API localmente:

```bash
docker-compose up -d ambev.developerevaluation.database
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

Acesse: **http://localhost:5119/swagger**

## Regras de negócio

Descontos progressivos por quantidade de items idênticos no mesmo pedido:

| Quantidade | Desconto |
|---|---|
| 1 a 3 | 0% |
| 4 a 9 | 10% |
| 10 a 20 | 20% |
| acima de 20 | proibido (HTTP 400) |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| POST | /api/Sales | Cria venda com seus items |
| GET | /api/Sales/{id} | Busca venda por id |
| GET | /api/Sales | Lista paginada com filtros e ordenação |
| PUT | /api/Sales/{id} | Atualiza cliente e filial |
| DELETE | /api/Sales/{id} | Cancela venda (cascade nos items) |
| DELETE | /api/Sales/{saleId}/items/{itemId} | Cancela item específico |

### Filtros disponíveis em GET /api/Sales

- `_page` (default: 1)
- `_size` (default: 10)
- `_order` (ex: `date desc, saleNumber asc`)
- `saleNumber`, `customerName`, `branchName` (suportam wildcards com `*`)
- `customerId`, `branchId`, `isCancelled`
- `_minDate`, `_maxDate` para filtrar por intervalo

## Domain Events

A criação, modificação e cancelamento de vendas e items dispara eventos de domínio publicados via MediatR. Os event handlers logam os eventos via Serilog (não publicamos em message broker real conforme escopo do desafio):

- SaleCreated
- SaleModified
- SaleCancelled
- ItemCancelled

## Testes

### Unitários (Domain + Application + ORM)

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit
```

136 casos cobrindo: regras de desconto, agregado Sale, eventos de domínio, todos os handlers CQRS, validators, AutoMapper profiles e SaleRepository via EF Core InMemory.

### Funcionais (end-to-end com Postgres real)

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Functional
```

7 testes que sobem a API real e um Postgres descartável via Testcontainers, fazendo chamadas HTTP de verdade ao Controller. Requer Docker rodando.

### Cobertura

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Arquitetura

```
src/
├── Ambev.DeveloperEvaluation.Domain         Entidades ricas, aggregates, domain events
├── Ambev.DeveloperEvaluation.Application    Handlers MediatR (CQRS), validators, profiles
├── Ambev.DeveloperEvaluation.ORM            Repository + EF Configurations + Migrations
├── Ambev.DeveloperEvaluation.WebApi         Controllers, requests/responses, middlewares
├── Ambev.DeveloperEvaluation.IoC            Registros de injeção de dependência
└── Ambev.DeveloperEvaluation.Common         Cross-cutting (auth JWT, logging, healthcheck)
```

Padrões aplicados:

- DDD com aggregate root `Sale` controlando `SaleItem`
- CQRS via MediatR (separação Commands/Queries)
- External Identities (CustomerId + CustomerName denormalizados, mesmo para Branch e Product)
- Domain Events publicados após persistência
- Repository Pattern com `ISaleRepository`
- Dependency Inversion (Application depende de abstração, ORM implementa)
