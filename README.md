\# CRM Online — Backend



> .NET 8 | Clean Architecture | CQRS | MediatR | EF Core | MySQL | RabbitMQ | JWT



\---



\## Yêu cầu



| Công cụ | Phiên bản |

|---------|-----------|

| .NET SDK | 8.0+ |

| Docker Desktop | Mới nhất |

| Visual Studio 2022 | 17.8+ |



\---



\## Cài đặt \& Chạy



\### 1. Clone project

```bash

git clone <url-repo>

cd LVTN

```



\### 2. Khởi động MySQL

```bash

docker run -d --name mysql-local \\

&#x20; -e MYSQL\_ROOT\_PASSWORD=123456 \\

&#x20; -e MYSQL\_DATABASE=CRMOnline \\

&#x20; -p 3306:3306 \\

&#x20; mysql:8.0

```



\### 3. Khởi động RabbitMQ

```bash

docker run -d --name rabbitmq \\

&#x20; -p 5672:5672 \\

&#x20; -p 15672:15672 \\

&#x20; rabbitmq:3-management

```



\### 4. Import database

```bash

Get-Content "CRMOnline.sql" -Raw | docker exec -i mysql-local mysql -uroot -p123456

```



\### 5. Chạy project

Mở `CRM.slnx` bằng Visual Studio → \*\*F5\*\*



Swagger: `https://localhost:7071/swagger`



\---



\## Tài khoản mặc định



| Username | Password | Role |

|----------|----------|------|

| admin | 123456 | Admin |

| manager | 123456 | Manager |

| sale01 | 123456 | Sale |

| ketoan01 | 123456 | Accountant |



\---



\## API đã hoàn thành



\- `POST /api/auth/login` — Đăng nhập, nhận JWT token

\- `GET /api/auth/users` — Danh sách user (Admin)

\- `GET/POST/PUT/DELETE /api/customer` — Quản lý khách hàng

\- `GET/POST/PUT/DELETE /api/lead` — Quản lý lead

\- `POST /api/lead/{id}/convert` — Chuyển lead thành khách hàng



\---



\## Kiến trúc



```

CRM.Domain          ← Entities, Enums, Interfaces

CRM.Application     ← CQRS, Validators

CRM.Infrastructure  ← EF Core, JWT, RabbitMQ, Repositories

CRM.API             ← Controllers, Middleware, Program.cs

```

