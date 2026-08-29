# syntax=docker/dockerfile:1

# ---------- Stage 1: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để tận dụng Docker layer cache — chỉ restore lại khi có csproj thay đổi,
# không phải build lại từ đầu mỗi khi sửa 1 dòng code.
COPY src/CRM.Domain/*.csproj src/CRM.Domain/
COPY src/CRM.Application/*.csproj src/CRM.Application/
COPY src/CRM.Infrastructure/*.csproj src/CRM.Infrastructure/
COPY src/CRM.API/*.csproj src/CRM.API/
RUN dotnet restore src/CRM.API/CRM.API.csproj

# Copy toàn bộ source rồi build + publish
COPY src/ src/
RUN dotnet publish src/CRM.API/CRM.API.csproj -c Release -o /app/publish --no-restore

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Chạy bằng user không phải root — thực hành bảo mật cơ bản, không chạy container với quyền root.
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CRM.API.dll"]
