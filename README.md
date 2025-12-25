# Nubrio Weather API

REST API для получения прогноза погоды и статистики запросов.

Проект реализован в учебных целях и демонстрирует:
- Clean Architecture
- работу с внешним провайдером погоды (Open-Meteo),
- кеширование,
- логирование HTTP-запросов,
- сбор и агрегацию статистики,
- работу с PostgreSQL через ORM (EF Core),
- контейнеризацию через Docker.

## Возможности

### Прогноз погоды
- прогноз на день
- прогноз на неделю
- поддержка кириллицы и латиницы
- иконки погодных условий

### Кеширование
- in-memory кеш прогнозов
- фиксация cache hit / cache miss
- кеш прозрачен для контроллеров

### Статистика
- логирование всех HTTP-запросов
- подсчёт количества запросов за период
- топ городов по числу запросов
- пагинация
- валидация входных параметров

### Техническое
- PostgreSQL
- EF Core
- FluentResults
- Middleware для логирования
- Docker / docker-compose


## Архитектура

Проект построен по принципам Clean Architecture и разделён на слои:

- **Domain**  
  Бизнес-сущности и enum’ы (без зависимостей)

- **Application**  
  Use cases, сервисы, DTO, интерфейсы

- **Infrastructure**  
  Работа с БД, внешними API, кеш, EF Core

- **Presentation**  
  Controllers, middleware, DTO ответов, маппинг, статические файлы (иконки)


## Запуск через Docker

### Требования
- Docker
- Docker Compose

### Шаги

1. Создать файл `.env` в корне проекта:

```env
#PostgreSql
POSTGRES_DB=<DBNAME>
POSTGRES_USER=<USERNAME>
POSTGRES_PASSWORD=<PASSWORD>

#pgAdmin
PGADMIN_DEFAULT_EMAIL=<EMAIL>
PGADMIN_DEFAULT_PASSWORD=<PASSWORD>
```

2.	Запустить контейнеры:
`docker compose up --build`

3. API будет доступно по адресу:
`https://localhost:8080`

4. Swagger UI:
`https://localhost:8080/swagger`


---

## Локальный запуск (без Docker)

### Требования
- .NET 8 SDK
- PostgreSQL

### Шаги

1. Указать строку подключения:
```bash
dotnet user-secrets set "ConnectionStrings:NubrioDb" "Host=localhost;Port=5432;Database=<DBNAME>;Username=<USERNAME>;Password=<PASSWORD>"
```
2.	Применить миграции:
`dotnet ef database update`

3.	Запустить API:
`dotnet run --project Nubrio.Presentation`

---

## При первом запуске необходимо применить миграции
`dotnet ef database update --project Nubrio.Infrastructure --startup-project Nubrio.Presentation`

---

## 7️⃣ Примеры запросов

## Примеры запросов


### Прогноз на день
`GET /api/weather/Berlin?date=2025-12-22`

### Прогноз на неделю
`GET /api/weather/Berlin/week`

### Топ городов за период
`GET /api/stats/top-cities?from=2025-01-01&to=2025-01-31&limit=10`

### Сырые запросы за период
`GET /api/stats/requests?from=2025-01-01&to=2025-01-31&page=1&pageSize=20`

---

## Особенности реализации

- Логирование запросов реализовано через middleware
- Ошибки валидируются и возвращаются в структурированном виде
- Используется пагинация с ограничением глубины (MaxSkip)
- Строки подключения формируются через переменные окружения
- Иконки погоды раздаются как статические файлы
- Минимальные unit-тесты для:
  - кеша
  - маппинга
  - резолвера иконок

## Стек технологий

- ASP.NET Core 8
- Entity Framework Core
- PostgreSQL
- FluentResults
- Moq + xUnit
- Docker / Docker Compose
