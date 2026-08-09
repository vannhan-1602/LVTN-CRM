# 🚀 CRM Online

> **CRM Online** là hệ thống quản lý quan hệ khách hàng (Customer Relationship Management) dành cho doanh nghiệp kinh doanh giải pháp phần mềm và dịch vụ CNTT, hỗ trợ quản lý khách hàng, khách hàng tiềm năng, sản phẩm, báo giá, hợp đồng, hóa đơn, thu chi và các hoạt động chăm sóc khách hàng trên cùng một nền tảng.

---

# 📖 Giới thiệu

CRM Online được phát triển với mục tiêu số hóa quy trình quản lý khách hàng, giúp doanh nghiệp theo dõi toàn bộ quá trình từ khi tiếp cận khách hàng tiềm năng đến khi hoàn thành giao dịch và chăm sóc sau bán hàng.

Hệ thống được xây dựng theo mô hình **Clean Architecture** (Domain – Application – Infrastructure – API), tách biệt rõ ràng giữa các tầng nghiệp vụ, dữ liệu và giao diện, giúp dễ dàng mở rộng, bảo trì và phát triển trong tương lai. Phần Application sử dụng mô hình **CQRS** với **MediatR** (Commands/Queries).

---

# ✨ Chức năng chính

- 🔐 Đăng nhập bằng JWT Authentication (không có API tự đăng ký công khai — chỉ Admin mới tạo được tài khoản mới)
- 👤 Quản lý người dùng và phân quyền theo vai trò (Admin, Manager, Sale, Accountant)
- 👥 Quản lý khách hàng, địa chỉ
- 📈 Quản lý khách hàng tiềm năng (Lead) — kèm form tiếp nhận Lead công khai từ landing page
- 💼 Quản lý cơ hội kinh doanh (Opportunity)
- 📦 Quản lý sản phẩm, danh mục
- 📝 Quản lý báo giá (Quotation) — kèm trang xem báo giá công khai qua link riêng
- 📑 Quản lý hợp đồng (Contract), xuất hợp đồng ra PDF
- 💰 Quản lý hóa đơn (Invoice) và phiếu thu chi
- 🎫 Quản lý Ticket hỗ trợ khách hàng — kèm form gửi Ticket công khai
- 🎁 Chương trình khách hàng thân thiết (Loyalty) và Voucher
- 🔔 Cảnh báo (Alert) và ghi nhận hoạt động (Activity log)
- 📊 Dashboard thống kê và phân tích (Analytics)
- 📜 Ghi nhận nhật ký hệ thống (Audit Log) qua hàng đợi RabbitMQ
- ⏰ Các tác vụ nền tự động: cảnh báo hợp đồng sắp hết hạn, nhắc gia hạn hợp đồng, nhắc thanh toán, cảnh báo vi phạm SLA của Ticket, xử lý vòng đời License, cộng điểm thân thiết hằng ngày
- 📧 Gửi Email bằng MailKit (báo giá, voucher, nhắc hạn...)
- 🤖 Tích hợp AI hỗ trợ phân tích trên Dashboard (tương thích OpenAI API, mặc định cấu hình mẫu dùng Groq)

---

# 🏗️ Kiến trúc hệ thống

Hệ thống được xây dựng theo mô hình **Clean Architecture**, gồm 4 project backend tách biệt và 1 project frontend.

```text
                     React Frontend (Vite)
                            │
                       RESTful API (JWT)
                            │
                    ASP.NET Core 8 Web API
                     (CRM.API)
                            │
        ┌───────────────────┴───────────────────┐
        │                                       │
CRM.Application (CQRS/MediatR)         CRM.Infrastructure
        │            (EF Core, RabbitMQ,        │
        │             Email, AI, PDF...)        │
        └───────────────────┬───────────────────┘
                            │
                      CRM.Domain
                    (Entities/Interfaces)
                            │
                    MySQL  +  RabbitMQ
```

Việc áp dụng Clean Architecture giúp:

- Tách biệt rõ nghiệp vụ với dữ liệu.
- Dễ bảo trì và mở rộng.
- Dễ kiểm thử (Unit Test).
- Giảm sự phụ thuộc giữa các tầng.

> ⚠️ **Lưu ý quan trọng:** Backend kết nối RabbitMQ ngay khi khởi động (dùng cho hàng đợi ghi Audit Log và thông báo). Nếu RabbitMQ chưa chạy, `dotnet run` sẽ báo lỗi và ứng dụng **không khởi động được** — đây là dependency bắt buộc, không phải tùy chọn.

---

# 🛠️ Công nghệ sử dụng

## Backend

- ASP.NET Core 8 (Web API)
- Entity Framework Core 8 + Pomelo.EntityFrameworkCore.MySql
- MediatR (CQRS)
- AutoMapper
- FluentValidation
- JWT Bearer Authentication
- BCrypt.Net-Next (mã hóa mật khẩu)
- Serilog (ghi log ra Console)
- MailKit / MimeKit (gửi Email)
- OpenAI SDK (tương thích các API dạng OpenAI, ví dụ Groq)
- QuestPDF (xuất hợp đồng ra file PDF)
- RabbitMQ.Client (hàng đợi Audit Log, thông báo)
- Swashbuckle / Swagger (tài liệu API)
- Rate Limiting (chống spam form public, chống brute-force đăng nhập)

## Frontend

- React 19
- Vite
- React Router DOM
- Tailwind CSS 4 (qua plugin `@tailwindcss/vite`)
- TanStack Query (React Query)
- Zustand (state management)
- React Hook Form
- Axios
- Recharts (biểu đồ)
- Lucide React (icon)

## Database & hạ tầng

- MySQL 8.0 trở lên
- RabbitMQ 3.x (kèm plugin management) — **bắt buộc**

---

# 📂 Cấu trúc thư mục

```text
LVTN
│
├── database
│   └── CRM-LVTN.sql          # Schema database (không kèm dữ liệu mẫu)
│
├── frontend
│   ├── public
│   ├── src
│   │   ├── api                # Cấu hình axios, gọi API (axiosClient.js)
│   │   ├── assets
│   │   ├── components
│   │   │   ├── common
│   │   │   └── layout
│   │   ├── features            # Code theo từng nghiệp vụ (feature-based)
│   │   │   ├── activities
│   │   │   ├── audit
│   │   │   ├── auth
│   │   │   ├── contracts
│   │   │   ├── customers
│   │   │   ├── dashboard
│   │   │   ├── invoices
│   │   │   ├── leads
│   │   │   ├── opportunities
│   │   │   ├── phieuthuchi
│   │   │   ├── products
│   │   │   ├── quotes
│   │   │   ├── settings
│   │   │   ├── tickets
│   │   │   └── users
│   │   ├── hooks
│   │   ├── routes
│   │   ├── stores
│   │   └── utils
│   └── package.json
│
├── src
│   ├── CRM.API                # Entry point, Controllers, Middleware
│   ├── CRM.Application        # CQRS Commands/Queries (MediatR)
│   ├── CRM.Domain              # Entities, Enums, Interfaces
│   └── CRM.Infrastructure      # EF Core, RabbitMQ, Email, AI, PDF
│
├── CRM.slnx
├── .gitignore
└── README.md
```

---

# 📚 Các module chính

| Module          | Mô tả                                              |
| --------------- | --------------------------------------------------- |
| Authentication  | Đăng nhập, đổi mật khẩu, xác thực (không có tự đăng ký) |
| User Management | Quản lý người dùng, tạo/khóa tài khoản, phân quyền   |
| Customer        | Quản lý khách hàng                                   |
| Address         | Quản lý địa chỉ khách hàng                           |
| Lead            | Quản lý khách hàng tiềm năng + form tiếp nhận công khai |
| Opportunity     | Quản lý cơ hội kinh doanh                            |
| Product         | Quản lý sản phẩm                                     |
| Danh mục        | Quản lý danh mục dùng chung (loại sản phẩm, loại ticket...) |
| Quotation       | Quản lý báo giá + trang xem báo giá công khai        |
| Contract        | Quản lý hợp đồng, xuất PDF hợp đồng                  |
| Invoice         | Quản lý hóa đơn                                      |
| Phiếu thu chi   | Quản lý các khoản thu/chi liên quan hợp đồng         |
| Ticket          | Hỗ trợ khách hàng + form gửi ticket công khai, cảnh báo vi phạm SLA |
| Loyalty         | Chương trình khách hàng thân thiết, cộng điểm hằng ngày |
| Voucher         | Quản lý mã giảm giá / ưu đãi                         |
| Alert           | Cảnh báo hệ thống                                    |
| Activity        | Nhật ký hoạt động theo từng đối tượng (khách hàng, hợp đồng...) |
| Analytics       | Phân tích số liệu, hỗ trợ AI trên Dashboard           |
| Dashboard       | Thống kê và báo cáo tổng quan                        |
| Audit Log       | Nhật ký hệ thống, xử lý bất đồng bộ qua RabbitMQ      |
| Location        | Dữ liệu địa giới hành chính (tỉnh/thành, quận/huyện...) |
| Health          | Endpoint kiểm tra tình trạng hoạt động của API        |

---

# 🔄 Quy trình nghiệp vụ

Hệ thống CRM hỗ trợ quản lý quy trình bán hàng từ khi tiếp cận khách hàng cho đến khi hoàn thành giao dịch.

```text
Lead
   │
   ▼
Opportunity
   │
   ▼
Quotation
   │
   ▼
Contract  ──▶  Phiếu thu chi
   │
   ▼
Invoice
```

Quy trình trên giúp doanh nghiệp theo dõi toàn bộ vòng đời của một khách hàng trong quá trình bán hàng.

---

# 💻 Yêu cầu môi trường

| Phần mềm            | Phiên bản tối thiểu | Ghi chú                                             |
| ------------------- | -------------------- | ---------------------------------------------------- |
| .NET SDK             | 8.0                   | Chạy Backend (ASP.NET Core Web API)                   |
| Node.js              | 20.x                  | Chạy Frontend (React 19 + Vite)                       |
| MySQL Server         | 8.0                   | Cơ sở dữ liệu chính                                   |
| RabbitMQ             | 3.x (kèm plugin management) | **Bắt buộc** — Backend sẽ không khởi động được nếu thiếu |
| Git                  | Bất kỳ                | Không bắt buộc, chỉ cần nếu chạy từ mã nguồn có `.git` |
| Visual Studio 2022   | 17.8 trở lên          | Khuyến nghị để chạy Backend, không bắt buộc            |
| Visual Studio Code   | Mới nhất              | Khuyến nghị để chạy Frontend, không bắt buộc           |

---

# ⚙️ Hướng dẫn cài đặt

## Bước 1. Clone source

```bash
git clone <repository-url>
cd LVTN
```

---

## Bước 2. Cài đặt và khởi động RabbitMQ (thực hiện trước tiên)

Cách nhanh nhất là dùng Docker (không cần cài trực tiếp lên máy):

```bash
docker run -d --name crm-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Kiểm tra RabbitMQ đã sẵn sàng tại trang quản trị `http://localhost:15672` (tài khoản mặc định `guest` / `guest`). Các queue/exchange (`crm.audit-log`, `crm.dlx`...) sẽ được Backend tự động khai báo khi khởi động, không cần tạo tay.

Nếu không dùng Docker, cài trực tiếp theo hướng dẫn tại https://www.rabbitmq.com/docs/download, đảm bảo service chạy ở cổng `5672` trước khi chạy Backend.

---

## Bước 3. Restore Backend

Mở Terminal tại thư mục `LVTN` và chạy:

```bash
cd src/CRM.API
dotnet restore
```

---

## Bước 4. Cài đặt Frontend

```bash
cd frontend
npm install
```

---

## Bước 5. Khởi tạo Database

Tạo database mới trong MySQL:

```sql
CREATE DATABASE `CRM-LVTN` CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;
```

Import file schema (chỉ chứa cấu trúc bảng, **không có dữ liệu mẫu**):

```
database/CRM-LVTN.sql
```

Có thể import bằng dòng lệnh:

```bash
mysql -u root -p CRM-LVTN < database/CRM-LVTN.sql
```

hoặc bằng MySQL Workbench / phpMyAdmin.

> ℹ️ Nếu bạn được cấp một file dump **đầy đủ dữ liệu** (đã có sẵn tài khoản mẫu), import trực tiếp file đó và bỏ qua Bước 6 bên dưới.

---

## Bước 6. Khởi tạo tài khoản Admin đầu tiên

Hệ thống **không có API đăng ký công khai** — chỉ Admin mới tạo được tài khoản mới qua giao diện. Vì vậy, nếu database đang trống (chỉ mới import schema), cần chèn thủ công một tài khoản Admin đầu tiên bằng SQL:

```sql
-- 1) Tạo 4 vai trò cố định
INSERT INTO HT_Role (TenRole, MoTa) VALUES
  ('Admin', 'Quản trị hệ thống'),
  ('Manager', 'Quản lý'),
  ('Sale', 'Nhân viên kinh doanh'),
  ('Accountant', 'Kế toán');

-- 2) Tạo hồ sơ nhân sự cho Admin
INSERT INTO HT_ThongTinNhanSu (HoTen, Email, TrangThai)
VALUES ('Quan Tri Vien', 'admin@crm.local', 1);

-- 3) Tạo tài khoản đăng nhập Admin (mật khẩu đã băm sẵn cho: 123456)
INSERT INTO HT_User (NhanSu_Id, Username, Password, Role_Id, TrangThai)
VALUES (
  (SELECT Id FROM HT_ThongTinNhanSu WHERE Email = 'admin@crm.local'),
  'admin',
  '$2b$11$efd.g7RsU9GLjUgTp2R9jeXlMPWRoJwvJ42p3LxEebEinNXR7eU7C',
  (SELECT Id FROM HT_Role WHERE TenRole = 'Admin'),
  'Active'
);
```

Sau khi đăng nhập bằng `admin` / `123456`, vào phân hệ **Quản lý người dùng** để tạo thêm các tài khoản Manager / Sale / Accountant qua giao diện. Nên đổi mật khẩu admin này ngay nếu triển khai ngoài môi trường thử nghiệm cục bộ.

---

## Bước 7. Cấu hình Backend

Tại thư mục `src/CRM.API`, tạo file `appsettings.Local.json` (đã có trong `.gitignore`, không bị commit lên git):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=CRM-LVTN;User=root;Password=<mat_khau_mysql_cua_ban>;CharSet=utf8mb4;"
  },
  "JwtSettings": {
    "Secret": "<chuoi_bi_mat_it_nhat_32_ky_tu_ngau_nhien>",
    "Issuer": "CRM.API",
    "Audience": "CRM.Client",
    "ExpirationInMinutes": 60
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "",
    "SmtpPassword": "",
    "FromName": "CRM System"
  },
  "OpenAI": {
    "ApiKey": "",
    "Model": "gpt-4o-mini"
  }
}
```

| Mục                 | Bắt buộc? | Ghi chú                                                             |
| -------------------- | --------- | --------------------------------------------------------------------- |
| `ConnectionStrings`   | Có        | Sai thì Backend không kết nối được MySQL                              |
| `JwtSettings.Secret`  | Có        | Bỏ trống sẽ ném lỗi khi khởi động                                     |
| `RabbitMq`            | Có        | Server phải đang chạy; `guest`/`guest` là tài khoản mặc định RabbitMQ |
| `Email`               | Không     | Bỏ trống vẫn chạy được, chỉ tính năng gửi email không hoạt động        |
| `OpenAI`              | Không     | Bỏ trống vẫn chạy được, chỉ tính năng phân tích AI trên Dashboard không hoạt động |

---

## Bước 8. Chạy Backend

Di chuyển đến thư mục API và chạy đúng profile **https** (bắt buộc — Frontend gọi cố định vào `https://localhost:7071/api`):

```bash
cd src/CRM.API
dotnet run --launch-profile https
```

> ⚠️ Nếu chạy `dotnet run` không chỉ định profile, ứng dụng có thể lên ở cổng khác (`5248`), khiến Frontend không gọi được API dù Backend vẫn đang chạy.

Nếu thành công, terminal hiện dòng `Now listening on: https://localhost:7071` và Swagger chạy tại:

```
https://localhost:7071/swagger
```

---

## Bước 9. Chạy Frontend

```bash
cd frontend
npm run dev
```

Sau đó truy cập:

```
http://localhost:5173
```

Frontend gọi API cố định vào `https://localhost:7071/api` (khai báo tại `frontend/src/api/axiosClient.js`) — nếu Backend chạy ở cổng khác, cần sửa lại giá trị này.

## Nếu cả Backend và Frontend đều chạy thành công, hệ thống CRM đã sẵn sàng để sử dụng.

---

# 👤 Tài khoản đăng nhập

Hệ thống **không có dữ liệu tài khoản mẫu sẵn có** trong file `database/CRM-LVTN.sql` (chỉ là schema). Sau khi thực hiện Bước 6 ở trên, bạn có tài khoản Admin đầu tiên:

| Username | Password | Vai trò        |
| -------- | -------- | -------------- |
| admin    | 123456   | Quản trị viên  |

Từ tài khoản Admin này, vào **Quản lý người dùng** để tạo thêm các tài khoản Manager / Sale / Accountant nhằm trải nghiệm đầy đủ phân quyền theo vai trò.

> Nếu bạn được cấp kèm một file dump database đầy đủ dữ liệu (khác với `CRM-LVTN.sql`), tài khoản đăng nhập sẽ theo đúng dữ liệu có sẵn trong file dump đó.

---

# 🔐 Xác thực người dùng

Hệ thống sử dụng **JWT (JSON Web Token)** để xác thực người dùng.

Quy trình đăng nhập:

```text
Đăng nhập
      │
      ▼
Kiểm tra tài khoản
      │
      ▼
Sinh JWT Token
      │
      ▼
Frontend lưu Token
      │
      ▼
Gửi Token trong Header
      │
      ▼
Truy cập các API được cấp quyền
```

Header Authorization:

```http
Authorization: Bearer <JWT_TOKEN>
```

Token mặc định hết hạn sau 60 phút (`JwtSettings.ExpirationInMinutes`).

---

# 📑 Swagger

Sau khi chạy Backend thành công, truy cập:

```
https://localhost:7071/swagger
```

Trình duyệt có thể cảnh báo "Kết nối này không an toàn" do chứng chỉ HTTPS tự ký (self-signed) dùng cho môi trường local — đây là cảnh báo bình thường, không phải lỗi thật, chỉ cần bấm **Advanced > Proceed to localhost**.

Các bước sử dụng:

### Bước 1
Đăng nhập bằng API `POST /api/Auth/login`.

### Bước 2
Copy chuỗi `accessToken` trả về trong `data`.

### Bước 3
Nhấn nút **Authorize** trên Swagger.

### Bước 4
Dán Token theo định dạng:

```text
Bearer eyJhbGciOi...
```

### Bước 5
Có thể bắt đầu kiểm thử toàn bộ API.

---

# 📡 Các nhóm API

| Module              | Ghi chú                                          |
| ------------------- | ------------------------------------------------- |
| Auth                | Đăng nhập, đổi mật khẩu                            |
| UserManagement      | Quản lý người dùng (chỉ Admin)                     |
| Customer            | Quản lý khách hàng                                 |
| Address             | Quản lý địa chỉ                                    |
| Lead                | Quản lý khách hàng tiềm năng                       |
| Public/Leads        | Tiếp nhận Lead từ form công khai (landing page)     |
| Opportunity         | Quản lý cơ hội kinh doanh                          |
| Product             | Quản lý sản phẩm                                   |
| DanhMuc             | Danh mục dùng chung                                |
| Quote               | Quản lý báo giá                                    |
| Public/Quotes       | Xem báo giá công khai qua link riêng                |
| Contract            | Quản lý hợp đồng                                   |
| Invoice             | Quản lý hóa đơn                                    |
| PhieuThuChi         | Quản lý phiếu thu chi                              |
| Ticket              | Quản lý Ticket hỗ trợ                              |
| Public/Tickets      | Gửi Ticket từ form công khai                        |
| Loyalty             | Chương trình khách hàng thân thiết                  |
| Voucher             | Quản lý voucher                                    |
| Alert               | Cảnh báo hệ thống                                   |
| Activity            | Nhật ký hoạt động                                   |
| Analytics           | Phân tích số liệu                                  |
| AuditLog            | Nhật ký hệ thống                                    |
| Location            | Dữ liệu địa giới hành chính                         |
| Health              | Kiểm tra tình trạng API                             |

Các endpoint dưới nhóm `Public/*` (dành cho landing page) không yêu cầu đăng nhập nhưng bị giới hạn tốc độ gọi (rate limiting) theo IP để chống spam.

---

# 📧 Gửi Email

Hệ thống sử dụng **MailKit** để gửi Email (báo giá, voucher, nhắc hạn hợp đồng...).

Thông tin SMTP được cấu hình trong `appsettings.Local.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your_email@gmail.com",
    "SmtpPassword": "your_app_password",
    "FromName": "CRM System"
  }
}
```

Nếu để trống, hệ thống vẫn chạy bình thường, chỉ tính năng gửi email không hoạt động.

---

# 🤖 Tích hợp AI

Hệ thống hỗ trợ tích hợp AI (thông qua OpenAI SDK, tương thích với các API theo chuẩn OpenAI, ví dụ Groq) để hỗ trợ phân tích số liệu trên Dashboard.

API Key và endpoint được cấu hình trong `appsettings.Local.json`, ví dụ:

```json
{
  "OpenAI": {
    "ApiKey": "",
    "BaseUrl": "https://api.groq.com/openai/v1",
    "Model": "llama-3.3-70b-versatile"
  }
}
```

Nếu để trống `ApiKey`, hệ thống vẫn chạy bình thường, chỉ tính năng phân tích AI không hoạt động.

---

# ⏰ Tác vụ nền (Background Jobs)

Backend chạy các Hosted Service sau khi khởi động:

| Tác vụ                              | Chức năng                                       |
| ------------------------------------ | ------------------------------------------------ |
| AuditLogConsumerHostedService        | Lắng nghe hàng đợi RabbitMQ, ghi Audit Log        |
| LoyaltyDailyJobHostedService         | Cộng điểm khách hàng thân thiết hằng ngày         |
| ContractExpirationJobHostedService   | Cảnh báo hợp đồng sắp hết hạn                     |
| ContractRenewalReminderJobHostedService | Nhắc gia hạn hợp đồng                          |
| PaymentReminderJobHostedService      | Nhắc thanh toán                                   |
| TicketSlaEscalationJobHostedService  | Cảnh báo Ticket vi phạm SLA                       |
| LicenseLifecycleJobHostedService     | Xử lý vòng đời License                            |

---

# 📊 Dashboard

Dashboard cung cấp các thông tin tổng quan như:

- Tổng số khách hàng
- Tổng số Lead
- Tổng doanh thu
- Báo giá
- Hợp đồng
- Hóa đơn
- Biểu đồ thống kê (Recharts)
- Phân tích hỗ trợ AI (nếu đã cấu hình OpenAI)

---

# 📷 Giao diện

Một số màn hình chính của hệ thống:

- Đăng nhập
- Dashboard
- Quản lý khách hàng
- Quản lý Lead
- Quản lý sản phẩm
- Quản lý báo giá
- Quản lý hợp đồng
- Quản lý hóa đơn
- Quản lý Ticket

> Có thể bổ sung hình ảnh minh họa sau khi triển khai hoàn thiện.

---

# ❓ Một số lỗi thường gặp

### `dotnet run` báo lỗi ngay khi khởi động, liên quan RabbitMQ / connection refused

- Nguyên nhân: RabbitMQ chưa chạy.
- Khắc phục: khởi động RabbitMQ (xem Bước 2) **trước** khi chạy Backend.

### `InvalidOperationException: JwtSettings configuration is missing`

- Nguyên nhân: thiếu `JwtSettings.Secret` trong `appsettings.Local.json`.
- Khắc phục: điền Secret ít nhất 32 ký tự.

### Frontend không gọi được API / lỗi CORS

- Nguyên nhân: Backend chạy sai cổng (không phải `7071`) hoặc chạy đúng cổng nhưng khác profile.
- Khắc phục: chạy đúng `dotnet run --launch-profile https`; nếu vẫn cần đổi cổng, sửa `API_BASE_URL` trong `frontend/src/api/axiosClient.js`.

### Không đăng nhập được dù đã import `CRM-LVTN.sql`

- Nguyên nhân: file SQL chỉ chứa schema, không có dữ liệu mẫu.
- Khắc phục: thực hiện Bước 6 để chèn tài khoản Admin đầu tiên.

### Không kết nối được Database

- Kiểm tra MySQL đã khởi động chưa.
- Kiểm tra `ConnectionStrings` (Server/Port/User/Password).
- Kiểm tra tên Database (`CRM-LVTN`).

### Không gửi được Email / Phân tích AI không hoạt động

- Đây là các tính năng tùy chọn — hệ thống vẫn chạy bình thường nếu bỏ trống cấu hình `Email` / `OpenAI`.
- Nếu cần dùng, kiểm tra lại SMTP, App Password, và API Key tương ứng.

### `npm install` báo lỗi thiếu quyền / EACCES

- Chạy lại terminal với quyền Administrator (Windows) hoặc `sudo` (macOS/Linux), hoặc cài Node qua `nvm`.

### Trình duyệt cảnh báo "Kết nối này không an toàn" khi mở Swagger

- Backend dùng chứng chỉ HTTPS tự ký cho môi trường local — bấm **Advanced > Proceed to localhost**, không phải lỗi thật.

### Cổng `3306` / `5672` / `7071` / `5173` đã bị chiếm dụng

- Có tiến trình khác đang dùng cổng đó — tắt tiến trình đó hoặc đổi cổng trong cấu hình tương ứng (MySQL, RabbitMQ, `launchSettings.json`, `vite.config.js`).

---

# 🚀 Hướng phát triển

Trong tương lai, hệ thống có thể được mở rộng với các chức năng:

- Quản lý kho, quản lý đơn hàng
- Mobile App
- Thông báo thời gian thực (SignalR)
- Docker Compose cho toàn bộ hệ thống (Backend + Frontend + MySQL + RabbitMQ)
- Redis Cache
- CI/CD
- Triển khai Cloud
- Báo cáo nâng cao
- AI Chatbot

---

# 👨‍💻 Thành viên thực hiện

| Họ và tên   | Vai trò             |
| ----------- | -------------------- |
| Võ Văn Nhân | Fullstack Developer  |

---

# 📄 Giấy phép

Dự án được phát triển phục vụ cho mục đích học tập và nghiên cứu.

---

# ⭐ Ghi chú

Nếu thấy dự án hữu ích, hãy để lại ⭐ cho repository để ủng hộ nhóm phát triển.

Cảm ơn bạn đã quan tâm đến dự án **CRM Online**.