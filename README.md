# dotnet-api-example

PoC: межсервисное взаимодействие .NET 8 через **OpenAPI-спеки + Refitter + Refit** вместо NuGet-пакетов с NSwag-клиентами.

## Контекст и решение

Множество .NET 8 сервисов общаются по HTTP. Раньше — NSwag-клиент публиковался в NuGet, потребитель подключал пакет. Проблемы: уродливый генерированный код, ручная публикация, нет нормальных тестов.

**Новая схема:** провайдер отдаёт `swagger.json`, потребитель кладёт его к себе в репо и генерирует Refit-интерфейс. Никакого NuGet с клиентами.

| | NSwag | Refit + Refitter |
|---|---|---|
| Код клиента | Сотни строк, коммитится | 10 строк интерфейса, в `obj/` |
| Тесты | `MockHttpMessageHandler` | `Substitute.For<IOrdersApi>()` |
| NuGet публикация | Нужна | Не нужна |
| Обновление контракта | Ручной запуск + коммит портянки | Положил `swagger.json`, `dotnet build` |
| Потребители не-.NET | Не используют | Берут ту же JSON-спеку |

## Почему Refit + Refitter, а не NSwag

> NSwag решает задачу «как сделать HTTP-вызов». Refit + Refitter решает ту же задачу, но оставляет в репо читаемый код, убирает ручные шаги и даёт интерфейс который мокается в тестах без танцев с бубном.

### Для разработчиков

**Читаемость.** NSwag генерирует сотни строк partial-классов, хелперов и внутренней кухни. Refit-интерфейс — 10 строк, которые любой прочитает за 30 секунд. Когда что-то сломается в 2 часа ночи — это важно.

**Тесты без боли.** С NSwag нужен `MockHttpMessageHandler`, который мокает на уровне HTTP. С Refit — обычный `Substitute.For<IOrdersApi>()`. Меньше кода, проще читать, легче поддерживать.

**Нет ручного шага.** Цикл с NSwag: скачал спеку → запустил генератор → проверил → закоммитил портянку → опубликовал NuGet. С Refitter — положил `swagger.json`, всё остальное при `dotnet build`.

### Для тимлида / архитектора

**Контракт — явный артефакт.** Сейчас истина живёт в реализации сервиса, спека генерится на лету. Поменял DTO → сломал потребителя → узнал в рантайме. В новой схеме обновление спеки — осознанный коммит, видный в PR. Изменение контракта становится событием, а не случайностью.

**Breaking changes видны до деплоя.** Если добавить `oasdiff` в CI — пайплайн упадёт при попытке задеплоить breaking change. Сейчас этой защиты нет вообще.

**Масштабируется на любых потребителей.** JSON-спека читается любым инструментом — Go, Python, TypeScript сгенерируют клиент из той же спеки. NuGet с NSwag-клиентом полезен только .NET-потребителям.

### Для менеджера

**Меньше времени на рутину.** Разработчики не тратят время на «перегенери клиент и опубликуй NuGet» при каждом изменении API.

**Меньше риск рассинхрона.** Если кто-то забыл обновить клиент — узнаёт в проде. В новой схеме старая спека = старый клиент = компилятор или тесты поймают несовместимость до деплоя.

**Низкая цена перехода.** Refitter понимает те же OpenAPI-спеки что и NSwag. Миграция — замена генератора и регистрации клиента. Один сервис можно перевести за день.

---

## Архитектура

```
Клиент
  │
  │  POST /api/orders
  ▼
ApiGateway  :5062
  ├── OrdersController        ← свой контроллер (публичный контракт)
  │     └── IOrdersApi        ← Refit-клиент из OrdersApi.Client.*
  │           └──────────────────────► OrdersApi :5041 (внутренний)
  │
  └── YARP                    ← прозрачный проброс
        └────────────────────────────► ProductsApi :5250 (внутренний)
```

**Gateway имеет свой контроллер для Orders** — это его публичный контракт. `IOrdersApi` используется как транспорт. Gateway контролирует что выставляет наружу и может добавлять авторизацию, логирование, трансформацию.

ProductsApi проксируется через YARP прозрачно — Gateway не знает о его контракте.

## Структура

```
dotnet-api-example/
├── dotnet-api-example.slnx
└── src/
    ├── OrdersApi/                     # внутренний сервис, :5041
    ├── ProductsApi/                   # внутренний сервис, :5250
    ├── OrdersApi.Client.Generated/    # клиент — Refitter.MSBuild (генерация при сборке)
    │   ├── .refitter
    │   └── swagger.json               # версионированный контракт, коммитится
    ├── OrdersApi.Client.Cli/          # клиент — Refitter CLI (Output.cs коммитится)
    │   ├── .refitter
    │   ├── swagger.json
    │   ├── generate.ps1               # скрипт регенерации
    │   └── Generated/
    │       └── Output.cs              # коммитится в репо
    └── ApiGateway/                    # публичный, :5062
```

## Два подхода к генерации клиента

Оба проекта дают одинаковый namespace `OrdersApi.Client` и одинаковый интерфейс `IOrdersApi`. ApiGateway переключается между ними одной строкой в `ApiGateway.csproj`.

### OrdersApi.Client.Generated — Refitter.MSBuild

Генерация происходит **автоматически при каждом `dotnet build`**. Файл `obj/refitter/Output.cs` не коммитится (покрыт стандартным `obj/` в `.gitignore`).

```xml
<!-- ApiGateway.csproj -->
<ProjectReference Include="..\OrdersApi.Client.Generated\OrdersApi.Client.Generated.csproj" />
```

Обновление контракта:
1. Скачать `swagger.json` из `http://localhost:5041/swagger/v1/swagger.json`
2. Заменить `src/OrdersApi.Client.Generated/swagger.json`
3. `dotnet build` — клиент пересоздаётся автоматически

#### Почему MSBuild, а не Refitter.SourceGenerator

Roslyn **inter-generator isolation**: `CreateSyntaxProvider` одного source generator не видит `AddSource()` другого. Refit 10.x требует compile-time фабрику (reflection fallback удалён).

`Refitter.MSBuild` записывает физический `.cs` файл **до компилятора** (`BeforeTargets="CoreCompile"`). Refit SG видит его как обычный исходный файл и генерирует фабрику.

### OrdersApi.Client.Cli — Refitter CLI

`Output.cs` генерируется вручную и **коммитится в репо**. При `dotnet build` генерации нет — компилируется обычный `.cs` файл.

```xml
<!-- ApiGateway.csproj -->
<ProjectReference Include="..\OrdersApi.Client.Cli\OrdersApi.Client.Cli.csproj" />
```

Установка Refitter CLI (один раз):
```powershell
dotnet tool install -g refitter
```

Обновление контракта:
1. Скачать `swagger.json` из `http://localhost:5041/swagger/v1/swagger.json`
2. Заменить `src/OrdersApi.Client.Cli/swagger.json`
3. Запустить регенерацию:
   ```powershell
   .\src\OrdersApi.Client.Cli\generate.ps1
   # или вручную:
   cd src/OrdersApi.Client.Cli
   dotnet refitter --settings-file .refitter
   ```
4. Закоммитить `Generated/Output.cs` — diff покажет изменения контракта

## Запуск

```powershell
# Собрать всё
dotnet build dotnet-api-example.slnx

# Запустить сервисы (каждый в отдельном терминале)
dotnet run --project src/OrdersApi      # http://localhost:5041
dotnet run --project src/ProductsApi    # http://localhost:5250
dotnet run --project src/ApiGateway     # http://localhost:5062
```

Swagger UI:
- OrdersApi: http://localhost:5041/swagger
- ProductsApi: http://localhost:5250/swagger
- ApiGateway: http://localhost:5062/swagger

## API

### OrdersApi (`/api/orders`)

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/api/orders` | Список заказов; фильтры: `status`, `customerId` |
| `POST` | `/api/orders` | Создать заказ |
| `GET` | `/api/orders/{id}` | Получить заказ |
| `DELETE` | `/api/orders/{id}` | Отменить заказ |
| `PUT` | `/api/orders/{id}/status` | Обновить статус |

Статусы: `Pending → Confirmed → Shipped`, `Cancelled` из любого состояния.

### ProductsApi (`/products`, `/categories`)

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/products` | Список; фильтры: `categoryId`, `inStock`, `search` |
| `POST` | `/products` | Добавить продукт |
| `GET` | `/products/{id}` | Получить продукт |
| `PUT` | `/products/{id}` | Обновить продукт |
| `PUT` | `/products/{id}/stock` | Обновить остаток |
| `GET` | `/categories` | Список категорий |

### Публичный роутинг Gateway

- `/api/orders/**` — Gateway's `OrdersController` → Refit → OrdersApi
- `/api/products/**`, `/api/categories/**` — YARP → ProductsApi

## Тестирование

Refitter генерирует интерфейс — мокается напрямую без `MockHttpMessageHandler`:

```csharp
var ordersApi = Substitute.For<IOrdersApi>();
ordersApi.GetOrders(Arg.Any<string>(), Arg.Any<Guid?>())
    .Returns(new List<OrderSummaryDto> { ... });

var sut = new OrdersController(ordersApi);
```

## Resilience

```csharp
builder.Services.AddRefitClient<IOrdersApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(...))
    .AddStandardResilienceHandler();  // retry + circuit breaker + timeout
```

`AddStandardResilienceHandler()` из `Microsoft.Extensions.Http.Resilience`. `IHttpClientFactory` под капотом `AddRefitClient<>()` — socket exhaustion исключён.

## Когда схема начнёт болеть

| Сигнал | Следующий шаг |
|---|---|
| Одна спека в 5+ репо разъехалась по версиям | Contracts-репо или artifact registry |
| Провайдер сломал контракт, узнали в рантайме | `oasdiff` в CI — проверка breaking changes |
| Появился не-.NET потребитель | Берёт ту же спеку, генерит клиент своим инструментом |
| Gateway разрастается в BFF | Выделить BFF, Gateway оставить чистым роутером |
