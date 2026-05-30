# Happeal Backend

Backend for **Happeal** — a hackathon MVP for faster support intake after blocked bank operations.

Happeal does not replace antifraud logic and does not automatically unblock transactions. The goal is simpler and safer: when an operation is blocked or needs confirmation, the system helps collect the client’s context, generate a structured support summary, and pass the case to the right support/compliance flow.

## What it does

The backend supports two main flows:

1. **Client appeal flow**

   * get the current demo user;
   * get the latest blocked operation;
   * create an appeal case;
   * save client answers;
   * attach mock documents;
   * generate a support summary.

2. **Support dashboard flow**

   * view submitted appeal cases;
   * open full case details;
   * submit a support decision: confirm operation, request more info, keep blocked, or escalate.

## Project structure

```txt
backend/
├── src/
│   ├── ComplianceDashboard/   # appeal flow + support dashboard API
│   └── GatewayApi/            # gateway/payment-related API service
├── compose.yaml               # local Docker setup
└── backend.slnx
```

## Tech stack

* C# / ASP.NET Core
* PostgreSQL
* Entity Framework Core
* Docker Compose
* Swagger / OpenAPI

## Running locally

The easiest way to run the backend is through Docker Compose:

```bash
docker compose up --build
```

This starts:

* `ComplianceDashboard` on `http://localhost:3001`
* `GatewayApi` on `http://localhost:3002`
* PostgreSQL on `localhost:5432`

Swagger for the main dashboard API:

```txt
http://localhost:3001/swagger
```

## Main API endpoints

### Health check

```http
GET /api/health
```

### Client flow

```http
GET /api/me
GET /api/operations/blocked
POST /api/appeal-cases
GET /api/appeal-cases/{caseId}
POST /api/appeal-cases/{caseId}/answers
POST /api/appeal-cases/{caseId}/documents
POST /api/appeal-cases/{caseId}/generate-summary
```

### Support dashboard

```http
GET /api/support/appeal-cases
GET /api/support/appeal-cases/{caseId}
POST /api/support/appeal-cases/{caseId}/decision
```

## Example flow

1. Frontend requests the latest blocked operation.
2. Client starts an appeal case.
3. Client answers a short questionnaire.
4. Optional mock documents are attached.
5. Backend generates a structured summary for support.
6. Support team sees the case in the dashboard.
7. Operator decides what to do next: confirm, request more info, keep blocked, or escalate.

## Notes

This is an MVP built for a hackathon demo. Some parts are intentionally simplified:

* no real authentication;
* mock user and seeded demo data;
* mock document upload instead of real file storage;
* support decisions are demo behavior, not real banking actions.

The important part is the process: Happeal shows how a bank can reduce manual support intake after blocked operations while keeping the final decision inside the existing bank process.
