# Fitora Backend

## Overview

Fitora Backend is a modern social network and technology platform for IT students at Dai Nam University. The system is built with ASP.NET Core using microservices architecture, supporting academic collaboration, document sharing, chat, Q&A, and scalable, extensible features.

---

## System Architecture

<div align="center">
  <img src="https://raw.githubusercontent.com/HuyHoangFSDev/fitora_backend/product/image.png" align="center" style="width: 100%" />
</div>

**Architecture Overview:**
- **Frontend:** ReactJS application communicates with the system through an API Gateway.
- **API Gateway:** Acts as a single entry point, routing requests to appropriate backend services.
- **Core Services:** Independent microservices, each with its own database/cache:
  - **Auth Service:** Authentication & authorization (MySQL, Redis cache)
  - **User Service:** User management (MySQL, Redis cache)
  - **Interact Service:** Interactions (posting, comments, likes...) (MySQL)
  - **Notification Service:** Notification management (MySQL)
  - **Chat Service:** Real-time chat/message (MongoDB)
- **Message Queue (RabbitMQ):** Asynchronous communication between services, event/message passing.
- **Search Service:** Handles searching functionality, possibly using Elasticsearch.

---

## Features

- **Q&A System:** Structured question and answer with voting (Stack Overflow style)
- **Document Sharing:** Upload, organize, and retrieve learning materials
- **Chat:** Real-time messaging, file sharing
- **Discussion Forum:** Reddit-style threads with upvote/downvote
- **Notification:** Real-time and batch notifications
- **Scalable Microservices:** Modular, maintainable, and cloud-ready
- **Future-ready:** Easy to integrate AI/chatbot and personalized recommendations

---

## Technologies Used

- **Backend:** ASP.NET Core (C#)
- **Frontend:** ReactJS
- **Databases:** MySQL (relational), MongoDB (chat), Redis (cache)
- **Message Broker:** RabbitMQ
- **Search:** Elasticsearch (planned or for search service)
- **API Testing:** Postman
- **DevOps:** Git, Docker (optional)
- **Methodology:** Agile/Scrum

---

## Project Structure

```
.
├── .github/
├── .idea/
├── ApiGateways/         # API Gateway for routing requests
├── BuildingBlocks/      # Shared utilities and libraries
├── Services/            # Microservices: Auth, User, Interact, Notification, Chat, Search
├── README.md
├── .gitignore
├── fitora_backend.sln
├── fitora_backend.sln.DotSettings.user
```

> Each service in `Services/` has its own codebase, database access, and possibly its own cache/message queue consumer.

---

## Prerequisites

- .NET 8.0 SDK
- MySQL (v8.0+)
- MongoDB (v4.4+)
- Redis (v6.0+)
- RabbitMQ (v3.8+)
- Git
- Postman

---

## Setup & Run

### 1. Clone repository

```bash
git clone https://github.com/HuyHoangDevVN/fitora_backend.git
cd fitora_backend
```

### 2. Install dependencies

```bash
dotnet restore
```

### 3. Configure environment

Create a `.env` file at the root:

```env
MYSQL_CONNECTION_STRING="Server=localhost;Database=Fitora;User=your_user;Password=your_password;"
MONGODB_CONNECTION_STRING="mongodb://localhost:27017/Fitora"
REDIS_CONNECTION="localhost:6379"
RABBITMQ_HOST="localhost"
JWT_SECRET="your_jwt_secret_key"
```

### 4. Run database migrations

```bash
dotnet ef migrations add InitialCreate --project Services/<ServiceName>
dotnet ef database update --project Services/<ServiceName>
```
> Replace `<ServiceName>` with the actual service (e.g., AuthService, UserService).

### 5. Start Services

```bash
dotnet run --project Services/AuthService
dotnet run --project Services/UserService
dotnet run --project Services/InteractService
dotnet run --project Services/NotificationService
dotnet run --project Services/ChatService
dotnet run --project Services/SearchService
```

### 6. API Gateway

```bash
dotnet run --project ApiGateways/<GatewayProject>
```
> Replace `<GatewayProject>` with your gateway implementation if applicable.

### 7. Test APIs

- Import the Postman collection from `docs/postman_collection.json` if available.
- Test endpoints such as `/api/auth/register`, `/api/user/profile`, `/api/interact/post`, `/api/notification`, `/api/chat/message`, `/api/search`.

### 8. Run with Docker (if docker-compose.yml is present)

```bash
docker-compose up -d
```

---

## API Endpoints (Sample)

- **Auth Service:**  
  - `POST /api/auth/register`  
  - `POST /api/auth/login`
- **User Service:**  
  - `GET /api/user/profile`  
  - `PUT /api/user/profile`
- **Interact Service:**  
  - `POST /api/interact/post`  
  - `POST /api/interact/comment`
- **Notification Service:**  
  - `GET /api/notification`
- **Chat Service:**  
  - `POST /api/chat/message`  
  - `GET /api/chat/history`
- **Search Service:**  
  - `GET /api/search?q=...`

---

## Testing

```bash
dotnet test
```

---

## Contribution

- Fork the repository and create a feature branch
- Follow code conventions and commit guidelines
- Submit pull requests for review

---

## Future Roadmap

- Integrate AI-powered recommendations
- Add real-time chatbot for learning support
- Enhance scalability with Kubernetes

---

## License

MIT License.

---

## Tiếng Việt

### Tổng quan

Fitora Backend là hệ thống mạng xã hội, không gian công nghệ dành cho sinh viên CNTT Đại học Đại Nam. Nền tảng xây dựng với ASP.NET Core, áp dụng kiến trúc microservices, hỗ trợ hỏi đáp, chia sẻ tài liệu, chat, thông báo, tìm kiếm, dễ dàng mở rộng và tích hợp AI.

### Kiến trúc hệ thống

<div align="center">
  <img src="https://raw.githubusercontent.com/HuyHoangFSDev/fitora_backend/product/image.png" align="center" style="width: 100%" />
</div>


- **Frontend:** Ứng dụng ReactJS
- **API Gateway:** Cổng kết nối giữa frontend và backend
- **Các Microservice:** Auth, User, Interact, Notification, Chat, Search (mỗi service có database/cache riêng)
- **Message Queue:** RabbitMQ dùng để giao tiếp bất đồng bộ giữa các service
- **Search Service:** Tìm kiếm, sử dụng Elasticsearch (nếu có)

### Tính năng chính

- Hỏi đáp, bình chọn
- Chia sẻ tài liệu học tập
- Chat real-time
- Thảo luận theo chủ đề
- Thông báo tức thời
- Dễ dàng mở rộng, sẵn sàng cho tích hợp AI

### Công nghệ

- ASP.NET Core, ReactJS, MySQL, MongoDB, Redis, RabbitMQ, Elasticsearch, Docker, Git

### Cài đặt & chạy

- Clone, restore, cấu hình `.env`, migrate DB, chạy các service và API Gateway
- Test API bằng Postman hoặc Swagger UI
- Có thể chạy toàn bộ qua Docker Compose

### Đóng góp

- Fork, tạo branch, tuân thủ coding convention, gửi pull request

### Định hướng phát triển

- Tích hợp AI gợi ý học tập
- Chatbot hỗ trợ real-time
- Mở rộng với Kubernetes

### Giấy phép

MIT License.
