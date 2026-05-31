# Happeal Backend

Backend для **Happeal** — hackathon MVP для более быстрого сбора информации после блокировки банковских операций.

Happeal не заменяет антифрод-логику и не разблокирует операции автоматически. Цель решения проще и безопаснее: когда операция заблокирована или требует подтверждения, система помогает собрать контекст от клиента, сформировать структурированное резюме для поддержки и передать кейс в нужный support/compliance flow.

## Что делает проект

Backend поддерживает два основных сценария:

1. **Клиентский appeal flow**

   * получить текущего demo-пользователя;
   * получить последнюю заблокированную операцию;
   * создать appeal case;
   * сохранить ответы клиента;
   * прикрепить mock-документы;
   * сгенерировать summary для поддержки.

2. **Support dashboard flow**

   * посмотреть отправленные appeal cases;
   * открыть полную информацию по кейсу;
   * принять решение по кейсу: подтвердить операцию, запросить больше информации, оставить блокировку или эскалировать.

## Структура проекта

```txt
backend/
├── src/
│   ├── ComplianceDashboard/   # API для appeal flow и dashboard поддержки
│   └── GatewayApi/            # gateway/payment-related API сервис
├── compose.yaml               # локальный Docker setup
└── backend.slnx
```

## Tech stack

* C# / ASP.NET Core
* PostgreSQL
* Entity Framework Core
* Docker Compose
* Swagger / OpenAPI

## Локальный запуск

Проще всего запустить backend через Docker Compose:

```bash
docker compose up --build
```

После запуска будут доступны:

* `ComplianceDashboard` на `http://localhost:3001`
* `GatewayApi` на `http://localhost:3002`
* PostgreSQL на `localhost:5432`

Swagger для основного dashboard API:

```txt
http://localhost:3001/swagger
```

## Основные API endpoints

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

## Пример flow

1. Frontend запрашивает последнюю заблокированную операцию.
2. Клиент создаёт appeal case.
3. Клиент отвечает на короткий опросник.
4. При необходимости прикрепляются mock-документы.
5. Backend генерирует структурированное summary для поддержки.
6. Служба поддержки видит кейс в dashboard.
7. Оператор решает, что делать дальше: подтвердить операцию, запросить больше информации, оставить блокировку или эскалировать.

## Notes

Это MVP, собранный для hackathon demo. Некоторые части намеренно упрощены:

* нет настоящей аутентификации;
* используются mock user и seeded demo data;
* mock-загрузка документов вместо настоящего file storage;
* решения поддержки — demo behavior, а не реальные банковские действия.

Главное в проекте — сам процесс: Happeal показывает, как банк может снизить ручную нагрузку на поддержку после блокировки операций, при этом оставляя финальное решение внутри существующего банковского процесса.
