# OrderFlow

Sistema de gestão de pedidos desenvolvido para o desafio técnico. A aplicação permite criar pedidos com múltiplos produtos, consultar seus detalhes e acompanhar o processamento assíncrono, incluindo um histórico das mudanças de status.

O ambiente de demonstração é iniciado pelo **Docker Compose**, incluindo API .NET, Worker, frontend React, PostgreSQL, pgAdmin, RabbitMQ e a importação automática da topologia de mensageria.

> **Escopo:** configuração voltada a desenvolvimento e demonstração local. O frontend é servido pelo Vite em modo de desenvolvimento. Não se trata de uma configuração de produção.

## Tecnologias e decisões

| Tecnologia                                     | Justificativa                                                                                             |
| ---------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| .NET 10 / ASP.NET Core                         | API REST e processamento em background com C# e tipagem estática.                                         |
| Clean Architecture simplificada e domínio rico | Regras do pedido concentradas no domínio; camadas de aplicação, infraestrutura e transporte separadas.    |
| EF Core 10 + Npgsql                            | Persistência relacional, migrations, mapeamentos explícitos e integração com PostgreSQL.                  |
| PostgreSQL 17                                  | Banco obrigatório no desafio; transações, integridade referencial e concorrência otimista com `xmin`.     |
| RabbitMQ 4                                     | Entrega assíncrona de eventos, confirmação manual de consumo e Dead Letter Queue (DLQ).                   |
| Transactional Outbox                           | Salva pedido e evento pendente na mesma transação, evitando dependência de publicação síncrona no broker. |
| React + TypeScript + Vite                      | Interface tipada com desenvolvimento ágil; React é uma das tecnologias previstas pelo desafio.            |
| Material UI                                    | Componentes reutilizáveis e responsivos para tabelas, formulários e detalhes.                             |
| Docker Compose                                 | Orquestra o ambiente local e facilita reproduzi-lo na avaliação.                                          |
| xUnit + Testcontainers                         | Testes de domínio e integração com PostgreSQL real, em ambiente isolado.                                  |

## Funcionalidades

- `POST /orders`: criação de pedido com cliente e múltiplos itens. O backend calcula o valor total.
- `GET /orders`: listagem de pedidos.
- `GET /orders/{id}`: consulta de detalhes, itens e histórico de status.
- Transições obrigatórias: `Pending → Processing → Finished`, com aproximadamente cinco segundos de processamento no Worker.
- Eventos `OrderCreated` com `CorrelationId = OrderId`, publicados via Outbox.
- Consumer com ACK manual, recuperação de reentregas e DLQ `order-created-dead`.
- Registro de histórico de status e detecção de alterações concorrentes com `xmin`.
- Endpoints `/health/live` e `/health/ready` na API.
- Interface React com listagem responsiva, criação, detalhes e atualização por _polling_ a cada três segundos.

## Arquitetura

Veja [diagramas de arquitetura e sequência](docs/architecture.md).

```text
order-flow/
├── src/
│   ├── OrderFlow.Api/
│   ├── OrderFlow.Application/
│   ├── OrderFlow.Contracts/
│   ├── OrderFlow.Domain/
│   ├── OrderFlow.Infrastructure/
│   └── OrderFlow.Worker/
├── tests/
│   ├── OrderFlow.UnitTests/
│   └── OrderFlow.IntegrationTests/
├── frontend/                 # React + TypeScript + MUI
├── infra/rabbitmq/
│   └── definitions.json     # Filas, exchange, binding e DLQ policy
├── docs/
│   └── architecture.md
├── .env.example
└── docker-compose.yml
```

## Executar o projeto

### Pré-requisitos

- Docker Desktop em funcionamento, com Docker Compose.
- Git para clonar o repositório.
- Navegador moderno.

**Não é necessário instalar .NET nem Node.js no Windows** para executar o ambiente completo pelo Compose. O SDK .NET é necessário somente se quiser executar os testes ou projetos fora do Docker.

### 1. Configurar variáveis

Na raiz do repositório, no PowerShell:

```powershell
Copy-Item .env.example .env
```

Edite `.env` e substitua as senhas de exemplo. Nunca faça commit do `.env` real. Confira que ele está ignorado pelo Git.

O arquivo de exemplo contém variáveis para PostgreSQL, pgAdmin e RabbitMQ. Variáveis do banco e broker são fornecidas à API e ao Worker pelo `docker-compose.yml`.

> Se um volume de banco ou broker já tiver sido inicializado, trocar a senha no `.env` **não altera automaticamente** o usuário armazenado nesse volume. Em ambientes existentes, use as credenciais correspondentes aos dados persistidos.

### 2. Iniciar todos os serviços

```powershell
docker compose config --quiet
docker compose up -d --build
docker compose ps -a
```

O resultado esperado é PostgreSQL, RabbitMQ e API saudáveis (_healthy_); pgAdmin, frontend e Worker executando (_up_); `rabbitmq-init` encerrado com `Exited (0)` após a importação das definições.

> Se algum serviço apresentar falha, consulte `docker compose logs --tail=100 api worker postgres rabbitmq rabbitmq-init frontend` antes de alterar configurações ou apagar volumes.

### 3. Acessar a aplicação

| Serviço             | Endereço local                       |
| ------------------- | ------------------------------------ |
| Frontend React      | <http://localhost:5173>              |
| API                 | <http://localhost:8080>              |
| API: liveness       | <http://localhost:8080/health/live>  |
| API: readiness      | <http://localhost:8080/health/ready> |
| pgAdmin             | <http://localhost:5050>              |
| RabbitMQ Management | <http://localhost:15672>             |

Todos esses serviços usam portas publicadas para `127.0.0.1` conforme o Compose de desenvolvimento.

Na conexão cadastrada **dentro do pgAdmin**, use `postgres` como host e `5432` como porta. Entre containers, a API e o Worker também usam `postgres:5432` e `rabbitmq:5672` em vez de `localhost`.

O navegador faz as requisições para a API em `http://localhost:8080`. O Compose fornece `VITE_API_URL=http://localhost:8080` ao processo do Vite. Essa variável **não é secreta**: variáveis `VITE_*` ficam acessíveis no código cliente.

### 4. Validar o fluxo completo

1. Abra <http://localhost:5173> e confira a tabela de pedidos.
2. Clique em **Novo pedido** e informe um cliente e um ou mais produtos.
3. Confirme que o pedido aparece na listagem.
4. Observe a mudança de `Pending` para `Processing` e depois `Finished`.
5. Abra **Detalhes** para conferir itens, total e histórico.
6. Consulte `/health/live` e `/health/ready` para conferir a API.

O Worker publica os eventos pendentes da Outbox e consome as mensagens do RabbitMQ. A tela consulta a API a cada três segundos, portanto pode haver pequeno atraso visual entre uma transição e sua exibição.

### 5. Encerrar os serviços

```powershell
docker compose down
```

O comando acima interrompe e remove containers/rede, **mas preserva os volumes nomeados**. Não execute `docker compose down -v` se quiser manter pedidos, configurações e dados persistidos.

## Migrations automáticas: verificação necessária

O desafio requer migrations automáticas. A estratégia adotada na dockerização é executar `Database.MigrateAsync()` na inicialização da API, após o PostgreSQL ficar saudável, e liberar o Worker quando a API estiver saudável.

**Antes de declarar esse requisito comprovado**, confirme que o `Program.cs` da API contém a chamada efetiva a `MigrateAsync()` e valide a inicialização em um **banco descartável e isolado**, sem remover os volumes de desenvolvimento atuais. Subir os serviços com um banco já migrado não comprova, sozinho, a criação automática do esquema.

Em aplicações com várias instâncias, migrations concorrentes merecem uma estratégia própria (por exemplo, job de migração antes da subida da API). O fluxo atual é destinado a uma instância no ambiente do desafio.

## API REST

| Método | Rota            | Finalidade                               |
| ------ | --------------- | ---------------------------------------- |
| `POST` | `/orders`       | Criar pedido.                            |
| `GET`  | `/orders`       | Listar pedidos.                          |
| `GET`  | `/orders/{id}`  | Consultar um pedido, itens e histórico.  |
| `GET`  | `/health/live`  | Liveness da API.                         |
| `GET`  | `/health/ready` | Conectividade com PostgreSQL e RabbitMQ. |

Exemplo de criação:

```http
POST http://localhost:8080/orders
Content-Type: application/json
```

```json
{
  "customer": "Maria Silva",
  "items": [
    { "product": "Teclado", "quantity": 2, "unitPrice": 150.0 },
    { "product": "Mouse", "quantity": 1, "unitPrice": 80.0 }
  ]
}
```

O valor total é calculado no domínio: `2 × 150 + 1 × 80 = 380`.

## Testes

Para executar testes pela CLI no Windows, instale o **.NET SDK 10**. Os testes de integração também precisam do Docker Desktop.

```powershell
dotnet test tests/OrderFlow.UnitTests
dotnet test tests/OrderFlow.IntegrationTests
```

Os comandos do frontend podem ser executados sem instalar Node.js no host:

```powershell
docker compose exec frontend npm run lint
docker compose exec frontend npm run build
```

## Mensageria e tolerância a falhas

A API grava o pedido e um evento na Outbox na mesma transação PostgreSQL. Um serviço do Worker publica eventos pendentes na fila `order-created`. Outro serviço consome as mensagens, executa as transições e confirma a mensagem após persistir o processamento.

A topologia do RabbitMQ é versionada em `infra/rabbitmq/definitions.json` e importada automaticamente pelo container `rabbitmq-init`. A policy direciona mensagens rejeitadas, sem requeue, para a DLQ `order-created-dead`.

O estado persistido do pedido permite retomar processamento interrompido e evitar reaplicar transições finalizadas. O mapeamento `xmin` do PostgreSQL protege contra determinadas escritas concorrentes.

## Limitações e melhorias possíveis

- O frontend usa **polling**, não push em tempo real. SignalR com fallback para polling permanece uma melhoria opcional de pontuação adicional.
- O Compose atual usa o **servidor de desenvolvimento Vite**. Uma publicação de produção pode usar build estático e Nginx.
- O ambiente foi validado integrado pelo Compose; a prova de migrations automáticas em **banco limpo isolado** deve ser registrada separadamente.
- Os health checks da API existem; a cobertura de todos os serviços pela orquestração pode ser aprimorada conforme o requisito de infraestrutura.
- IA/Analytics, tracing ponta a ponta e golden tests são diferenciais opcionais não documentados como implementados.

## Segurança

- Não versionar `.env`, senhas, tokens ou arquivos de segredos.
- Configurar credenciais por variáveis de ambiente.
- Não inserir credenciais em variáveis `VITE_*`.
- A configuração Compose e o `ASPNETCORE_ENVIRONMENT=Development` destinam-se à demonstração local.

## Referência

Projeto desenvolvido a partir do documento **Desafio Técnico TMB: Sistema de Gestão de Pedidos**. O README explicita as decisões adicionais, principalmente múltiplos itens por pedido, Transactional Outbox, DLQ, histórico e controle de concorrência.
