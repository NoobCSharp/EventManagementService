# EventManagementService
Сервис управления мероприятиями на ASP.NET Core Web API

## Сборка проекта
dotnet build

## Запуск проекта
dotnet run

### Тестирование проекта после запуска
Swagger:
http://localhost:5169/swagger/index.html

#### API
Основной контроллер обработки запросов EventsContoller.
Реализует следующие HTTP запросы:
GET /events — возвращает список событий  
GET /events/{id} — возвращает событие по id  
POST /events — создает событие  
PUT /events/{id} — обновляет событие по id
DELETE /events/{id} — удаляет событие по id

