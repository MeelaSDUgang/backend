# AGENTS.md — Backend Instructions

## Project Context

We are building a hackathon MVP for a banking false-positive block support flow.

The product is NOT an anti-fraud engine.
The product is NOT an AI risk scoring system.
The product does NOT automatically unblock accounts.

The product is a **support intake backend** for blocked operations and blocked account appeals.

Main idea:

> When a client’s operation or account is blocked, the backend stores the appeal case, saves clarification answers and documents, generates a structured support summary, and exposes data to the support dashboard.

The main MVP scenario:

1. A client has a blocked operation.
2. The client confirms that they made the operation.
3. The client answers clarification questions.
4. The client uploads a mock document.
5. The backend creates a structured appeal case.
6. The support dashboard receives the case.
7. A support operator can make a decision:

   * confirm operation;
   * request more info;
   * keep blocked;
   * escalate.

Do NOT build real fraud detection.
Do NOT build AML logic.
Do NOT build real unblocking.
Do NOT build risk scoring.

---

## Recommended Tech Stack

Use the existing project stack if already created.

If starting from scratch, use:

* Node.js
* TypeScript
* NestJS or Express
* PostgreSQL or SQLite
* Prisma ORM
* Zod for validation if useful
* Swagger/OpenAPI if there is time

For hackathon speed, SQLite + Prisma is acceptable.

If backend is already in another stack, keep the same domain model and endpoints.

---

## Main Domain

The backend should support two flows:

### 1. Blocked operation confirmation

Example:

```txt
Operation: 250 000 ₸
Recipient: Алишер М.
Status: PENDING_CONFIRMATION
Reason: CLIENT_CONFIRMATION_REQUIRED
```

The client confirms the operation, explains the payment purpose, provides recipient relation, and attaches proof.

### 2. Blocked account appeal

This is secondary for MVP.

Example:

```txt
Account status: LIMITED
Reason: ACCOUNT_RESTRICTION
Client explains recent activity and source of funds.
```

Prioritize the first flow.

---

## Database Models

Create these models.

### User

```ts
id: string
fullName: string
phone: string
accountStatus: "ACTIVE" | "LIMITED" | "BLOCKED"
createdAt: Date
updatedAt: Date
```

Example seed:

```ts
{
  id: "user_1",
  fullName: "Андрей К.",
  phone: "+7 777 000 00 00",
  accountStatus: "LIMITED"
}
```

---

### Operation

```ts
id: string
userId: string
amount: number
currency: "KZT"
recipientName: string
recipientAccount?: string
status: "SUCCESS" | "BLOCKED" | "PENDING_CONFIRMATION"
blockReasonCode:
  | "CLIENT_CONFIRMATION_REQUIRED"
  | "SUSPICIOUS_TRANSFER"
  | "ACCOUNT_RESTRICTION"
createdAt: Date
updatedAt: Date
```

Example seed:

```ts
{
  id: "op_1",
  userId: "user_1",
  amount: 250000,
  currency: "KZT",
  recipientName: "Алишер М.",
  recipientAccount: "KZ00 **** **** 1234",
  status: "PENDING_CONFIRMATION",
  blockReasonCode: "CLIENT_CONFIRMATION_REQUIRED"
}
```

---

### AppealCase

```ts
id: string
userId: string
operationId?: string
caseType:
  | "OPERATION_CONFIRMATION"
  | "ACCOUNT_BLOCK_APPEAL"
status:
  | "DRAFT"
  | "SUBMITTED"
  | "WAITING_SUPPORT"
  | "NEED_MORE_INFO"
  | "RESOLVED"
  | "ESCALATED"
supportSummary?: string
clientMessage?: string
missingInfoJson?: string
routeTo:
  | "SUPPORT"
  | "ANTIFRAUD"
  | "COMPLIANCE"
createdAt: Date
updatedAt: Date
```

Important:

* `supportSummary` is for internal bank staff.
* `clientMessage` is safe text shown to the client.
* `missingInfoJson` can store an array as JSON string for speed.
* `routeTo` is not a final decision. It only says who should handle the case.

---

### AppealAnswer

```ts
id: string
caseId: string
questionKey: string
questionText: string
answer: string
createdAt: Date
```

Example:

```ts
{
  caseId: "case_1",
  questionKey: "client_confirmed_operation",
  questionText: "Вы совершали эту операцию?",
  answer: "Да, это был я"
}
```

---

### AppealDocument

```ts
id: string
caseId: string
documentType:
  | "CHECK"
  | "CONTRACT"
  | "CHAT_SCREENSHOT"
  | "INVOICE"
  | "OTHER"
fileName: string
mockUrl?: string
createdAt: Date
```

For MVP, no real file storage is required.

Just store:

```ts
fileName
documentType
mockUrl
```

Example:

```ts
{
  caseId: "case_1",
  documentType: "CHAT_SCREENSHOT",
  fileName: "olx_chat.png",
  mockUrl: "/mock-files/olx_chat.png"
}
```

---

### SupportDecision

```ts
id: string
caseId: string
decision:
  | "CONFIRM_OPERATION"
  | "REQUEST_MORE_INFO"
  | "KEEP_BLOCKED"
  | "ESCALATE"
comment?: string
createdAt: Date
```

Important:

This does NOT actually unblock anything in a real bank system.

For demo:

* `CONFIRM_OPERATION` can set case status to `RESOLVED`.
* `REQUEST_MORE_INFO` can set case status to `NEED_MORE_INFO`.
* `KEEP_BLOCKED` can set case status to `RESOLVED`.
* `ESCALATE` can set case status to `ESCALATED`.

---

## Prisma Schema Example

If using Prisma, create something close to this:

```prisma
model User {
  id            String      @id
  fullName      String
  phone         String
  accountStatus String
  operations    Operation[]
  appealCases   AppealCase[]
  createdAt     DateTime    @default(now())
  updatedAt     DateTime    @updatedAt
}

model Operation {
  id              String       @id
  userId          String
  amount          Int
  currency        String
  recipientName   String
  recipientAccount String?
  status          String
  blockReasonCode String
  user            User         @relation(fields: [userId], references: [id])
  appealCases     AppealCase[]
  createdAt       DateTime     @default(now())
  updatedAt       DateTime     @updatedAt
}

model AppealCase {
  id              String           @id
  userId          String
  operationId     String?
  caseType        String
  status          String
  supportSummary  String?
  clientMessage   String?
  missingInfoJson String?
  routeTo         String
  user            User             @relation(fields: [userId], references: [id])
  operation       Operation?        @relation(fields: [operationId], references: [id])
  answers         AppealAnswer[]
  documents       AppealDocument[]
  decisions       SupportDecision[]
  createdAt       DateTime         @default(now())
  updatedAt       DateTime         @updatedAt
}

model AppealAnswer {
  id           String     @id
  caseId       String
  questionKey  String
  questionText String
  answer       String
  case         AppealCase @relation(fields: [caseId], references: [id])
  createdAt    DateTime   @default(now())
}

model AppealDocument {
  id           String     @id
  caseId       String
  documentType String
  fileName     String
  mockUrl      String?
  case         AppealCase @relation(fields: [caseId], references: [id])
  createdAt    DateTime   @default(now())
}

model SupportDecision {
  id        String     @id
  caseId    String
  decision  String
  comment   String?
  case      AppealCase @relation(fields: [caseId], references: [id])
  createdAt DateTime   @default(now())
}
```

---

## API Endpoints

Implement exactly these endpoints for MVP.

---

## 1. Health Check

```http
GET /api/health
```

Response:

```json
{
  "status": "ok"
}
```

---

## 2. Get Current User

```http
GET /api/me
```

For demo, return seeded user.

Response:

```json
{
  "id": "user_1",
  "fullName": "Андрей К.",
  "phone": "+7 777 000 00 00",
  "accountStatus": "LIMITED"
}
```

No auth is required for MVP.

---

## 3. Get Blocked Operation

```http
GET /api/operations/blocked
```

Return the main demo operation.

Response:

```json
{
  "id": "op_1",
  "userId": "user_1",
  "amount": 250000,
  "currency": "KZT",
  "recipientName": "Алишер М.",
  "recipientAccount": "KZ00 **** **** 1234",
  "status": "PENDING_CONFIRMATION",
  "blockReasonCode": "CLIENT_CONFIRMATION_REQUIRED",
  "createdAt": "2026-05-30T10:30:00.000Z"
}
```

If multiple blocked operations exist, return the latest one.

---

## 4. Create Appeal Case

```http
POST /api/appeal-cases
```

Body:

```json
{
  "operationId": "op_1",
  "caseType": "OPERATION_CONFIRMATION"
}
```

Behavior:

* Find operation.
* Create appeal case.
* Default status: `DRAFT`.
* Default routeTo: `SUPPORT`.
* Use operation.userId as userId.

Response:

```json
{
  "id": "case_1",
  "userId": "user_1",
  "operationId": "op_1",
  "caseType": "OPERATION_CONFIRMATION",
  "status": "DRAFT",
  "routeTo": "SUPPORT",
  "answers": [],
  "documents": [],
  "createdAt": "2026-05-30T10:35:00.000Z",
  "updatedAt": "2026-05-30T10:35:00.000Z"
}
```

Validation:

* `operationId` is required for `OPERATION_CONFIRMATION`.
* `caseType` must be one of:

  * `OPERATION_CONFIRMATION`
  * `ACCOUNT_BLOCK_APPEAL`

---

## 5. Get Appeal Case

```http
GET /api/appeal-cases/:caseId
```

Return full case with:

* user;
* operation;
* answers;
* documents;
* decisions.

Response example:

```json
{
  "id": "case_1",
  "userId": "user_1",
  "operationId": "op_1",
  "caseType": "OPERATION_CONFIRMATION",
  "status": "DRAFT",
  "routeTo": "SUPPORT",
  "supportSummary": null,
  "clientMessage": null,
  "missingInfo": [],
  "user": {
    "id": "user_1",
    "fullName": "Андрей К.",
    "phone": "+7 777 000 00 00",
    "accountStatus": "LIMITED"
  },
  "operation": {
    "id": "op_1",
    "amount": 250000,
    "currency": "KZT",
    "recipientName": "Алишер М.",
    "status": "PENDING_CONFIRMATION",
    "blockReasonCode": "CLIENT_CONFIRMATION_REQUIRED"
  },
  "answers": [],
  "documents": [],
  "decisions": []
}
```

Convert `missingInfoJson` to `missingInfo` array in the response.

---

## 6. Save Appeal Answers

```http
POST /api/appeal-cases/:caseId/answers
```

Body:

```json
{
  "answers": [
    {
      "questionKey": "client_confirmed_operation",
      "questionText": "Вы совершали эту операцию?",
      "answer": "Да, это был я"
    },
    {
      "questionKey": "payment_purpose",
      "questionText": "За что был перевод?",
      "answer": "Покупка товара"
    },
    {
      "questionKey": "recipient_relation",
      "questionText": "Кем вам является получатель?",
      "answer": "Продавец товара"
    }
  ]
}
```

Behavior:

* Delete previous answers for this case or upsert by `questionKey`.
* Save all provided answers.
* Keep case status as `DRAFT`.

Response:

```json
{
  "ok": true
}
```

Validation:

* `answers` must be a non-empty array.
* Each answer requires:

  * `questionKey`
  * `questionText`
  * `answer`

---

## 7. Add Appeal Document

```http
POST /api/appeal-cases/:caseId/documents
```

Body:

```json
{
  "documentType": "CHAT_SCREENSHOT",
  "fileName": "olx_chat.png"
}
```

Behavior:

* No real file upload required.
* Create document row.
* Set `mockUrl` to:

```txt
/mock-files/{fileName}
```

Response:

```json
{
  "id": "doc_1",
  "caseId": "case_1",
  "documentType": "CHAT_SCREENSHOT",
  "fileName": "olx_chat.png",
  "mockUrl": "/mock-files/olx_chat.png",
  "createdAt": "2026-05-30T10:40:00.000Z"
}
```

Validation:

`documentType` must be one of:

```txt
CHECK
CONTRACT
CHAT_SCREENSHOT
INVOICE
OTHER
```

---

## 8. Generate Support Summary

```http
POST /api/appeal-cases/:caseId/generate-summary
```

This endpoint prepares the internal summary for support.

For hackathon MVP, this can be deterministic rule-based logic.
Do NOT require real LLM integration.

Behavior:

* Load case with operation, answers, documents, user.
* Build `supportSummary`.
* Build `clientMessage`.
* Build `missingInfo`.
* Decide `routeTo`.
* Set status to `SUBMITTED`.

Response:

```json
{
  "caseId": "case_1",
  "status": "SUBMITTED",
  "routeTo": "ANTIFRAUD",
  "supportSummary": "Клиент подтвердил операцию на 250 000 ₸. Указал назначение: покупка товара. Получатель — продавец товара. Клиент приложил документ: olx_chat.png. Требуется проверка приложенного подтверждения и принятие решения по операции.",
  "missingInfo": [
    "Чек оплаты или договор купли-продажи отсутствует"
  ],
  "clientMessage": "Спасибо. Мы передали ваши ответы и документы специалисту. Если потребуется дополнительная информация, мы сообщим в приложении."
}
```

Important:

Do NOT say:

```txt
Клиент честный
Клиент мошенник
Можно автоматически разблокировать
Risk score
```

Only summarize.

---

## Summary Generation Rules

Use these rules.

### Get answer values by keys:

```ts
client_confirmed_operation
payment_purpose
payment_purpose_other
recipient_relation
proof_document
```

### If client said “Нет, это не я”

Then:

```ts
routeTo = "ANTIFRAUD"
status = "SUBMITTED"
supportSummary = "Клиент сообщил, что не совершал операцию..."
clientMessage = "Мы передали информацию в службу безопасности. Ограничение будет сохранено до проверки."
```

### If client said “Да, это был я”

Then generate:

```txt
Клиент подтвердил операцию на {amount} ₸. Указал назначение: {paymentPurpose}. Получатель — {recipientRelation}. {documentsPart}. Требуется проверка приложенной информации и принятие решения по операции.
```

### Documents part

If documents exist:

```txt
Клиент приложил документ: {fileName}.
```

If no documents:

```txt
Подтверждающие документы не приложены.
```

### Missing info rules

If no documents:

```txt
Не приложены подтверждающие документы
```

If payment purpose is `Покупка товара` and no CHECK or CONTRACT:

```txt
Чек оплаты или договор купли-продажи отсутствует
```

If recipient relation is `Не знаю получателя лично`:

```txt
Клиент не знает получателя лично
```

### Route rules

```ts
if client_confirmed_operation includes "Нет":
  routeTo = "ANTIFRAUD"

else if recipient_relation includes "Продавец" or recipient_relation includes "Не знаю":
  routeTo = "ANTIFRAUD"

else:
  routeTo = "SUPPORT"
```

For account block appeal:

```ts
routeTo = "COMPLIANCE"
```

---

## 9. Get Support Cases

```http
GET /api/support/appeal-cases
```

Return all submitted/support-visible cases.

Include cases with statuses:

```txt
SUBMITTED
WAITING_SUPPORT
NEED_MORE_INFO
RESOLVED
ESCALATED
```

Response:

```json
[
  {
    "id": "case_1",
    "clientName": "Андрей К.",
    "caseType": "Подтверждение операции",
    "amount": "250 000 ₸",
    "recipientName": "Алишер М.",
    "status": "SUBMITTED",
    "routeTo": "ANTIFRAUD",
    "summary": "Клиент подтвердил операцию на 250 000 ₸. Указал назначение: покупка товара. Получатель — продавец товара. Клиент приложил документ: olx_chat.png."
  }
]
```

Formatting:

* Amount should be returned as string for frontend convenience.
* Use Russian case type labels:

  * `OPERATION_CONFIRMATION` → `Подтверждение операции`
  * `ACCOUNT_BLOCK_APPEAL` → `Ограничение счёта`

---

## 10. Get Support Case Details

```http
GET /api/support/appeal-cases/:caseId
```

Return full case for dashboard detail page.

Response:

```json
{
  "id": "case_1",
  "client": {
    "id": "user_1",
    "fullName": "Андрей К.",
    "phone": "+7 777 000 00 00",
    "accountStatus": "LIMITED"
  },
  "operation": {
    "id": "op_1",
    "amount": 250000,
    "currency": "KZT",
    "recipientName": "Алишер М.",
    "recipientAccount": "KZ00 **** **** 1234",
    "status": "PENDING_CONFIRMATION",
    "blockReasonCode": "CLIENT_CONFIRMATION_REQUIRED"
  },
  "caseType": "OPERATION_CONFIRMATION",
  "status": "SUBMITTED",
  "routeTo": "ANTIFRAUD",
  "answers": [
    {
      "questionKey": "client_confirmed_operation",
      "questionText": "Вы совершали эту операцию?",
      "answer": "Да, это был я"
    }
  ],
  "documents": [
    {
      "id": "doc_1",
      "documentType": "CHAT_SCREENSHOT",
      "fileName": "olx_chat.png",
      "mockUrl": "/mock-files/olx_chat.png"
    }
  ],
  "supportSummary": "Клиент подтвердил операцию...",
  "missingInfo": [
    "Чек оплаты или договор купли-продажи отсутствует"
  ],
  "clientMessage": "Спасибо. Мы передали ваши ответы и документы специалисту.",
  "decisions": []
}
```

---

## 11. Submit Support Decision

```http
POST /api/support/appeal-cases/:caseId/decision
```

Body:

```json
{
  "decision": "CONFIRM_OPERATION",
  "comment": "Клиент подтвердил операцию, документы приложены"
}
```

Allowed decisions:

```txt
CONFIRM_OPERATION
REQUEST_MORE_INFO
KEEP_BLOCKED
ESCALATE
```

Behavior:

* Create SupportDecision row.
* Update case status:

```ts
CONFIRM_OPERATION -> RESOLVED
REQUEST_MORE_INFO -> NEED_MORE_INFO
KEEP_BLOCKED -> RESOLVED
ESCALATE -> ESCALATED
```

* For demo only:

  * If `CONFIRM_OPERATION`, operation status can be changed from `PENDING_CONFIRMATION` to `SUCCESS`.
  * If `KEEP_BLOCKED`, operation status can be changed to `BLOCKED`.

Response:

```json
{
  "ok": true,
  "caseStatus": "RESOLVED",
  "operationStatus": "SUCCESS"
}
```

Important:

This is demo behavior only.
Do not describe it as a real bank unblock.

---

## Seed Data

Create seed data for these cases.

### Seed User 1

```ts
{
  id: "user_1",
  fullName: "Андрей К.",
  phone: "+7 777 000 00 00",
  accountStatus: "LIMITED"
}
```

### Seed Operation 1 — Main Demo

```ts
{
  id: "op_1",
  userId: "user_1",
  amount: 250000,
  currency: "KZT",
  recipientName: "Алишер М.",
  recipientAccount: "KZ00 **** **** 1234",
  status: "PENDING_CONFIRMATION",
  blockReasonCode: "CLIENT_CONFIRMATION_REQUIRED"
}
```

### Seed Support Case 2 — Account Restriction

```ts
{
  id: "case_2",
  userId: "user_2",
  operationId: null,
  caseType: "ACCOUNT_BLOCK_APPEAL",
  status: "NEED_MORE_INFO",
  routeTo: "COMPLIANCE",
  supportSummary: "Клиент указал, что ограничение появилось после нескольких входящих переводов. Подтверждающие документы пока не приложены.",
  missingInfoJson: "[\"Нужно подтверждение происхождения средств\", \"Нужно описание назначения входящих переводов\"]"
}
```

### Seed Support Case 3 — Simple Confirmation

```ts
{
  id: "case_3",
  userId: "user_3",
  operationId: "op_3",
  caseType: "OPERATION_CONFIRMATION",
  status: "SUBMITTED",
  routeTo: "SUPPORT",
  supportSummary: "Клиент подтвердил оплату услуги. Получатель — компания/сервис. Приложен чек оплаты.",
  missingInfoJson: "[]"
}
```

Also create matching users and operations for case 2 and case 3.

---

## Validation Rules

Use strict validation.

### Case type

Allowed:

```txt
OPERATION_CONFIRMATION
ACCOUNT_BLOCK_APPEAL
```

### Case status

Allowed:

```txt
DRAFT
SUBMITTED
WAITING_SUPPORT
NEED_MORE_INFO
RESOLVED
ESCALATED
```

### Route to

Allowed:

```txt
SUPPORT
ANTIFRAUD
COMPLIANCE
```

### Document type

Allowed:

```txt
CHECK
CONTRACT
CHAT_SCREENSHOT
INVOICE
OTHER
```

### Decision

Allowed:

```txt
CONFIRM_OPERATION
REQUEST_MORE_INFO
KEEP_BLOCKED
ESCALATE
```

Return `400 Bad Request` on invalid values.

---

## Error Handling

Use clean error responses.

Example:

```json
{
  "error": "VALIDATION_ERROR",
  "message": "Invalid documentType"
}
```

Common errors:

```txt
NOT_FOUND
VALIDATION_ERROR
CASE_ALREADY_SUBMITTED
INTERNAL_ERROR
```

Status codes:

```txt
200 OK
201 Created
400 Bad Request
404 Not Found
500 Internal Server Error
```

For hackathon, do not over-engineer auth errors.

---

## CORS

Enable CORS for frontend.

Allow:

```txt
http://localhost:3000
http://localhost:5173
```

If unsure, allow all origins for hackathon demo only.

---

## No Auth For MVP

Do not implement real authentication.

For client endpoints:

* Assume current user is `user_1`.

For support endpoints:

* No login required.

This is acceptable for hackathon MVP.

---

## API Response Style

Use predictable JSON.

Do not return deeply inconsistent structures.

Good:

```json
{
  "id": "case_1",
  "status": "SUBMITTED",
  "routeTo": "ANTIFRAUD"
}
```

Bad:

```json
{
  "data": {
    "case": {
      "case_status_value": "submitted"
    }
  }
}
```

Keep naming camelCase.

---

## File Upload

For MVP, use mock upload.

Do NOT build real multipart file upload unless everything else is done.

The endpoint accepts JSON:

```json
{
  "documentType": "CHAT_SCREENSHOT",
  "fileName": "olx_chat.png"
}
```

Later, if needed, this can be replaced with real file upload.

---

## Support Summary Is Not AI Decision

Important product rule:

The summary generator must never make a final decision.

Correct wording:

```txt
Требуется проверка приложенной информации и принятие решения по операции.
```

Incorrect wording:

```txt
Операцию можно безопасно разблокировать.
```

Correct:

```txt
Клиент сообщил, что совершал операцию.
```

Incorrect:

```txt
Клиент точно совершал операцию и не является мошенником.
```

---

## Demo Flow To Support

The backend must support this full demo path:

1. Frontend calls:

```http
GET /api/operations/blocked
```

2. Frontend calls:

```http
POST /api/appeal-cases
```

3. Frontend calls:

```http
POST /api/appeal-cases/case_1/answers
```

4. Frontend calls:

```http
POST /api/appeal-cases/case_1/documents
```

5. Frontend calls:

```http
POST /api/appeal-cases/case_1/generate-summary
```

6. Frontend calls:

```http
GET /api/support/appeal-cases
```

7. Frontend calls:

```http
GET /api/support/appeal-cases/case_1
```

8. Frontend calls:

```http
POST /api/support/appeal-cases/case_1/decision
```

This path must work before adding any extra features.

---

## Postman / Curl Test Examples

### Health

```bash
curl http://localhost:3001/api/health
```

### Get blocked operation

```bash
curl http://localhost:3001/api/operations/blocked
```

### Create appeal case

```bash
curl -X POST http://localhost:3001/api/appeal-cases \
  -H "Content-Type: application/json" \
  -d '{
    "operationId": "op_1",
    "caseType": "OPERATION_CONFIRMATION"
  }'
```

### Save answers

```bash
curl -X POST http://localhost:3001/api/appeal-cases/case_1/answers \
  -H "Content-Type: application/json" \
  -d '{
    "answers": [
      {
        "questionKey": "client_confirmed_operation",
        "questionText": "Вы совершали эту операцию?",
        "answer": "Да, это был я"
      },
      {
        "questionKey": "payment_purpose",
        "questionText": "За что был перевод?",
        "answer": "Покупка товара"
      },
      {
        "questionKey": "recipient_relation",
        "questionText": "Кем вам является получатель?",
        "answer": "Продавец товара"
      }
    ]
  }'
```

### Add document

```bash
curl -X POST http://localhost:3001/api/appeal-cases/case_1/documents \
  -H "Content-Type: application/json" \
  -d '{
    "documentType": "CHAT_SCREENSHOT",
    "fileName": "olx_chat.png"
  }'
```

### Generate summary

```bash
curl -X POST http://localhost:3001/api/appeal-cases/case_1/generate-summary
```

### Get support cases

```bash
curl http://localhost:3001/api/support/appeal-cases
```

### Submit support decision

```bash
curl -X POST http://localhost:3001/api/support/appeal-cases/case_1/decision \
  -H "Content-Type: application/json" \
  -d '{
    "decision": "CONFIRM_OPERATION",
    "comment": "Клиент подтвердил операцию, документы приложены"
  }'
```

---

## Implementation Priority

Build in this exact order:

1. Project setup
2. Database schema
3. Seed data
4. `GET /api/health`
5. `GET /api/me`
6. `GET /api/operations/blocked`
7. `POST /api/appeal-cases`
8. `POST /api/appeal-cases/:caseId/answers`
9. `POST /api/appeal-cases/:caseId/documents`
10. `POST /api/appeal-cases/:caseId/generate-summary`
11. `GET /api/support/appeal-cases`
12. `GET /api/support/appeal-cases/:caseId`
13. `POST /api/support/appeal-cases/:caseId/decision`
14. CORS
15. Swagger/Postman collection if time remains

Do not start optional features before the main demo flow works.

---

## What NOT To Build

Do NOT build:

* Real anti-fraud
* Risk score
* ML model
* AML checks
* Graph of transactions
* Real OCR
* Real file storage
* Real bank integration
* Real card/account unblock
* KYC
* Authentication
* Role management
* Notifications
* WebSockets
* Complex admin panel
* Microservices

This is a hackathon MVP. Keep it narrow.

---

## Final Product Message

The backend supports this product message:

> We do not replace bank anti-fraud systems.
> We reduce support workload by collecting structured context from the client before the case reaches a human operator.

In Russian:

```txt
Мы не принимаем решение за банк. Мы помогаем быстрее подготовить обращение: собираем ответы клиента, документы и краткое резюме для специалиста.
```

---

## Success Criteria

The backend is successful if:

* The frontend can complete the blocked operation appeal flow.
* The support dashboard can display submitted cases.
* The support detail page has all answers, documents, summary, and missing info.
* The operator can submit a decision.
* No real fraud decision is claimed.
* The demo can run fully from seeded data.

The winning backend is not complex.

The winning backend is stable, predictable, and demo-ready.
