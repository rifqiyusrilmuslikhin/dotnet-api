# DotNet API

A modern, production-ready REST API built with **.NET 10**, featuring Clean Architecture principles, comprehensive authentication (JWT + Google OAuth2 + refresh tokens), user management, and file handling capabilities.

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- [PostgreSQL](https://www.postgresql.org/download/) 14+
- Visual Studio / VS Code with C# extension

### Installation

```bash
# Clone and restore dependencies
git clone https://github.com/rifqiyusrilmuslikhin/dotnet-api.git
cd dotnet-api
dotnet restore
```

### Configuration

Update `src/DotnetApi.Api/appsettings.json` with your settings:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dotnet_clean_arch;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "SecretKey": "YOUR_SUPER_SECRET_KEY_MINIMUM_32_CHARS_LONG",
    "Issuer": "DotnetApi",
    "Audience": "DotnetApiUsers",
    "ExpiresInSeconds": 3600,
    "RefreshTokenExpiresInDays": 7
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  }
}
```

### Database Setup

```bash
# Apply EF Core migrations
dotnet ef database update --project src/DotnetApi.Infrastructure --startup-project src/DotnetApi.Api
```

### Run the API

```bash
dotnet run --project src/DotnetApi.Api
```

The API will be available at `https://localhost:5161`. Access the interactive Swagger documentation at `/swagger`.

### Run Tests

```bash
dotnet test
# Total: 146 tests | Passed: 146 | Failed: 0
```

---

## Architecture

This project follows **Clean Architecture** principles with strict separation of concerns and CQRS pattern via MediatR. For comprehensive design documentation, see [`ARCHITECTURE.md`](ARCHITECTURE.md).

```
src/
├── DotnetApi.Domain/          # Core domain entities, interfaces, business rules
├── DotnetApi.Application/     # Use cases, CQRS handlers, validation logic (MediatR)
├── DotnetApi.Infrastructure/  # Data access, external services, repositories
└── DotnetApi.Api/             # API controllers, middleware, dependency injection

tests/
├── DotnetApi.Domain.Tests/          # Domain entity & exception tests (27 tests)
├── DotnetApi.Application.Tests/     # Handler & validator tests (93 tests)
└── DotnetApi.Infrastructure.Tests/  # Repository & service tests (26 tests)
```

**Dependency rule:** Domain ← Application ← Infrastructure ← Api (inner layers never reference outer layers).

---

## Key Features

| Feature | Description |
|---------|-------------|
| 🔐 **JWT Authentication** | Access tokens (configurable expiry) + refresh token rotation |
| 🔄 **Refresh Tokens** | Database-stored, rotatable, revocable refresh tokens with configurable expiry |
| 🌐 **Google OAuth2** | Sign in / sign up with Google ID token validation |
| 👤 **User Management** | Profile CRUD, password change, email validation |
| 📸 **Avatar Upload** | Image upload (JPEG/PNG/WebP, max 2 MB) with local file storage |
| 🔒 **Password Security** | BCrypt hashing with verification |
| ✅ **Request Validation** | FluentValidation pipeline behavior for all commands |
| 🚨 **Global Error Handling** | Centralized `IExceptionHandler` with domain-aware error mapping |
| 📝 **Structured Logging** | Serilog with console + rolling file sinks, enrichers |
| 📖 **API Documentation** | Swagger/OpenAPI with JWT bearer authentication support |
| 🌍 **CORS** | Configurable allowed origins via `appsettings.json` |
| ⚡ **Rate Limiting** | Fixed window rate limiter (configurable permits & window) |
| 🏥 **Health Check** | Application health endpoint with version and environment info |

---

## Technology Stack

| Category | Technology | Version |
|----------|-----------|---------|
| Framework | .NET / ASP.NET Core | 10.0 |
| Database | PostgreSQL + EF Core (Npgsql) | EF Core 10.0 |
| Architecture | Clean Architecture + CQRS | MediatR 12.4.1 |
| Validation | FluentValidation | 12.1.1 |
| Authentication | JWT + Google OAuth2 | System.IdentityModel.Tokens.Jwt 8.17.0, Google.Apis.Auth 1.70.0 |
| Password Hashing | BCrypt | BCrypt.Net-Next 4.1.0 |
| Logging | Serilog | 10.0.0 |
| API Docs | Swashbuckle (Swagger) | 10.1.6 |
| Testing | xUnit + Shouldly + NSubstitute | 2.9.3 / 4.2.1 / 5.3.0 |
| DB Naming | EFCore.NamingConventions (snake_case) | 10.0.1 |

---

## API Endpoints

### Health Check

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/healthcheck` | Application health status | ❌ |

### Authentication

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/register` | Register a new user account | ❌ |
| `POST` | `/api/auth/login` | Login with email & password | ❌ |
| `POST` | `/api/auth/google` | Login / register via Google OAuth2 | ❌ |
| `POST` | `/api/auth/refresh` | Refresh access token (token rotation) | ❌ |
| `POST` | `/api/auth/revoke` | Revoke a refresh token | ❌ |

### Users (Requires Authentication 🔒)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/users/me` | Get current user profile | ✅ |
| `PUT` | `/api/users/me` | Update current user profile | ✅ |
| `PATCH` | `/api/users/me/password` | Change password | ✅ |
| `POST` | `/api/users/me/avatar` | Upload avatar image | ✅ |

### Authentication Flow

```
┌─────────┐         ┌─────────┐         ┌─────────┐
│  Client  │──POST──▶│ /login  │──200──▶ │ Access  │
│          │         │ /register│        │ Token + │
│          │         │ /google │        │ Refresh │
│          │         └─────────┘        │ Token   │
│          │                            └────┬────┘
│          │                                 │
│          │  Access Token expired?          │
│          │──POST /auth/refresh────────────▶│
│          │◀── New Access Token + ──────────┘
│          │    New Refresh Token (rotation)
│          │
│          │  Logout?
│          │──POST /auth/revoke────────────▶ Invalidate
└─────────┘
```

**Refresh token rotation:** Every time a refresh token is used, the old token is revoked and replaced with a new one. This limits the window of opportunity if a token is compromised.

---

## Project Structure

Each layer follows a **feature-based folder structure**:

```
src/DotnetApi.Application/Features/
├── Auth/
│   └── Commands/
│       ├── Register/          # RegisterCommand, Validator, Handler
│       ├── Login/             # LoginCommand, Validator, Handler
│       ├── GoogleLogin/       # GoogleLoginCommand, Validator, Handler
│       ├── RefreshToken/      # RefreshTokenCommand, Validator, Handler
│       └── RevokeToken/       # RevokeTokenCommand, Validator, Handler
│
├── HealthCheck/
│   └── Queries/
│       └── GetHealthCheck/    # Query, Response, Handler
│
└── Users/
    ├── Commands/
    │   ├── UpdateCurrentUser/         # Command, Validator, Handler
    │   ├── ChangeCurrentUserPassword/ # Command, Validator, Handler
    │   └── UploadAvatar/              # Command, Validator, Handler
    └── Queries/
        └── GetCurrentUser/            # Query, Handler
```

### Domain Entities

| Entity | Description |
|--------|-------------|
| `User` | Core user entity with name, email, avatar, timestamps |
| `UserAccount` | Authentication account (local password or Google provider) |
| `RefreshToken` | Database-stored refresh token with expiry, revocation, rotation tracking |
| `HealthCheck` | Application health status value object |

### Database Schema (PostgreSQL, snake_case)

```
users
├── id (PK)
├── name
├── email (unique)
├── avatar_url
├── created_at
└── updated_at

user_accounts
├── id (PK)
├── user_id (FK → users)
├── provider ("local" | "google")
├── provider_id
└── password_hash

refresh_tokens
├── id (PK)
├── user_id (FK → users, cascade delete)
├── token (unique index)
├── expires_at
├── created_at
├── revoked_at
└── replaced_by_token
```

---

## Configuration

All settings are managed via `appsettings.json`:

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| `ConnectionStrings` | `DefaultConnection` | — | PostgreSQL connection string |
| `Jwt` | `SecretKey` | — | HMAC-SHA256 signing key (min 32 chars) |
| `Jwt` | `Issuer` | `DotnetApi` | Token issuer |
| `Jwt` | `Audience` | `DotnetApiUsers` | Token audience |
| `Jwt` | `ExpiresInSeconds` | `3600` | Access token lifetime (1 hour) |
| `Jwt` | `RefreshTokenExpiresInDays` | `7` | Refresh token lifetime |
| `Google` | `ClientId` | — | Google OAuth2 client ID |
| `FileStorage` | `UploadPath` | `uploads` | Local file storage directory |
| `FileStorage` | `BaseUrl` | `/uploads` | Public URL prefix for uploaded files |
| `Cors` | `AllowedOrigins` | `localhost:4200, localhost:3000` | Allowed CORS origins |
| `RateLimit` | `PermitLimit` | `100` | Max requests per window |
| `RateLimit` | `WindowSeconds` | `60` | Rate limit window duration |

---

## Getting Started with Development

1. **Clone** the repository and restore dependencies
2. **Configure** PostgreSQL connection and JWT settings in `appsettings.json`
3. **Apply migrations**: `dotnet ef database update --project src/DotnetApi.Infrastructure --startup-project src/DotnetApi.Api`
4. **Review** [`ARCHITECTURE.md`](ARCHITECTURE.md) for detailed design documentation
5. **Run tests** to verify the setup: `dotnet test`
6. **Start the API**: `dotnet run --project src/DotnetApi.Api`
7. **Explore** endpoints via Swagger at `/swagger`

### Adding a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/DotnetApi.Infrastructure \
  --startup-project src/DotnetApi.Api
```

---

## License

MIT
