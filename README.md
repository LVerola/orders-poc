# Orders POC 🛒

POC de **gestão de pedidos** desenvolvida como parte de processo seletivo.  
O objetivo é demonstrar domínio em **.NET, React, PostgreSQL, mensageria e boas práticas de arquitetura**.

---

## 📌 Objetivo
- Criar, listar e visualizar pedidos.
- Publicar mensagens no **Azure Service Bus** quando um pedido é criado.
- Consumir mensagens em um **Worker**, atualizando o status do pedido (`Pendente → Processando → Finalizado`).
- Exibir os pedidos em uma interface moderna e responsiva.

---

## 🛠 Tecnologias Utilizadas

### Backend
- [.NET 9](https://dotnet.microsoft.com/pt-br/download) + C#
- Entity Framework Core + Npgsql
- Azure Service Bus SDK
- Health Checks
- SignalR (atualização em tempo real)
- Ollama (IA/Analytics) - integração com LLM local para perguntas em linguagem natural

### Frontend
- [React](https://react.dev/)
- [TailwindCSS](https://tailwindcss.com/)
- Axios / React Query
- SignalR (escuta de eventos em tempo real)

### Infra
- Docker / Docker Compose
- PostgreSQL 15
- PgAdmin
- Ollama (Serviço de IA rodando em container)

---

## 📐 Diagramas

### Arquitetura

![Arquitetura](./docs/diagram.png)

Fluxo:
1. Usuário acessa o **Frontend** (React).
2. Frontend chama a **API Backend** (.NET).
3. API salva no **PostgreSQL** e envia evento ao **Azure Service Bus**.
4. O **Worker** consome a fila, processa o pedido e atualiza o banco.
5. Frontend exibe as mudanças de status em tempo real (ou via refresh).

---

### Modelo de Dados

![Banco de Dados](./docs/db-diagram.png)

Tabela **Orders**:
- `Id` (UUID, PK)
- `Cliente` (string)
- `Produto` (string)
- `Valor` (decimal)
- `Status` (enum: `Pendente`, `Processando`, `Finalizado`)
- `DataCriacao` (datetime)

Tabela **OrderStatusHistory**:
- `Id` (UUID, PK)
- `OrderId` (UUID, FK)
- `Status` (string)
- `DataAlteracao` (datetime)

---

## 🚀 Como Rodar Localmente

### Pré-requisitos
- [.NET 9+](https://dotnet.microsoft.com/pt-br/download)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Passo 1 – Clonar o repositório
```bash
git clone https://github.com/LVerola/orders-poc.git
cd orders-poc
```

### Passo 2 – Variáveis de ambiente

Crie um arquivo .env na raiz com:

```env
# Configuração do PostgreSQL
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=orders
POSTGRES_HOST=postgres

# Azure Service Bus
SERVICEBUS_CONNECTIONSTRING=Endpoint=sb://orderazurebus.xpto...

# PgAdmin
PGADMIN_DEFAULT_EMAIL=admin@admin.com
PGADMIN_DEFAULT_PASSWORD=admin

# SignalR e CORS
FRONTEND_URL=http://localhost:3000
API_URL=http://api:8080
OLLAMA_URL=http://ollama:11434
```

e também dentro de frontend/Orders.Frontend com:

```env
VITE_API_URL=http://localhost:5000
```

### Passo 3 – Subir ambiente com Docker Compose

- Acesse a pasta infra no terminal e depois rode o comando para subir o docker compose

```bash
cd infra
docker compose --env-file ../.env up -d --build
```

### Passo 4 – baixar o modelo do ollama para o IA Analytics

```bash
docker compose exec ollama ollama pull llama3
```

IMPORTANTE:

- Esse passo só precisa ser realizado na primeira vez que rodar o projeto.

### Passo 5 – Acessar o Frontend

Ele ficará disponível em:

- URL: http://localhost:3000

---

## ✅ Endpoints Principais

### POST /orders
Cria um novo pedido.

**Request**
```json
{
  "cliente": "João da Silva",
  "produto": "Notebook",
  "valor": 4500.00
}
```

**Response**

```json
{
  "id": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
  "cliente": "João da Silva",
  "produto": "Notebook",
  "valor": 4500.00,
  "status": "Pendente",
  "dataCriacao": "2025-09-18T14:22:00Z",
  "statusHistories": [
		{
			"id": "019962d1-2c1f-74e1-aaa0-0ab4c3c7c264",
			"orderId": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
			"status": "Pendente",
			"dataAlteracao": "2025-09-18T14:22:00Z"
		}
	]
}
```

---

### GET /orders
Lista todos os pedidos cadastrados.

**Response**

```json
[
  {
    "id": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
    "cliente": "João da Silva",
    "produto": "Notebook",
    "valor": 4500.00,
    "status": "Finalizado",
    "dataCriacao": "2025-09-18T14:22:00Z",
    "statusHistories": [
      {
        "id": "019962d1-2c1f-74e1-aaa0-0ab4c3c7c264",
        "orderId": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
        "status": "Pendente",
        "dataAlteracao": "2025-09-18T14:22:00Z"
      },
      {
        "id": "019962d1-32f3-7f85-b17b-e0b4c246a751",
        "orderId": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
        "status": "Processando",
        "dataAlteracao": "2025-09-19T16:31:41.274118Z"
      },
      {
        "id": "019962d1-46c0-7265-8c48-1574cd2ac4f7",
        "orderId": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
        "status": "Finalizado",
        "dataAlteracao": "2025-09-19T16:31:46.367675Z"
      }
	  ]
  }
]
```

---

### GET /orders/{id}
Retorna os detalhes de um pedido específico.

**Response**

```json
{
  "id": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
  "cliente": "João da Silva",
  "produto": "Notebook",
  "valor": 4500.00,
  "status": "Finalizado",
  "dataCriacao": "2025-09-18T14:22:00Z",
  "statusHistories": [
    {
      "id": "019962d1-2c1f-74e1-aaa0-0ab4c3c7c264",
      "orderId": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
      "status": "Pendente",
      "dataAlteracao": "2025-09-18T14:22:00Z"
    },
    {
      "id": "019962d1-32f3-7f85-b17b-e0b4c246a751",
      "orderId": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
      "status": "Processando",
      "dataAlteracao": "2025-09-19T16:31:41.274118Z"
    },
    {
      "id": "019962d1-46c0-7265-8c48-1574cd2ac4f7",
      "orderId": "a3b9f2b6-8d8c-4e32-8f7f-2cbe0d8f3e21",
      "status": "Finalizado",
      "dataAlteracao": "2025-09-19T16:31:46.367675Z"
    }
  ]
}
```

---

### 📊 Acessando o pgAdmin

O **pgAdmin** já está configurado no `docker-compose.yml`.  
Após subir os serviços com:

```bash
docker compose --env-file ../.env up -d --build
```

Ele ficará disponível em:

- URL: http://localhost:5050

- Usuário: ${PGADMIN_DEFAULT_EMAIL}

- Senha: ${PGADMIN_DEFAULT_PASSWORD}

### 🔗 Conectando ao Postgres no pgAdmin

1. Clique em Add New Server.

2. Em General → Name, coloque um nome (ex: OrdersDB).

3. Em Connection:

    - Host: postgres

    - Port: 5432

    - Username: ${POSTGRES_USER}

    - Password: ${POSTGRES_PASSWORD}

4. Clique em Save.

Agora você poderá navegar pelas tabelas Orders e OrderStatusHistories. 🚀

---

## 🧪 Testes

- **Backend**
  - Testes unitários com **xUnit** para regras de negócio e serviços.
  - Testes de integração com banco de dados via **Docker**.
  - Testes de API simulando chamadas REST (ex.: criação e consulta de pedidos).

- **Frontend**
  - Testes de componentes com **Jest + React Testing Library**.
  - Testes de fluxo principal: criar pedido → listar pedidos → visualizar detalhes.

Para rodar os testes:
```bash
dotnet test
npm test
```
---

## 📈 Diferenciais Técnicos

- [X] Healthchecks implementados (API, Banco e Azure Service Bus).

- [X] Sequência de status obrigatória Pendente → Processando → Finalizado.

- [X] Histórico de status do pedido.

- [X] Outbox Pattern para mensageria transacional.

- [X] SignalR/WebSockets para atualização em tempo real.

- [ ] Testcontainers para integração.

- [X] Módulo IA/Analytics para perguntas em linguagem natural.

---

## 🗂 Estrutura do Projeto

```bash
📦 order-management
 ┣ 📂 backend
 ┃ ┣ 📂 Orders.Api
 ┃ ┃ ┣ 📂 Controllers
 ┃ ┃ ┃ ┣ AnalyticsController.cs
 ┃ ┃ ┃ ┗ OrdersController.cs
 ┃ ┃ ┣ 📂 Migrations
 ┃ ┃ ┣ 📂 Models
 ┃ ┃ ┃ ┣ Order.cs
 ┃ ┃ ┃ ┗ OrderStatusHistory.cs
 ┃ ┃ ┣ 📂 Mocks
 ┃ ┃ ┣ 📂 SignalR
 ┃ ┃ ┃ ┗ OrdersHub.cs
 ┃ ┃ ┣ dockerfile
 ┃ ┃ ┣ appsettings.json
 ┃ ┃ ┣ Orders.Api.csproj
 ┃ ┗ ┗ Program.cs
 ┣ 📂 docs           # Diagramas (arquitetura / banco)
 ┣ 📂 frontend
 ┃ ┣ 📂 Orders.Frontend
 ┃ ┃ ┣ 📂 src
 ┃ ┃ ┃ ┣ 📂 components
 ┃ ┃ ┃ ┃ ┣ Header.tsx
 ┃ ┃ ┃ ┃ ┣ IAChat.tsx
 ┃ ┃ ┃ ┃ ┣ NewOrder.tsx
 ┃ ┃ ┃ ┃ ┣ OrderCard.tsx
 ┃ ┃ ┃ ┃ ┣ OrderDetails.tsx
 ┃ ┃ ┃ ┃ ┗ OrderOverview.tsx
 ┃ ┃ ┃ ┣ 📂 hooks
 ┃ ┃ ┃ ┃ ┣ useOrder.tsx
 ┃ ┃ ┃ ┃ ┗ useOrders.tsx
 ┃ ┃ ┃ ┣ 📂 services
 ┃ ┃ ┃ ┃ ┣ signalr.ts
 ┃ ┃ ┃ ┃ ┗ api.ts
 ┃ ┃ ┃ ┃ App.tsx
 ┃ ┃ ┃ ┃ index.css
 ┃ ┃ ┃ ┗ main.tsx
 ┃ ┃ ┣ .env.example
 ┃ ┃ ┣ package.json
 ┃ ┃ ┗ vite.config.json
 ┣ 📂 infra          # docker-compose.yml
 ┣ 📂 worker
 ┃ ┣ 📂 Orders.Worker
 ┃ ┃ ┣ 📂 Models
 ┃ ┃ ┃ ┣ Order.cs
 ┃ ┃ ┃ ┗ OrderStatusHistory.cs
 ┃ ┃ ┣ dockerfile
 ┃ ┃ ┣ appsettings.json
 ┃ ┃ ┣ Orders.Worker.csproj
 ┃ ┗ ┗ Program.cs
 ┣ README.md
 ┣ .gitignore
 ┗ .env.example
```

---

## 👨‍💻 Autor

- Luis Gabriel Verola Santos
- [LinkedIn](https://www.linkedin.com/in/lverola)
- [Github](https://www.github.com/lverola)