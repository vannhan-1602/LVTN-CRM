# 🚀 CRM Online

> **CRM Online** là hệ thống quản lý quan hệ khách hàng (Customer Relationship Management) được xây dựng nhằm hỗ trợ doanh nghiệp quản lý khách hàng, khách hàng tiềm năng, sản phẩm, báo giá, hợp đồng, hóa đơn và các hoạt động chăm sóc khách hàng trên cùng một nền tảng.

---

# 📖 Giới thiệu

CRM Online được phát triển với mục tiêu số hóa quy trình quản lý khách hàng, giúp doanh nghiệp theo dõi toàn bộ quá trình từ khi tiếp cận khách hàng tiềm năng đến khi hoàn thành giao dịch và chăm sóc sau bán hàng.

Hệ thống được xây dựng theo mô hình **Clean Architecture**, tách biệt rõ ràng giữa các tầng nghiệp vụ, dữ liệu và giao diện, giúp dễ dàng mở rộng, bảo trì và phát triển trong tương lai.

---

# ✨ Chức năng chính

Hệ thống hiện hỗ trợ các chức năng:

- 🔐 Đăng nhập bằng JWT Authentication
- 👤 Quản lý người dùng và phân quyền
- 👥 Quản lý khách hàng
- 📈 Quản lý khách hàng tiềm năng (Lead)
- 💼 Quản lý cơ hội kinh doanh
- 📦 Quản lý sản phẩm
- 📝 Quản lý báo giá
- 📑 Quản lý hợp đồng
- 💰 Quản lý hóa đơn
- 🎫 Quản lý Ticket hỗ trợ khách hàng
- 📊 Dashboard thống kê
- 📍 Quản lý địa chỉ
- 📧 Gửi Email bằng MailKit
- 🤖 Tích hợp AI (OpenAI)
- 📜 Ghi nhận nhật ký hệ thống (Audit Log)

---

# 🏗️ Kiến trúc hệ thống

Hệ thống được xây dựng theo mô hình **Clean Architecture**.

```text
                 React Frontend
                        │
                  RESTful API
                        │
                ASP.NET Core API
                        │
        ┌───────────────┴───────────────┐
        │                               │
 Application Layer             Infrastructure Layer
        │                               │
        └───────────────┬───────────────┘
                        │
                  Domain Layer
                        │
                      MySQL
```

Việc áp dụng Clean Architecture giúp:

- Tách biệt rõ nghiệp vụ với dữ liệu.
- Dễ bảo trì và mở rộng.
- Dễ kiểm thử (Unit Test).
- Giảm sự phụ thuộc giữa các tầng.

---

# 🛠️ Công nghệ sử dụng

## Backend

- ASP.NET Core 8
- Entity Framework Core
- MySQL
- MediatR
- AutoMapper
- FluentValidation
- JWT Authentication
- BCrypt
- Serilog
- MailKit
- OpenAI SDK
- Swagger

---

## Frontend

- React
- Vite
- React Router
- Tailwind CSS
- TanStack Query
- Zustand
- React Hook Form
- Axios
- Recharts
- Lucide Icons

---

## Database

- MySQL 8

---

# 📂 Cấu trúc thư mục

```text
LVTN
│
├── frontend
│   ├── public
│   ├── src
│   │   ├── components
│   │   ├── hooks
│   │   ├── layouts
│   │   ├── pages
│   │   ├── routes
│   │   ├── services
│   │   ├── store
│   │   └── utils
│   └── package.json
│
├── src
│   ├── CRM.API
│   ├── CRM.Application
│   ├── CRM.Domain
│   └── CRM.Infrastructure
│
├── CRM-LVTN.sql
├── CRM.slnx
└── README.md
```

---

# 📚 Các module chính

| Module         | Mô tả                            |
| -------------- | -------------------------------- |
| Authentication | Đăng nhập và xác thực người dùng |
| User           | Quản lý người dùng               |
| Customer       | Quản lý khách hàng               |
| Lead           | Quản lý khách hàng tiềm năng     |
| Opportunity    | Quản lý cơ hội kinh doanh        |
| Product        | Quản lý sản phẩm                 |
| Quotation      | Quản lý báo giá                  |
| Contract       | Quản lý hợp đồng                 |
| Invoice        | Quản lý hóa đơn                  |
| Ticket         | Hỗ trợ khách hàng                |
| Dashboard      | Thống kê và báo cáo              |
| Address        | Quản lý địa chỉ                  |
| Audit Log      | Nhật ký hoạt động hệ thống       |

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
Contract
   │
   ▼
Invoice
```

Quy trình trên giúp doanh nghiệp theo dõi toàn bộ vòng đời của một khách hàng trong quá trình bán hàng.

---

# 💻 Yêu cầu môi trường

| Phần mềm           | Phiên bản    |
| ------------------ | ------------ |
| .NET SDK           | 8.0 trở lên  |
| NodeJS             | 20 trở lên   |
| MySQL              | 8.0 trở lên  |
| Visual Studio 2022 | 17.8 trở lên |
| Visual Studio Code | Mới nhất     |

---

# ⚙️ Hướng dẫn cài đặt

## Bước 1. Clone source

```bash
git clone <repository-url>
cd LVTN
```

---

## Bước 2. Restore Backend

Mở Terminal tại thư mục project và chạy:

```bash
dotnet restore
```

Lệnh trên sẽ cài đặt toàn bộ thư viện cần thiết cho Backend.

---

## Bước 3. Cài đặt Frontend

Di chuyển đến thư mục frontend:

```bash
cd frontend
```

Sau đó cài đặt các package:

```bash
npm install
```

---

## Bước 4. Khởi tạo Database

Tạo Database mới trong MySQL.

Ví dụ:

```sql
CREATE DATABASE CRMOnline;
```

Sau đó import file:

```
CRM-LVTN.sql
```

Có thể import bằng:

- MySQL Workbench
- phpMyAdmin
- MySQL Command Line

---

## Bước 5. Cấu hình kết nối Database

Mở file:

```
src/CRM.API/appsettings.json
```

hoặc

```
src/CRM.API/appsettings.Local.json
```

Cập nhật chuỗi kết nối:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CRMOnline;Uid=root;Pwd=your_password;"
  }
}
```

---

## Bước 6. Chạy Backend

Di chuyển đến thư mục API:

```bash
cd src/CRM.API
```

Chạy project:

```bash
dotnet run
```

Nếu thành công, Swagger sẽ chạy tại:

```
https://localhost:7071/swagger
```

---

## Bước 7. Chạy Frontend

Di chuyển đến thư mục frontend:

```bash
cd frontend
```

Khởi động ứng dụng:

```bash
npm run dev
```

Sau đó truy cập:

```
http://localhost:5173
```

## Nếu cả Backend và Frontend đều chạy thành công, hệ thống CRM đã sẵn sàng để sử dụng.

# 👤 Tài khoản đăng nhập mặc định

Sau khi import database thành công, bạn có thể sử dụng các tài khoản dưới đây để trải nghiệm hệ thống.

> **Lưu ý:** Danh sách dưới đây sẽ được cập nhật theo đúng dữ liệu trong file `CRM-LVTN.sql`.

| Username   | Password | Vai trò              |
| ---------- | -------- | -------------------- |
| admin      | 123456   | Quản trị viên        |
| manager    | 123456   | Quản lý              |
| sale01     | 123456   | Nhân viên kinh doanh |
| accountant | 123456   | Kế toán              |

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

---

# 📑 Swagger

Sau khi chạy Backend thành công, truy cập:

```
https://localhost:7071/swagger
```

Các bước sử dụng:

### Bước 1

Đăng nhập bằng API Login.

### Bước 2

Copy JWT Token trả về.

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

Dự án được chia thành nhiều nhóm API tương ứng với từng nghiệp vụ.

| Module         | Chức năng                    |
| -------------- | ---------------------------- |
| Authentication | Đăng nhập, xác thực          |
| User           | Quản lý người dùng           |
| Customer       | Quản lý khách hàng           |
| Lead           | Quản lý khách hàng tiềm năng |
| Opportunity    | Quản lý cơ hội kinh doanh    |
| Product        | Quản lý sản phẩm             |
| Quotation      | Quản lý báo giá              |
| Contract       | Quản lý hợp đồng             |
| Invoice        | Quản lý hóa đơn              |
| Ticket         | Quản lý Ticket               |
| Dashboard      | Thống kê                     |
| Address        | Quản lý địa chỉ              |

---

# 📧 Gửi Email

Hệ thống sử dụng **MailKit** để gửi Email.

Thông tin SMTP được cấu hình trong:

```
appsettings.json
```

hoặc

```
appsettings.Local.json
```

Ví dụ:

```json
{
  "MailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your_email@gmail.com",
    "Password": "your_app_password"
  }
}
```

---

# 🤖 Tích hợp AI

Hệ thống hỗ trợ tích hợp **OpenAI** để mở rộng các chức năng AI.

Ví dụ:

- Gợi ý nội dung
- Hỗ trợ chăm sóc khách hàng
- Sinh nội dung Email
- Trợ lý AI

API Key được cấu hình trong:

```
appsettings.Local.json
```

---

# 📊 Dashboard

Dashboard cung cấp các thông tin tổng quan như:

- Tổng số khách hàng
- Tổng số Lead
- Tổng doanh thu
- Báo giá
- Hợp đồng
- Hóa đơn
- Biểu đồ thống kê

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

### Không kết nối được Database

- Kiểm tra MySQL đã khởi động chưa.
- Kiểm tra Connection String.
- Kiểm tra tên Database.

---

### Không đăng nhập được

- Đã import đầy đủ dữ liệu từ file SQL.
- Kiểm tra Username và Password.
- Kiểm tra JWT Secret.

---

### Frontend không gọi được API

- Kiểm tra Backend đã chạy chưa.
- Kiểm tra địa chỉ API trong file cấu hình.
- Kiểm tra CORS.

---

### Không gửi được Email

- Kiểm tra SMTP.
- Kiểm tra App Password.
- Kiểm tra Firewall.

---

# 🚀 Hướng phát triển

Trong tương lai, hệ thống có thể được mở rộng với các chức năng:

- Quản lý kho
- Quản lý đơn hàng
- Mobile App
- Thông báo thời gian thực (SignalR)
- Docker Compose
- Redis Cache
- CI/CD
- Triển khai Cloud
- Báo cáo nâng cao
- AI Chatbot

---

# 👨‍💻 Thành viên thực hiện

| Họ và tên   | Vai trò             |
| ----------- | ------------------- |
| Võ Văn Nhân | Fullstack Developer |

---

# 📄 Giấy phép

Dự án được phát triển phục vụ cho mục đích học tập và nghiên cứu.

---

# ⭐ Ghi chú

Nếu thấy dự án hữu ích, hãy để lại ⭐ cho repository để ủng hộ nhóm phát triển.

Cảm ơn bạn đã quan tâm đến dự án **CRM Online**.
