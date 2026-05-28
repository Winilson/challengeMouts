# Ambev Developer Evaluation — Sales API

API REST para gestão de vendas (DeveloperStore), implementando CRUD completo com regras de desconto progressivo, padrão DDD + CQRS, arquitetura hexagonal e eventos de domínio.

## Stack

- .NET 8 / ASP.NET Core
- PostgreSQL 13 (via Docker)
- Entity Framework Core 8
- MediatR (CQRS), AutoMapper, FluentValidation, Serilog
- xUnit / FluentAssertions / NSubstitute / Testcontainers

## Como rodar

```bash
git clone https://github.com/Winilson/challengeMouts.git
cd challengeMouts/template/backend
docker-compose up --build
```

Aguarde 1-2 minutos. Quando aparecer `Now listening on: http://[::]:8080`, abra:

**http://localhost:8080/swagger**

As migrations são aplicadas automaticamente no startup; a extensão `pgcrypto` é criada pelo `init.sql` na primeira inicialização do banco.

### Plano B (se Docker bloqueado no MCR)

```bash
docker-compose up -d ambev.developerevaluation.database
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

Acesse **http://localhost:5119/swagger**

## Regras de negócio (descontos)

| Quantidade | Desconto |
|---|---|
| 1 a 3 | 0% |
| 4 a 9 | 10% |
| 10 a 20 | 20% |
| acima de 20 | proibido (HTTP 400) |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/Sales` | Cria venda |
| GET | `/api/Sales/{id}` | Busca por id |
| GET | `/api/Sales` | Lista paginada com filtros |
| PUT | `/api/Sales/{id}` | Atualiza cliente/filial |
| DELETE | `/api/Sales/{id}` | Cancela venda (cascade) |
| DELETE | `/api/Sales/{saleId}/items/{itemId}` | Cancela item específico |

### Filtros disponíveis na listagem
- `_page`, `_size`, `_order`
- `saleNumber`, `customerName`, `branchName` (com wildcards `*`)
- `customerId`, `branchId`, `isCancelled`
- `_minDate`, `_maxDate`

## Testes

```bash
# Unitários (136 testes)
dotnet test tests/Ambev.DeveloperEvaluation.Unit

# Funcionais com Postgres real (7 testes via Testcontainers)
dotnet test tests/Ambev.DeveloperEvaluation.Functional
```

## Cobertura

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Arquitetura