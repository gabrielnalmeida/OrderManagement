# OrderManagement

API REST para gerenciamento de pedidos construída com .NET 10, ASP.NET Core Minimal APIs e organização em camadas no estilo Clean Architecture.

O projeto já possui endpoints reais para compradores, produtos e pedidos, além de recursos de apoio para desenvolvimento e operação local, como OpenAPI, Scalar, health checks, validação com FluentValidation e persistência com EF Core + SQL Server.

## Visão Geral

O objetivo da solution é centralizar operações comuns de um fluxo de pedidos, mantendo a separação clara entre domínio, casos de uso, infraestrutura e camada HTTP.

Atualmente a API permite:

- gerenciar compradores
- gerenciar produtos
- criar e consultar pedidos
- atualizar itens de um pedido
- avançar o status de um pedido para processado
- enviar ou cancelar pedidos
- validar payloads com regras explícitas
- expor documentação interativa da API

## Stack Tecnológico

- .NET SDK 10.0.104
- ASP.NET Core Minimal APIs
- MediatR
- FluentValidation
- EF Core com SQL Server
- AutoMapper
- OpenAPI
- Scalar
- NUnit

## Arquitetura E Estrutura Da Solution

O projeto segue uma divisão por responsabilidades:

- `src/Domain`: entidades, enums e regras centrais do domínio
- `src/Application`: comandos, queries, validações, comportamentos de pipeline e contratos
- `src/Infrastructure`: persistência, `DbContext`, cache e inicialização do banco
- `src/Web`: camada HTTP, endpoints Minimal API, versionamento, OpenAPI e tratamento de erros
- `src/Shared`: constantes e artefatos simples compartilhados
- `tests/Application.UnitTests`: projeto de testes automatizados

Estrutura principal:

```text
OrderManagement.sln
src/
  Application/
  Domain/
  Infrastructure/
  Shared/
  Web/
tests/
  Application.UnitTests/
```

## Funcionalidades Disponíveis

### Buyers

- listar compradores
- criar comprador
- atualizar comprador
- remover comprador

### Products

- listar produtos
- consultar produto por id
- criar produto
- atualizar produto
- remover produto

### Orders

- listar pedidos
- filtrar pedidos por comprador, status e intervalo de criação
- consultar pedido por id
- criar pedido com itens
- atualizar itens de um pedido
- processar pedido
- enviar pedido
- cancelar pedido
- remover pedido

## Pré-Requisitos

Para subir a aplicação localmente usando apenas containers, garanta que o ambiente tenha:

- Docker
- Docker Compose V2

O fluxo recomendado neste repositório é executar toda a stack via `docker compose`, sem depender de instalação local de .NET SDK ou SQL Server.

## Configuração Do Ambiente

O projeto já traz um `docker-compose.yml` com os dois serviços necessários para execução local:

- `sqlserver`: banco SQL Server 2022
- `web`: API ASP.NET Core

A connection string usada pela API dentro do container `web` é injetada via variável de ambiente pelo próprio `docker-compose.yml`:

```text
ConnectionStrings__OrderManagementDb=Server=sqlserver,1433;Database=OrderManagementDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False
```

O `src/Web/appsettings.json` continua disponível como configuração padrão para cenários fora do Docker, mas não é necessário alterá-lo para rodar o projeto localmente com `docker compose`.

## Como Executar

### 1. Subir toda a stack

```bash
docker compose up --build -d
```

### 2. Acompanhar os logs da API

```bash
docker compose logs -f web
```

### 3. Parar os containers

```bash
docker compose down
```

Se quiser remover também o volume persistido do SQL Server:

```bash
docker compose down -v
```

Ao iniciar, a aplicação:

- registra os serviços da camada `Application`, `Infrastructure` e `Web`
- aplica migrações do banco
- executa seed inicial quando necessário
- publica a documentação OpenAPI
- expõe a UI do Scalar
- redireciona `/` para `/scalar`

## URLs Úteis

Com a execução via `docker compose`, as URLs mais importantes são:

- `http://localhost:8080`
- `http://localhost:8080/scalar`
- `http://localhost:8080/health`
- `http://localhost:8080/openapi/v1.json`

## Dados Iniciais

Na inicialização, a aplicação popula dados básicos quando o banco está vazio.

Hoje o seed cria:

- 3 compradores iniciais
- 3 produtos iniciais
- 1 pedido inicial com itens

Isso facilita o teste imediato da API e do arquivo `src/Web/Web.http`.

## Versionamento E Convenções Das Rotas

Os endpoints HTTP usam versionamento por segmento de URL. As rotas seguem o padrão:

```text
/api/v1/<Recurso>
```

Os grupos atualmente expostos são:

- `/api/v1/Buyers`
- `/api/v1/Products`
- `/api/v1/Orders`

## Endpoints Da API

### Buyers

| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/v1/Buyers` | Lista todos os compradores |
| `POST` | `/api/v1/Buyers` | Cria um comprador |
| `PUT` | `/api/v1/Buyers/{id}` | Atualiza um comprador |
| `DELETE` | `/api/v1/Buyers/{id}` | Remove um comprador |

Payload de criação:

```json
{
  "name": "Buyer 1"
}
```

Payload de atualização:

```json
{
  "id": 1,
  "name": "Buyer Atualizado"
}
```

Regras principais:

- `name` é obrigatório
- `name` deve ter no máximo 100 caracteres

### Products

| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/v1/Products` | Lista todos os produtos |
| `GET` | `/api/v1/Products/{id}` | Consulta um produto por id |
| `POST` | `/api/v1/Products` | Cria um produto |
| `PUT` | `/api/v1/Products/{id}` | Atualiza um produto |
| `DELETE` | `/api/v1/Products/{id}` | Remove um produto |

Payload de criação:

```json
{
  "name": "Notebook Pro",
  "description": "Notebook leve para uso corporativo",
  "price": 4999.90
}
```

Payload de atualização:

```json
{
  "id": 1,
  "name": "Notebook Pro",
  "description": "Notebook leve para uso corporativo",
  "price": 4999.90
}
```

Regras principais:

- `name` é obrigatório
- `name` deve ter no máximo 100 caracteres
- `description` pode ser nulo, mas deve ter no máximo 500 caracteres
- `price` deve ser maior que zero

### Orders

| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/v1/Orders` | Lista pedidos |
| `GET` | `/api/v1/Orders/{id}` | Consulta um pedido por id |
| `POST` | `/api/v1/Orders` | Cria um pedido |
| `PUT` | `/api/v1/Orders/{id}` | Atualiza os itens de um pedido |
| `PUT` | `/api/v1/Orders/{id}/process` | Marca um pedido como processado |
| `PUT` | `/api/v1/Orders/{id}/ship` | Marca um pedido como enviado |
| `PUT` | `/api/v1/Orders/{id}/cancel` | Cancela um pedido |
| `DELETE` | `/api/v1/Orders/{id}` | Remove um pedido |

Filtros disponíveis em `GET /api/v1/Orders`:

- `BuyerId`
- `Status`
- `CreatedFrom`
- `CreatedTo`

Exemplo de criação:

```json
{
  "buyerId": 1,
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ]
}
```

Exemplo de atualização:

```json
{
  "id": 1,
  "items": [
    {
      "productId": 1,
      "quantity": 3
    }
  ]
}
```

Status possíveis de pedido:

- `Started`
- `Processed`
- `Shipped`
- `Cancelled`

Regras principais:

- `buyerId` deve existir
- o pedido deve conter pelo menos um item
- cada item precisa de `productId` válido
- cada item precisa de `quantity` maior que zero
- o `id` da URL deve coincidir com o `id` do payload nos updates

## Fluxo Recomendado De Teste

Uma sequência prática para explorar a API localmente:

1. consultar `GET /health`
2. listar compradores em `GET /api/v1/Buyers`
3. listar produtos em `GET /api/v1/Products`
4. criar um pedido em `POST /api/v1/Orders`
5. consultar o pedido criado em `GET /api/v1/Orders/{id}`
6. processar o pedido em `PUT /api/v1/Orders/{id}/process`
7. enviar o pedido em `PUT /api/v1/Orders/{id}/ship`

Se preferir, também é possível cancelar o pedido em vez de enviá-lo, dependendo do cenário que deseja validar.

## Tratamento De Erros

A API utiliza `ProblemDetails` para respostas de erro HTTP.

Isso cobre principalmente:

- falhas de validação
- recursos não encontrados
- inconsistências de request, como divergência entre id da rota e id do payload

As validações de entrada são implementadas com FluentValidation na camada `Application`.

## Testes E Qualidade

Para execução da aplicação localmente, o fluxo principal é via `docker compose`.

Para validações de qualidade do código durante desenvolvimento, os comandos úteis continuam sendo:

```bash
dotnet test OrderManagement.sln
dotnet format --verify-no-changes OrderManagement.sln
```

## Docker Compose

O arquivo `docker-compose.yml` já orquestra a API e o SQL Server com os mapeamentos necessários para desenvolvimento local.

Resumo dos serviços:

- `sqlserver`: exposto em `localhost:1433`
- `web`: exposto em `localhost:8080`

O build da API usa `src/Web/Dockerfile`, sem exigir configuração adicional para o cenário local padrão.

## Arquivo De Requests

O arquivo `src/Web/Web.http` foi pensado para servir como coleção local de testes rápidos da API.

Ao usar `docker compose`, ajuste `@baseUrl` para `http://localhost:8080`.

Ele inclui exemplos para:

- health check
- buyers
- products
- orders
- filtros de pedidos
- transições de status do pedido

## Limitações E Próximos Passos

Alguns pontos ainda podem evoluir conforme o produto amadurecer:

- endurecimento de configuração sensível por ambiente
- ampliação da cobertura de testes automatizados
- autenticação e autorização
- regras de negócio adicionais para o ciclo de vida do pedido
- observabilidade e telemetria mais robustas
