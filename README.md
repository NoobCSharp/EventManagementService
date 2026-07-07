# EventManagementService

Сервис управления мероприятиями на ASP.NET Core Web API

## Архитектура системы

Решение построено по принципам Clean Architecture и разделено на 3 микросервиса и одну общую библиотеку:

### 1. EventManagement Identity

Отвечает за регистрацию пользователей, аутентификацию и выдачу JWT.

   - База данных: PostgreSQL (users)
   - API: http://localhost:5433
   - Контекст: IdentityDbContext

### 2. EventManagement Events

Отвечает за управление событиями.

   - База данных: PostgreSQL (events)
   - API: http://localhost:5434
   - Контекст: EventsDbContext

### 3. EventManagement Bookings

Отвечает за создание и обработку бронирований.

   - База данных: PostgreSQL (bookings)
   - API: http://localhost:5435
   - Контекст: BookingsDbContext

### 4. EventManagement Shared

Shared Kafka Library вынесена в отдельную общую библиотеку (EventManagement.Shared.Kafka).

Содержит:

Kafka Producer / Consumer абстракции
Базовый Consumer BackgroundService
Конфигурацию брокера
Общие топики и константы

Используется всеми сервисами для интеграции с Kafka.

------------------------------------------------------------------------

## Kafka потоки:

 1. BookingCreated
   - Публикует BookingService (Producer)
   - Подписан BookingCreatedKafkaService (Consumer)
   - При получении BookingCreatedKafkaService обрабатывает запрос на создание бронирования: проверяет существование события, возможность бронирования и наличие свободных мест. При успехе резервирует место и публикует BookingCreatedConfirmed, при любой ошибке — BookingCreatedFailed с причиной.

 1.1. BookingCreatedConfirmed
   - Публикует BookingCreatedKafkaService (Producer)
   - Подписан BookingCreatedConfirmedKafkaService (Consumer)
   - При получении BookingCreatedConfirmedKafkaService обрабатывает подтверждение бронирования: находит бронь, переводит её в статус Confirmed и сохраняет изменения. Если бронь отсутствует или уже подтверждена — только записывает информацию в лог.

 1.2. BookingCreatedFailed
   - Публикует BookingCreatedKafkaService (Producer)
   - Подписан BookingCreatedFailedKafkaService (Consumer)
   - При получении BookingCreatedFailedKafkaService обрабатывает отклонение бронирования: находит бронь, переводит её в статус Rejected и сохраняет изменения. Если бронь отсутствует или уже отклонена — записывает информацию в лог.
    
 2. BookingCancelled
   - Публикует BookingService (Producer)
   - Подписан BookingCancelledKafkaService (Consumer)
   - При получении BookingCancelledKafkaService обрабатывает отмену бронирования: проверяет существование события и возможность отмены, освобождает место и публикует BookingCancelledConfirmed. При ошибке публикует BookingCancelledFailed с причиной.

 2.1. BookingCancelledConfirmed
   - Публикует BookingCancelledKafkaService (Producer)
   - Подписан BookingCancelledConfirmedKafkaService (Consumer)
   - При получении BookingCancelledKafkaService обрабатывает подтверждение отмены бронирования: находит бронь, переводит её в статус Cancelled и сохраняет изменения. Если бронь отсутствует или уже отменена — записывает информацию в лог.

 2.2. BookingCancelledFailed
   - Публикует BookingCancelledKafkaService (Producer)
   - Подписан BookingCancelledFailedKafkaService (Consumer)
   - При получении BookingCancelledFailedKafkaService обрабатывает ошибку отмены бронирования: проверяет существование брони и фиксирует неуспешную отмену. В текущей реализации изменения не выполняются (планируется обработка причины ошибки и уведомление пользователя).

------------------------------------------------------------------------

## Требования

Для запуска приложения необходимо:

   - Установленный PostgreSQL (версии 12+ рекомендуется)
   - .NET SDK (рекомендуется .NET 8 и выше)
   - Docker (для Kafka и интеграционных тестов)

## Настройка конфигурации

Пример `appsettings.json` (каждый сервис имеет свой):

``` json
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=events;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your_secret_key",
    "Issuer": "your_issuer",
    "Audience": "your_audience"
  },

  "Kafka": {
    "Producer": {
      "BootstrapServers": "localhost:9092"
    },
    "Consumer": {
      "BootstrapServers": "localhost:9092",
      "ConsumerGroup": "events-service",
      "AutoOffsetReset": "Earliest",
      "EnableAutoOffsetStore": false,
      "EnableAutoCommit": false
    }
```

Рекомендации для production по безопасности:

   - не храните Jwt:Secret в appsettings.json
   - используйте переменные окружения или внешние хранилища
   - используйте длинный случайный ключ (минимум 32–64 символа)

## Управление схемой базы данных (EF Core)

Миграции находятся в `Infrastructure` слоях каждого сервиса.

## Создание миграции

Из корня решения:
``` bash
dotnet ef migrations add InitialCreate --project src/EventManagement.Events.Infrastructure --startup-project src/EventManagement.Events.Api
```

## Применение миграций к базе данных

``` bash
dotnet ef database update --project src/EventManagement.Events.Infrastructure --startup-project src/EventManagement.Events.Api
```

## Удаление последней миграции

``` bash
dotnet ef migrations remove --project src/EventManagement.Events.Infrastructure --startup-project src/EventManagement.Events.Api
```

## Сборка проекта

``` bash
dotnet build
```

## Запуск каждого сервиса отдельно

``` bash
dotnet run --project src/EventManagement.Events.Api
dotnet run --project src/EventManagement.Bookings.Api
dotnet run --project src/EventManagement.Identity.Api
```

## Kafka инфраструктура (Docker)

``` bash
docker-compose up -d
```

## Запуск всех тестов

``` bash
dotnet test
```

------------------------------------------------------------------------

## Интеграционные тесты

Интеграционные тесты используют реальную базу данных `PostgreSQL` в `Docker-контейнере`.

Для запуска интеграционных тестов необходимо:

 1. Установить Docker
 2. Запустить Docker Desktop / Docker Engine

## Запуск интеграционных тестов каждого сервиса

``` bash
dotnet test tests/EventManagementService.Identity.IntegrationTests
dotnet test tests/EventManagementService.Events.IntegrationTests
dotnet test tests/EventManagementService.Bookings.IntegrationTests

```

Интеграционные тесты проверяют:

   - работу репозиториев с PostgreSQL
   - корректность применения миграций
   - взаимодействие сервисов с базой данных

------------------------------------------------------------------------

## Unit-тесты

Для unit-тестов используются:

   - xUnit
   - Moq

Unit-тесты:

   - Не требует реальной базы данных
   - Позволяет быстро выполнять unit-тесты
   - Изолирует тесты друг от друга

## Запуск Unit тестов

``` bash
dotnet test tests/EventManagementService.UnitTests
```

## Тестирование API проекта после запуска

Каждый сервис имеет свой Swagger:

   - Events: http://localhost:5210/swagger
   - Bookings: http://localhost:5085/swagger
   - Identity: http://localhost:5232/swagger

------------------------------------------------------------------------

### API

### EventsController

Основной контроллер обработки запросов по событиям `EventsController`.

Реализует следующие HTTP запросы:

   - GET    `/events` — возвращает список событий
   - GET    `/events/{id}` — возвращает событие по id  
   - POST   `/events` — создает событие
   - PUT    `/events/{id}` — обновляет событие по id
   - DELETE `/events/{id}` — удаляет событие по id

------------------------------------------------------------------------

### Фильтрация (`GET /events`)

 Для фильтрации HTTP запроса GET /events применяются следующие параметры фильтрации:

 1. `Title` (фильтр по названию события, частичное совпадение, регистронезависимый поиск)
 2. `From`  (фильтр по дате начала события, возвращает события, которые начинаются не раньше указанной даты)
 3. `To`    (фильтр по дате окончания события, возвращает события, которые заканчиваются не позже указанной даты)

------------------------------------------------------------------------

### Пагинация (`GET /events`)

Для группировки HTTP запроса GET /events принимает следующие параметры группировки:
 1. `Page`     (Номер страницы)
 2. `PageSize` (Количество событий на странице) 

Пример HTTP запроса GET /events с фильтрацией: GET `/events?title=meeting`

------------------------------------------------------------------------

### BookingController

Основной контроллер обработки запросов по бронированию `BookingController`.

Реализует следующие HTTP запросы:

   - POST   `/events/{id}/book` — создает бронь для события с идентификатором по id
   - GET    `/bookings/{id}` — получает информацию о бронировании по id брони
   - Cancel `/bookings/{id}` — отменяет бронь по id брони

------------------------------------------------------------------------

### AuthenticationController

Основной контроллер обработки запросов регистрации и авторизации пользователей `AuthenticationController`.

Реализует следующие HTTP запросы:

   - POST `/auth/login` — входи и авторизация пользователя, возвращает JWT токен
   - POST `/auth/register` — регистрация нового пользователя

------------------------------------------------------------------------

### Аутентификация и авторизация (JWT)

Для доступа к защищённым методам API необходимо зарегистрировать пользователя и выполнить вход. 
После успешной аутентификации сервер выдаёт JWT-токен, который следует передавать в заголовке каждого защищённого запроса:

```http
Authorization: Bearer <JWT-токен>
```

## Получение JWT-токена через Swagger

После запуска приложения откройте Swagger UI:

```
http://localhost:5232/swagger/index.html
```

### Шаг 1. Зарегистрируйте пользователя

Выполните запрос на регистрацию `POST /auth/register` и передайте необходимые данные:

```json
{
  "login": "test user",
  "password": "your_password",
}
```

После успешной регистрации пользователь будет создан в системе

### Шаг 2. Выполните вход

Выполните запрос авторизации `POST /auth/login` с учётными данными зарегистрированного пользователя:

```json
{
  "login": "test user",
  "password": "your_password"
}
```

В ответе сервер вернёт JWT-токен.

### Шаг 3. Авторизуйтесь в Swagger

1. Нажмите кнопку `Authorize`
2. Введите полученный токен в текстовом формате
3. Нажмите `Authorize`

После этого Swagger будет автоматически добавлять токен в заголовок `Authorization` при выполнении защищённых запросов.

------------------------------------------------------------------------

### Ролевая модель и разграничение прав

В системе реализована ролевая модель на основе claims в JWT.

Доступные роли:
User — базовый пользователь
   - может создавать брони
   - может просматривать свои брони
   - может отменять свои брони
   - может просматривать список событий

Admin — администратор системы
   - полный доступ ко всем операциям
   - может создавать, редактировать и удалять события
   - может просматривать и управлять всеми бронями

------------------------------------------------------------------------

### Обработка ошибок

При ошибках HTTP запросов используется стандартный формат ошибок Problem Details (RFC 7807)
Обеспечивает корректные HTTP-статусы для разных типов ошибок:

 1. `400 Bad Request`           для ошибок валидации или ошибок бронирования события
 2. `403 Forbidden`             для ситуаций, когда пользователь не имеет прав на выполнение операции 
 2. `404 Not Found`             для ситуаций, когда ресурс не найден
 3. `409 Conflict`              для конфликтов при выполнении запроса
 4. `500 Internal Server Error` для непредвиденных ошибок

Пример ответа HTTP запроса GET /events/{id} с несуществующим событием:

``` json
{
  "title": "Resource not found",
  "status": 404,
  "detail": "Событие по указанному Id не найдено!"
}
```

------------------------------------------------------------------------

### Модель Event

Сущность события представлена классом `Event` и содержит следующие поля:

   - `EventId`        (Guid) — уникальный идентификатор события
   - `Title`          (string) — название события
   - `Description`    (string) — описание события
   - `StartAt`        (DateTime) — дата и время начала события
   - `EndAt`          (DateTime) — дата и время окончания события
   - `TotalSeats`     (int) — общее количество мест для бронирования
   - `AvailableSeats` (int) — количество доступных мест для бронирования (при создании события равно `TotalSeats`)

------------------------------------------------------------------------

### Модель Booking

Сущность брони представлена классом `Booking` и содержит следующие поля:

   - `BookingId`   (Guid) — уникальный идентификатор брони
   - `EventId`     (Guid) — идентификатор события, к которому относится бронь
   - `Status`      (BookingStatus) — текущий статус брони (см. ниже)
   - `CreatedAt`   (DateTime) — время создания брони
   - `ProcessedAt` (DateTime) — время, когда бронь была обработана фоновым сервисом (null до обработки)

### BookingStatus

Сущность статусов брони представленна перечислением `BookingStatus` и содержит статусы:

   - `Pending`   (бронь создана и ожидает обработки)
   - `Confirmed` (бронь подтверждена)
   - `Rejected`  (бронь отклонена)
   - `Cancelled` (бронь отменена)

------------------------------------------------------------------------

### Модель User

Сущность пользователя представлена классом `User` и содержит следующие поля:

   - `UserId`       (Guid) — уникальный идентификатор пользователя
   - `Login`        (String) — логин пользователя
   - `PasswordHash` (String) — хэш пароля пользователя
   - `Role`         (Role) — роль пользователя (см. ниже)

### Role

Сущность роли пользователя представлена перечислением `Role` и содержит:

   - `User`   (пользователь с правами на создание и отмены брони)
   - `Admin`  (пользователь с правами на создание, редактирование и удаление событий и броней)

------------------------------------------------------------------------

### Пример сценария

Пример сценария использования `BookingController`:

1. Создать событие:
   - POST `/events` с ответом получает EventId
2. Создать бронь для события:
   - POST `/events/{id}/book`
   
 Ответ: созданный Booking:
   
``` json
{
  "bookingId": "00000000-0000-0000-0000-000000000000",
  "eventId": "11111111-1111-1111-1111-111111111111",
  "status": "Pending",
  "createdAt": "2026-03-30 12:00:00",
  "processedAt": null
}
```

3. Подождать пока сервис `BookingCreatedKafkaService` обработает бронь
4. Получить бронь по Id:
   - `GET /bookings/{id}` теперь status будет `Confirmed` и `ProcessedAt` заполнен

------------------------------------------------------------------------

### Используемые примитивы синхронизации

Для обеспечения корректной работы при конкурентных запросах используется (`SemaphoreSlim`):
   - Гарантирует, что только один поток одновременно изменяет состояние события `AvailableSeats`.

Используется для защиты критической секции:
   - Проверка доступных мест
   - Уменьшение `AvailableSeats`

------------------------------------------------------------------------

### Пример сценария овербукинга

1. Создать событие:
   - POST `/events` с параметром `TotalSeats = 5`, с ответом получает `EventId`
2. Выполнить 10 одновременных запросов на бронирование
   - POST `/events/{id}/book` — создает бронь для события с идентификатором по `EventId`

Без синхронизации:
   - Все 10 потоков читают `AvailableSeats = 5`
   - Все считают, что места есть
   - Создаётся 10 броней (овербукинг)

С синхронизацией (`SemaphoreSlim`):
   - Потоки заходят в критическую секцию по очереди
Первые 5:
   - успешно создают бронирование
   - уменьшают `AvailableSeats`

Остальные 5:
   - получают `409 Conflict`

------------------------------------------------------------------------

### Примечание по архитектуре

   - Kafka вынесена в EventManagement.Shared.Kafka
   - Сервисы общаются асинхронно
   - Базы данных полностью изолированы
