# syntax=docker/dockerfile:1

# ---------- Stage 1: Build ----------
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
# VITE_API_BASE_URL truyền vào lúc build (docker-compose build args) — Vite nhúng thẳng giá trị
# này vào bundle JS tĩnh, không đọc được nữa sau khi build xong (khác biến môi trường runtime
# của backend .NET) — đây là lý do dùng ARG/ENV ở bước build, không phải ở stage runtime.
ARG VITE_API_BASE_URL=http://localhost:8080/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# ---------- Stage 2: Runtime (Nginx) ----------
FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
