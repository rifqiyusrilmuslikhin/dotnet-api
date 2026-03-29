# DotNet API

A modern, production-ready REST API built with .NET 10, featuring clean architecture principles, comprehensive authentication, user management, and file handling capabilities.

## Quick Start

### Prerequisites
- .NET 10 SDK or later
- Visual Studio / VS Code with C# extension

### Installation

```bash
# Clone and restore dependencies
git clone <repository-url>
cd dotnet-api
dotnet restore

# Run the API
dotnet run --project src/DotnetApi.Api
```

The API will be available at `https://localhost:5161`. Access the interactive Swagger documentation at `/swagger`.

## Architecture

This project follows **Clean Architecture** principles with strict separation of concerns:

```
src/
├── DotnetApi.Domain/          # Core domain entities, interfaces, business rules
├── DotnetApi.Application/     # Use cases, handlers, validation logic (MediatR)
├── DotnetApi.Infrastructure/  # Data access, external services, repositories
└── DotnetApi.Api/            # API controllers, middleware, dependency injection

tests/
├── DotnetApi.Domain.Tests/          # Domain layer unit tests (17 tests)
├── DotnetApi.Application.Tests/     # Application/handler tests (49 tests)
└── DotnetApi.Infrastructure.Tests/  # Infrastructure/repository tests (19 tests)
```

## Key Features

- **JWT Authentication** — Secure token-based authentication with refresh token support
- **User Management** — Complete CRUD operations with email validation
- **File Upload** — Avatar upload with local file storage service
- **Password Security** — BCrypt hashing with verification
- **Request Validation** — FluentValidation for robust input validation
- **Error Handling** — Centralized exception handling with meaningful error responses
- **Logging** — Serilog integration for structured logging
- **API Documentation** — Swagger/OpenAPI automatic documentation

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10, ASP.NET Core 10 |
| Database | SQL Server (EF Core) |
| Architecture Pattern | Clean Architecture + CQRS (MediatR) |
| Validation | FluentValidation 12.1.1 |
| Testing | xUnit 2.9.3, Shouldly 4.2.1, NSubstitute 5.3.0 |
| Authentication | JWT (System.IdentityModel.Tokens.Jwt) |
| Logging | Serilog |
| API Documentation | Swashbuckle.AspNetCore |

## API Endpoints

### Authentication
- `POST /api/auth/register` — Create new user account
- `POST /api/auth/login` — Authenticate and receive JWT token

### Users (Requires Authentication)
- `GET /api/users/me` — Get current user profile
- `PUT /api/users/me` — Update user profile
- `PUT /api/users/me/password` — Change password
- `POST /api/users/me/avatar` — Upload avatar image

## Getting Started with Development

1. **Open the solution** in your IDE
2. **Review** `ARCHITECTURE.md` for detailed design documentation
3. **Run tests** to verify the setup: `dotnet test`
4. **Start the API** and explore endpoints via Swagger

## Project Structure

Each layer follows a feature-based folder structure:

```
Features/
├── Auth/
│   ├── Commands/
│   │   ├── Register/
│   │   └── Login/
│   └── Queries/
│
├── Users/
│   ├── Commands/
│   │   ├── UpdateCurrentUser/
│   │   ├── ChangeCurrentUserPassword/
│   │   └── UploadAvatar/
│   └── Queries/
│       └── GetCurrentUser/
```

## License

MIT
