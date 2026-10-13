# Arquitetura do OrderFlow

A aplicação é executada em um ambiente local com Docker Compose: `frontend`, `api`, `worker`, `postgres`, `pgadmin`, `rabbitmq` e `rabbitmq-init`.

## Visão geral

```mermaid
flowchart LR
    U["Navegador"] -->|"Carrega a interface"| F["React + Vite (frontend)"]
    U -->|"HTTP: /orders"| A["ASP.NET Core (api)"]
    A -->|"EF Core / Npgsql"| P[("PostgreSQL")]

    subgraph Worker["OrderFlow.Worker"]
        PUB["OutboxPublisherService"]
        CON["OrderCreatedConsumerService"]
    end

    PUB -->|"Consulta Outbox"| P
    PUB -->|"Publica OrderCreated"| MQ["RabbitMQ: order-created"]
    MQ -->|"Entrega"| CON
    CON -->|"Status + histórico"| P
    MQ -.->|"Mensagem rejeitada"| DLQ["RabbitMQ: order-created-dead"]
    INIT["rabbitmq-init / definitions.json"] -.->|"Configura filas, exchanges e policy"| MQ
    INIT -.-> DLQ
    ADM["pgAdmin"] --> P
```

> O frontend é servido por um container, mas o `fetch()` é executado pelo navegador. Por isso, a URL da API no React é `http://localhost:8080`. Já entre containers, PostgreSQL e RabbitMQ são endereçados por `postgres` e `rabbitmq`.

## Criação e processamento

```mermaid
sequenceDiagram
    autonumber
    participant UI as Navegador / React
    participant API as API .NET
    participant DB as PostgreSQL
    participant PUB as Worker / Publisher
    participant MQ as RabbitMQ
    participant CON as Worker / Consumer

    UI->>API: POST /orders
    API->>DB: Transação: Order Pending + Outbox
    DB-->>API: Commit
    API-->>UI: 201 Created
    PUB->>DB: Buscar mensagens pendentes
    DB-->>PUB: Evento OrderCreated
    PUB->>MQ: Publicar (CorrelationId = OrderId)
    PUB->>DB: Marcar publicação confirmada
    MQ->>CON: Entregar OrderCreated
    CON->>DB: Processing + histórico
    Note over CON: Aguarda aproximadamente 5 segundos
    CON->>DB: Finished + histórico
    CON-->>MQ: ACK após persistir
    loop A cada 3 segundos
        UI->>API: GET /orders ou GET /orders/{id}
        API->>DB: Consultar pedido e histórico
        API-->>UI: Dados atualizados
    end
```

## Orquestração e inicialização

```mermaid
flowchart TD
    UP["docker compose up -d --build"] --> DB["PostgreSQL saudável"]
    UP --> MQ["RabbitMQ saudável"]
    MQ --> INIT["rabbitmq-init finaliza com Exited (0)"]
    DB --> API["API inicia e expõe health check"]
    API --> WORKER["Worker inicia"]
    INIT --> WORKER
    UP --> FRONT["Frontend Vite"]
    UP --> ADMIN["pgAdmin"]
```

**Migrations:** a aplicação automática depende da chamada efetiva a `Database.MigrateAsync()` na inicialização da API. Sua execução em um banco vazio deve ser validada em ambiente isolado antes de registrar o requisito como concluído.

## Decisões técnicas

- **Domínio rico:** `Order` controla mudanças de status e seu histórico.
- **Outbox transacional:** pedido e intenção de publicar o evento são persistidos juntos.
- **Mensageria assíncrona:** RabbitMQ desacopla o recebimento HTTP do processamento.
- **ACK manual e reentrega:** o processamento é confirmado depois da persistência.
- **DLQ:** mensagens rejeitadas podem ser inspecionadas sem bloquear a fila principal.
- **Concorrência otimista:** o mapeamento `xmin` evita sobrescrever silenciosamente determinadas alterações concorrentes.
- **Polling:** o React atualiza a interface a cada três segundos, sem alegar comunicação push em tempo real.
