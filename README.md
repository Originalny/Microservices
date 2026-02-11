## Архитектура системы

```
                            ┌─────────────────┐
                            │     Клиент      │
                            └────────┬────────┘
                                     │
                                     ▼
                            ┌─────────────────┐
                            │   API Gateway   │
                            │   :5000         │
                            └────────┬────────┘
                                     │
           ┌─────────────────────────┼─────────────────────────┐
           │                         │                         │
           ▼                         ▼                         ▼
   ┌───────────────┐        ┌───────────────┐        ┌───────────────┐
   │ UsersService  │        │ProductsService│        │ OrdersService │
   │    :5001      │        │    :5002      │        │    :5003      │
   └───────────────┘        └───────────────┘        └───────────────┘
```

---

## Этап 1: Микросервис пользователей (UsersService)

### Назначение
Управление учётными записями пользователей, регистрация и авторизация.

### API эндпоинты

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/api/users` | Список всех пользователей |
| GET | `/api/users/{id}` | Получение пользователя по ID |
| POST | `/api/users` | Регистрация нового пользователя |
| POST | `/api/users/auth` | Авторизация пользователя |

### Порт: `5001`

---

## Этап 2: Микросервис товаров (ProductsService)

### Назначение
Управление каталогом товаров, поиск и управление остатками на складе.

### API эндпоинты

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/api/products` | Список всех товаров |
| GET | `/api/products/{id}` | Получение товара по ID |
| GET | `/api/products/search?q=` | Поиск товаров по названию |
| POST | `/api/products` | Добавление нового товара |
| PUT | `/api/products/{id}/stock` | Изменение остатка на складе |

### Порт: `5002`

---

## Этап 3: Микросервис заказов (OrdersService)

### Назначение
Обработка заказов с валидацией через другие микросервисы.

### API эндпоинты

| Метод | Эндпоинт | Описание |
|-------|----------|----------|
| GET | `/api/orders` | Список всех заказов |
| GET | `/api/orders/{id}` | Детали заказа с данными пользователя и товара |
| POST | `/api/orders` | Создание нового заказа |
| PUT | `/api/orders/{id}/status` | Обновление статуса заказа |

### Взаимодействие с другими сервисами

При создании заказа (`POST /api/orders`):
1. **Проверка пользователя** → GET `/users/{userId}` через Gateway
2. **Проверка товара** → GET `/products/{productId}` через Gateway
3. **Проверка остатка** → Валидация `Stock >= Quantity`
4. **Резервирование товара** → PUT `/products/{id}/stock` с отрицательным Delta

### Порт: `5003`

---

## Этап 4: API Gateway

### Назначение
Единая точка входа для всех клиентских запросов с маршрутизацией к соответствующим микросервисам.

### Таблица маршрутизации

| Путь | Целевой сервис | URL |
|------|----------------|-----|
| `/users/*` | UsersService | `http://localhost:5001` |
| `/products/*` | ProductsService | `http://localhost:5002` |
| `/orders/*` | OrdersService | `http://localhost:5003` |

### Поддерживаемые методы
- **GET** — получение данных
- **POST** — создание ресурсов
- **PUT** — обновление ресурсов
### Порт: `5000`

---

## Этап 5: Демонстрация работы

### Сценарий: Создание заказа

```
1. Клиент → POST /orders
   Body: {"userId": 1, "productId": 2, "quantity": 2}

2. Gateway → OrdersService (5003)

3. OrdersService → Gateway → UsersService
   GET /users/1 ✓ Пользователь найден

4. OrdersService → Gateway → ProductsService
   GET /products/2 ✓ Товар найден, Stock: 25

5. OrdersService → Gateway → ProductsService
   PUT /products/2/stock {"Delta": -2} ✓ Stock: 23

6. OrdersService создаёт заказ
   Response: {"id": 1, "status": "Created", ...}
```

### Пример запросов

```bash
# Получить список пользователей
curl http://localhost:5000/users

# Получить товар
curl http://localhost:5000/products/1

# Создать заказ
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"userId":1,"productId":2,"quantity":2}'

# Получить детали заказа (с данными пользователя и товара)
curl http://localhost:5000/orders/1
```

---

## Структура проекта

```
Microservices/
├── ApiGateway/
│   ├── Program.cs           # Маршрутизация запросов
│   └── ApiGateway.csproj
├── UsersService/
│   ├── Program.cs           # Управление пользователями
│   └── UsersService.csproj
├── ProductsService/
│   ├── Program.cs           # Каталог товаров
│   └── ProductsService.csproj
├── OrdersService/
│   ├── Program.cs           # Обработка заказов
│   └── OrdersService.csproj
```

---

## Запуск системы

### Запуск всех сервисов (в отдельных терминалах)

```bash
# Терминал 1: API Gateway
cd Microservices/ApiGateway && dotnet run

# Терминал 2: UsersService
cd Microservices/UsersService && dotnet run

# Терминал 3: ProductsService
cd Microservices/ProductsService && dotnet run

# Терминал 4: OrdersService
cd Microservices/OrdersService && dotnet run
```

### Проверка работоспособности

```bash
curl http://localhost:5000/
```
