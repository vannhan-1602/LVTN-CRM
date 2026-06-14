# CRM Online 

> Backend: .NET 8 | Clean Architecture | CQRS | MediatR | EF Core | MySQL | RabbitMQ | JWT
> Frontend: React 18 | Vite | Tailwind CSS | Zustand | React Hook Form
---

## Yêu cầu

| Công cụ            | Phiên bản |
| ------------------ | --------- |
| .NET SDK           | 8.0+      |
| Docker Desktop     | Mới nhất  |
| Visual Studio 2022 | 17.8+     |
| VS Code            | Mới nhất  |
---

## Cài đặt & Chạy

### 1. Clone project

```bash
git clone <url-repo>
cd LVTN
```

### 2. Khởi động MySQL

```bash
docker run -d --name mysql-local \
  -e MYSQL_ROOT_PASSWORD=123456 \
  -e MYSQL_DATABASE=CRMOnline \
  -p 3306:3306 \
  mysql:8.0
```

### 3. Khởi động RabbitMQ

```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

### 4. Import Database

```powershell
Get-Content "CRMOnline.sql" -Raw | docker exec -i mysql-local mysql -uroot -p123456
```

### 5. Chạy Project

Mở file `CRM.slnx` bằng Visual Studio và nhấn **F5**.

### Swagger

```text
https://localhost:7071/swagger
```

---

## Tài khoản mặc định

| Username | Password | Role       |
| -------- | -------- | ---------- |
| admin    | 123456   | Admin      |
| manager  | 123456   | Manager    |
| sale01   | 123456   | Sale       |
| ketoan01 | 123456   | Accountant |

---

## API đã hoàn thành

### Authentication

#### Đăng nhập

```http
POST /api/auth/login
```

Đăng nhập và nhận JWT Token.

#### Danh sách người dùng

```http
GET /api/auth/users
```

Chỉ dành cho Admin.

### Customer Management

#### Lấy danh sách khách hàng

```http
GET /api/customer
```

#### Lấy chi tiết khách hàng

```http
GET /api/customer/{id}
```

#### Tạo khách hàng

```http
POST /api/customer
```

#### Cập nhật khách hàng

```http
PUT /api/customer/{id}
```

#### Xóa khách hàng

```http
DELETE /api/customer/{id}
```

### Lead Management

#### Lấy danh sách Lead

```http
GET /api/lead
```

#### Lấy chi tiết Lead

```http
GET /api/lead/{id}
```

#### Tạo Lead

```http
POST /api/lead
```

#### Cập nhật Lead

```http
PUT /api/lead/{id}
```

#### Xóa Lead

```http
DELETE /api/lead/{id}
```

#### Chuyển Lead thành Customer

```http
POST /api/lead/{id}/convert
```

---

## Kiến trúc dự án

```text
CRM.Domain
├── Entities
├── Enums
└── Interfaces

CRM.Application
├── CQRS
├── Commands
├── Queries
├── Validators
└── Behaviors

CRM.Infrastructure
├── Entity Framework Core
├── Repositories
├── JWT Authentication
└── RabbitMQ

CRM.API
├── Controllers
├── Middleware
├── Swagger
└── Program.cs
```

---

## Công nghệ sử dụng

```text
- ASP.NET Core 8
- Entity Framework Core
- MySQL
- MediatR
- FluentValidation
- AutoMapper
- JWT Authentication
- RabbitMQ
- Swagger / OpenAPI
```

---

## Kiến trúc áp dụng

```text
- Clean Architecture
- CQRS Pattern
- Repository Pattern
- Dependency Injection
- Domain-Driven Design (DDD)
- Event-Driven Architecture
```
