# AutoRentalApi — ASP.NET Core Web API (.NET 8)

Учебный проект для предметной области «Автопрокат»: бронирование, выдача и возврат автомобилей; JWT-авторизация и роли; аналитика загрузки, просрочек и выручки по тарифам; кэширование аналитики; логирование; CSV-отчёт.

## Запуск
1. Установить .NET 8 SDK.
2. Открыть терминал в папке проекта.
3. Выполнить `dotnet restore`, затем `dotnet run`.
4. В режиме Development открыть адрес Swagger, указанный в терминале (обычно `/swagger`). База `autorental.db` создаётся автоматически вместе с демонстрационными данными.

## Демонстрационные учётные записи
- `admin` / `Admin123!` — полные права;
- `manager` / `Manager123!` — работа с автопарком и арендой;
- `analyst` / `Analyst123!` — аналитика.

Это только учётные записи учебного стенда. Перед публикацией в сети необходимо заменить их, вынести секрет JWT из `appsettings.json` в User Secrets/переменные окружения и настроить HTTPS.

## Основные маршруты
- `POST /api/auth/login` — получить JWT.
- `GET, POST /api/cars` — список и создание автомобилей; `GET /api/cars/{id}`; `DELETE /api/cars/{id}` (Admin).
- `GET, POST /api/tariffs` — тарифы.
- `GET, POST /api/customers` — клиенты.
- `GET, POST /api/bookings` — список и создание броней.
- `POST /api/bookings/{id}/issue` — выдача автомобиля.
- `POST /api/bookings/{id}/return` — возврат автомобиля и расчёт стоимости.
- `POST /api/bookings/{id}/cancel` — отмена подтверждённой брони.
- `GET /api/analytics/utilization?from=2026-10-01T00:00:00Z&to=2026-11-01T00:00:00Z` — загрузка автопарка.
- `GET /api/analytics/overdue` — активные просрочки.
- `GET /api/analytics/revenue-by-tariff?from=2026-10-01T00:00:00Z&to=2026-11-01T00:00:00Z` — выручка по тарифам.
- `GET /api/analytics/report.csv` — выгрузка отчёта в CSV.

## Как вызывать защищённые маршруты
1. Отправить `POST /api/auth/login` с JSON, например `{"username":"admin","password":"Admin123!"}`.
2. Скопировать `accessToken` из ответа.
3. В Swagger нажать **Authorize** и ввести `Bearer <токен>`.

## Пример сценария
1. Выбрать автомобиль `GET /api/cars`, клиента `GET /api/customers` и тариф `GET /api/tariffs`.
2. Создать бронь `POST /api/bookings`:
   `{"carId":1,"customerId":1,"tariffId":1,"plannedStart":"2026-10-20T10:00:00Z","plannedReturn":"2026-10-23T10:00:00Z"}`
3. Оформить выдачу `POST /api/bookings/1/issue`:
   `{"odometerAtIssue":35000,"notes":"Автомобиль выдан без повреждений"}`
4. Оформить возврат `POST /api/bookings/1/return`:
   `{"odometerAtReturn":35320,"notes":"Возврат принят"}`
5. Проверить аналитику через `/api/analytics/...`.

## Соответствие методическим требованиям
- ASP.NET Core Web API и REST-маршруты;
- EF Core и SQLite;
- JWT и роли Admin/Manager/Analyst;
- генерация предметных событий: Booking, RentalIssue, RentalReturn;
- аналитика и MemoryCache;
- ILogger для журналирования;
- CSV-экспорт отчёта.

## Ограничения учебной версии
Используется `EnsureCreated` для быстрого старта. Для реального проекта следует заменить его миграциями EF Core, добавить полноценное управление пользователями, аудит изменений, тесты, транзакционную защиту от параллельного бронирования и уточнить правила тарификации/штрафов за просрочку. Текущая загрузка рассчитывается по фактическому времени выдачи и возврата; просрочка показывает активные аренды, срок возврата которых прошёл.
