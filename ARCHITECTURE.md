# Architecture Guide & Coding Standards

This guide must be read before adding any new feature. It explains the architecture structure, naming conventions, and coding standards used in this project.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Folder Structure](#2-folder-structure)
3. [Inter-Layer Dependency Rules](#3-inter-layer-dependency-rules)
4. [Standards for Adding New Features](#4-standards-for-adding-new-features)
5. [Naming Conventions](#5-naming-conventions)
6. [Domain Layer](#6-domain-layer)
7. [Application Layer](#7-application-layer)
8. [Infrastructure Layer](#8-infrastructure-layer)
9. [API Layer](#9-api-layer)
10. [Error Handling](#10-error-handling)
11. [Logging](#11-logging)
12. [New Feature Checklist](#12-new-feature-checklist)
13. [Unit Testing](#13-unit-testing)

---

## 1. Architecture Overview

This project uses **Clean Architecture** with the **CQRS** (Command Query Responsibility Segregation) pattern via **MediatR**.

```
┌─────────────────────────────────────────────┐
│                  API Layer                  │  ← Controllers, Extensions, Exceptions
├─────────────────────────────────────────────┤
│             Application Layer               │  ← Commands, Queries, Behaviors, Validators
├─────────────────────────────────────────────┤
│             Infrastructure Layer            │  ← DbContext, Repositories, Services, JWT
├─────────────────────────────────────────────┤
│               Domain Layer                  │  ← Entities, Interfaces, Enums, Exceptions
└─────────────────────────────────────────────┘
```

**Request flow:**
```
HTTP Request
  → Controller
    → MediatR.Send(Command/Query)
      → ValidationBehavior (FluentValidation)
        → LoggingBehavior
          → Handler
            → Repository / Service (via Interface)
              → Database / External Service
                → Response DTO
```

---

## 2. Folder Structure

```
src/
├── DotnetApi.Domain/
│   ├── Entities/           # Domain entities with business logic
│   ├── Enums/              # Domain enumerations
│   ├── Exceptions/         # DomainException, NotFoundException
│   └── Interfaces/         # Repository & service contracts
│
├── DotnetApi.Application/
│   ├── Common/
│   │   ├── Behaviors/      # MediatR pipeline behaviors
│   │   ├── Exceptions/     # ValidationException
│   │   └── Models/         # Shared response DTOs (AuthResponse, UserResponse, ErrorResponse)
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs   # AddApplication()
│   └── Features/
│       └── {FeatureName}/
│           ├── Commands/
│           │   └── {ActionName}/
│           │       ├── {Action}Command.cs
│           │       ├── {Action}CommandValidator.cs
│           │       └── {Action}CommandHandler.cs
│           ├── Queries/
│           │   └── {ActionName}/
│           │       ├── {Action}Query.cs
│           │       └── {Action}QueryHandler.cs
│           └── Options/                     # If the feature has appsettings configuration
│               └── {Feature}Options.cs
│
├── DotnetApi.Infrastructure/
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs   # AddInfrastructureServices()
│   ├── Options/            # Infrastructure-specific options (JwtOptions)
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/  # IEntityTypeConfiguration<T> per entity
│   ├── Repositories/       # IRepository implementations
│   └── Services/           # IService implementations (JWT, PasswordHasher, etc.)
│
└── DotnetApi.Api/
    ├── Controllers/        # API controllers
    ├── Exceptions/
    │   └── GlobalExceptionHandler.cs
    ├── Extensions/
    │   ├── SerilogExtensions.cs
    │   └── WebApplicationBuilderExtensions.cs  # AddServices() + ConfigurePipeline()
    └── Program.cs

tests/
├── DotnetApi.Domain.Tests/
│   ├── Helpers/            # Shared test utilities (UserFactory, FakeUploadedFile)
│   ├── Entities/           # User entity tests
│   └── Exceptions/         # DomainException, NotFoundException tests
│
├── DotnetApi.Application.Tests/
│   └── Features/
│       ├── Auth/
│       │   └── Commands/
│       │       ├── Login/      # Handler + Validator tests
│       │       └── Register/   # Handler + Validator tests
│       └── Users/
│           ├── Commands/
│           │   ├── ChangeCurrentUserPassword/   # Handler + Validator tests
│           │   ├── UpdateCurrentUser/           # Handler + Validator tests
│           │   └── UploadAvatar/                # Handler + Validator tests
│           └── Queries/
│               └── GetCurrentUser/              # Handler tests
│
└── DotnetApi.Infrastructure.Tests/
    ├── Repositories/       # UserRepository tests (EF InMemory)
    └── Services/           # JwtTokenService, PasswordHasher, LocalFileStorageService tests
```

---

## 3. Inter-Layer Dependency Rules

| Layer | May Reference |
|---|---|
| **Domain** | No other layers |
| **Application** | Domain only |
| **Infrastructure** | Domain + external packages (EF Core, JWT, BCrypt) |
| **API** | Application + Infrastructure (only for DI registration) |

> **Important:** The Application layer **must not** reference Infrastructure. Communication is done through interfaces defined in Domain and implemented in Infrastructure.

---

## 4. Standards for Adding New Features

Every new feature follows this implementation order:

### Step 1 — Domain
If the feature requires a new entity or interface:
- Create the entity in `Domain/Entities/`
- Add the repository/service interface in `Domain/Interfaces/`
- Add enum in `Domain/Enums/` if needed

### Step 2 — Application
Create the feature folder in `Application/Features/{FeatureName}/`:
- Command or Query along with its Validator and Handler
- Add new response DTOs in `Common/Models/` if needed

### Step 3 — Infrastructure
- Create repository implementation in `Infrastructure/Repositories/`
- Create EF Core configuration in `Infrastructure/Persistence/Configurations/`
- Create service implementation in `Infrastructure/Services/` if needed
- **Register** all implementations in `Infrastructure/Extensions/ServiceCollectionExtensions.cs`

### Step 4 — API
- Create controller in `Api/Controllers/`
- Run `dotnet build` to verify

---

## 5. Naming Conventions

### Files & Classes

| Type | Pattern | Example |
|---|---|---|
| Entity | `{Name}.cs` | `User.cs` |
| Interface | `I{Name}.cs` | `IUserRepository.cs` |
| Command | `{Action}{Feature}Command.cs` | `RegisterCommand.cs` |
| Query | `Get{Feature}Query.cs` | `GetCurrentUserQuery.cs` |
| Handler (Command) | `{Action}{Feature}CommandHandler.cs` | `RegisterCommandHandler.cs` |
| Handler (Query) | `Get{Feature}QueryHandler.cs` | `GetCurrentUserQueryHandler.cs` |
| Validator | `{Action}{Feature}CommandValidator.cs` | `RegisterCommandValidator.cs` |
| Repository | `{Name}Repository.cs` | `UserRepository.cs` |
| Service | `{Name}Service.cs` | `JwtTokenService.cs` |
| Options | `{Feature}Options.cs` | `HealthCheckOptions.cs`, `JwtOptions.cs` |
| Controller | `{Feature}Controller.cs` | `UsersController.cs` |
| Configuration (EF) | `{Entity}Configuration.cs` | `UserConfiguration.cs` |

### Namespaces

Always follow the folder structure:

```csharp
// Domain
namespace DotnetApi.Domain.Entities;
namespace DotnetApi.Domain.Interfaces;
namespace DotnetApi.Domain.Enums;
namespace DotnetApi.Domain.Exceptions;

// Application
namespace DotnetApi.Application.Features.{Feature}.Commands.{Action};
namespace DotnetApi.Application.Features.{Feature}.Queries.{ActionName};
namespace DotnetApi.Application.Common.Models;
namespace DotnetApi.Application.Common.Behaviors;
namespace DotnetApi.Application.Common.Exceptions;
namespace DotnetApi.Application.Extensions;

// Infrastructure
namespace DotnetApi.Infrastructure.Repositories;
namespace DotnetApi.Infrastructure.Services;
namespace DotnetApi.Infrastructure.Persistence;
namespace DotnetApi.Infrastructure.Persistence.Configurations;
namespace DotnetApi.Infrastructure.Options;
namespace DotnetApi.Infrastructure.Extensions;

// API
namespace DotnetApi.Api.Controllers;
namespace DotnetApi.Api.Exceptions;
namespace DotnetApi.Api.Extensions;
```

---

## 6. Domain Layer

### Entity

Entities have no public setters. All state changes are done through methods.

```csharp
public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor — always private
    private User() { }

    // Factory method — the only way to create an entity
    public static User Create(string email, string fullName, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        // domain validation...
        return new User { ... };
    }

    // Domain method — the only way to change state
    public void UpdateProfile(string fullName) { ... }
    public void UpdatePassword(string newPasswordHash) { ... }
}
```

**Rules:**
- Always include a `private` constructor for EF Core
- Use `static Create(...)` as the factory method
- Domain validation is done inside the entity; throw `DomainException` on failure
- Methods that change state must always update `UpdatedAt = DateTime.UtcNow`

### Repository Interface

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
}
```

**Rules:**
- All methods are `async` with a defaulted `CancellationToken`
- Return `T?` for queries that may not find data
- Return `T` for write operations (add/update)

---

## 7. Application Layer

### Command

```csharp
// Use record — immutable by design
public record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<AuthResponse>;
```

**Rules:**
- Always use `record` instead of `class`
- Implement `IRequest<TResponse>` for commands that return data
- Implement `IRequest` (non-generic) for commands that return nothing (e.g., delete, change password)
- Do not put `UserId` or JWT-derived IDs in the command — inject them in the controller via `command with { UserId = GetCurrentUserId() }`

### Query

```csharp
public record GetCurrentUserQuery(int UserId) : IRequest<UserResponse>;
```

### Validator

```csharp
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Must contain at least one digit.");
    }
}
```

**Rules:**
- Every command **must** have a validator
- Queries are optional (add one only if parameters need validation)
- Use descriptive `.WithMessage()` on every rule
- **Business rule** validations (e.g., email already registered) are done in the handler, not the validator

### Handler

```csharp
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Business rule check
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ValidationException("Email is already registered.");

        // 2. Domain operation
        var hashedPassword = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, request.FullName, hashedPassword);

        // 3. Persist
        await _userRepository.AddAsync(user, cancellationToken);

        // 4. Return DTO
        var token = _jwtTokenService.GenerateToken(user);
        return new AuthResponse(...);
    }
}
```

**Rules:**
- Handlers may only interact with other layers through **interfaces** (from Domain)
- Do not inject concrete classes from Infrastructure
- Throw `ValidationException` for business rule violations
- Throw `NotFoundException` if an entity is not found
- Always add `/// <inheritdoc/>` above the `Handle` method
- Entity-to-DTO mapping is done in the handler, not in the repository

### Response DTO

```csharp
// Place in Application/Common/Models/
// Use record — immutable
public record UserResponse(
    int Id,
    string Email,
    string FullName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
```

**Rules:**
- All DTOs use `record`
- Do not expose sensitive domain properties (e.g., `Password`)
- `ErrorResponse` already exists in `Common/Models/` — do not create a new one

---

## 8. Infrastructure Layer

### Repository

```csharp
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }
}
```

**Rules:**
- Use `AsNoTracking()` for read-only queries
- Always pass `cancellationToken` to all EF Core methods
- `SaveChangesAsync` is called in the repository (not in the handler)
- Do not apply filtering/sorting outside the repository — query logic lives here

### EF Core Configuration

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");                          // snake_case
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_{table}_{column}");      // index naming convention
    }
}
```

**Rules:**
- Table and column names use `snake_case`
- All columns must be explicitly defined with `HasColumnName`
- Always set `HasMaxLength` for string columns
- Index names follow the pattern `ix_{table}_{column}`
- Nullable columns use `.IsRequired(false)`

### DI Registration

All Infrastructure service registrations are in one place:

```csharp
// Infrastructure/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Database
    services.AddDbContext<ApplicationDbContext>(...);

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();

    // Services
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();

    // Auth
    services.AddAuthentication(...).AddJwtBearer(...);
    services.AddAuthorization();

    return services;
}
```

**Rules:**
- Use `AddScoped` for repositories and services (per-request lifetime)
- Use `AddSingleton` only for services that are truly stateless and thread-safe
- Do not register services in `Program.cs` — everything goes in each layer's extension method

---

## 9. API Layer

### Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]                        // Add if all endpoints require auth
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Brief description of this endpoint
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Description of the return value</returns>
    /// <response code="200">Success description</response>
    /// <response code="401">Unauthorized — valid JWT token required</response>
    [HttpGet("me", Name = "GetCurrentUser")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(GetCurrentUserId()), cancellationToken);
        return Ok(result);
    }
}
```

**Rules:**
- Controllers may only inject `IMediator` — do not inject repositories or services directly
- Always add XML doc (`/// <summary>`) to every action
- Always declare `[ProducesResponseType]` for all possible responses
- Always declare `[HttpGet/Post/Put/Patch/Delete]` with a unique `Name`
- Return `IActionResult` — do not return DTOs directly
- Do not put business logic in controllers

### HTTP Methods Convention

| Operation | Method | Route | Response |
|---|---|---|---|
| Fetch data | `GET` | `/api/{resource}/me` or `/api/{resource}/{id}` | `200 OK` |
| Create data | `POST` | `/api/{resource}` | `201 Created` |
| Update all data | `PUT` | `/api/{resource}/me` | `200 OK` |
| Update partial data | `PATCH` | `/api/{resource}/me/{sub-resource}` | `204 No Content` |
| Delete data | `DELETE` | `/api/{resource}/me` | `204 No Content` |

### Getting User ID from JWT

```csharp
// Always use this helper method — do not parse manually in every action
private int GetCurrentUserId() =>
    int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User identity could not be determined."));
```

### Program.cs

`Program.cs` must remain minimal — no configuration details should be placed here:

```csharp
SerilogExtensions.AddBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServices();      // all DI registrations

    var app = builder.Build();
    app.ConfigurePipeline();    // all middleware

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

---

## 10. Error Handling

Use the existing exceptions — **do not create new ones** unless absolutely necessary:

| Exception | Location | When to Use | HTTP Response |
|---|---|---|---|
| `DomainException` | `Domain/Exceptions/` | Business rule violation inside an entity | `400 Bad Request` |
| `NotFoundException` | `Domain/Exceptions/` | Entity not found in the repository | `404 Not Found` |
| `ValidationException` | `Application/Common/Exceptions/` | Invalid input or business rule violation in a handler | `400 Bad Request` |
| `UnauthorizedAccessException` | .NET built-in | Identity cannot be determined from the token | `401 Unauthorized` |

### Usage Examples

```csharp
// In entity (DomainException)
if (!email.Contains('@'))
    throw new DomainException("Email format is invalid.");

// In handler — not found (NotFoundException)
var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
    ?? throw new NotFoundException(nameof(User), request.Id);

// In handler — business rule (ValidationException)
if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
    throw new ValidationException("Email is already registered.");

// In handler — wrong password (ValidationException)
if (!_passwordHasher.Verify(request.CurrentPassword, user.Password))
    throw new ValidationException("Current password is incorrect.");
```

> The `GlobalExceptionHandler` in the API layer will catch all of the above exceptions and return a consistent JSON response in the `ErrorResponse` format.

---

## 11. Logging

Logging uses **Serilog** configured via `appsettings.json`.

### Log Levels

| Level | When to Use |
|---|---|
| `LogInformation` | Normal flow, incoming/outgoing requests |
| `LogWarning` | Handled exceptions (4xx errors) |
| `LogError` | Unhandled exceptions (5xx errors) |
| `LogFatal` | Application crash during startup |

### Logging in Handlers (via LoggingBehavior)

`LoggingBehavior` automatically logs every request entering the MediatR pipeline. There is no need to inject `ILogger` in every handler unless there is a specific requirement.

### Log Files

Logs are stored in the `logs/` folder with daily rolling:
```
logs/
├── log-20260327.txt
├── log-20260328.txt
└── log-20260329.txt
```

Log file format:
```
[2026-03-29 14:32:01 WRN] TraceId=0HN8K:00000001 Handled exception [400]: One or more validation errors occurred.
```

---

## 12. New Feature Checklist

Use this checklist every time you add a new feature:

### Domain
- [ ] Entity created with `private` constructor and `static Create()`
- [ ] All properties use `private set`
- [ ] Repository/service interface added in `Domain/Interfaces/`
- [ ] Domain exceptions use `DomainException` or `NotFoundException`

### Application
- [ ] Command/Query uses `record`
- [ ] Every command has a validator (`AbstractValidator<T>`)
- [ ] Handler uses `/// <inheritdoc/>` on the `Handle` method
- [ ] Handler only injects interfaces — no concrete classes
- [ ] Entity → DTO mapping is done in the handler
- [ ] Response DTOs use `record` in `Common/Models/`

### Infrastructure
- [ ] Repository implements the interface from Domain
- [ ] EF Core configuration created in `Persistence/Configurations/`
- [ ] Table and column names use `snake_case`
- [ ] New services registered in `Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- [ ] Database migration created if schema changed

### API
- [ ] Controller only injects `IMediator`
- [ ] All actions have XML doc
- [ ] All possible responses documented via `[ProducesResponseType]`
- [ ] `[HttpGet/Post/Put/Patch]` has a unique `Name`
- [ ] `dotnet build` succeeds with no new errors or warnings

### Tests
- [ ] Domain entity tests cover `Create()`, state-change methods, and domain exceptions
- [ ] Validator tests cover both valid and invalid inputs using `TestValidate()`
- [ ] Handler tests cover happy path and all exception paths using NSubstitute mocks
- [ ] Infrastructure tests cover repository CRUD and service behavior
- [ ] `dotnet test` passes with 0 failures

---

## 13. Unit Testing

### Test Project Location

```
tests/
├── DotnetApi.Domain.Tests/         # mirrors DotnetApi.Domain
├── DotnetApi.Application.Tests/    # mirrors DotnetApi.Application
└── DotnetApi.Infrastructure.Tests/ # mirrors DotnetApi.Infrastructure
```

All three test projects are included in `DotnetApi.slnx`. `DotnetApi.Application.Tests` and `DotnetApi.Infrastructure.Tests` both reference `DotnetApi.Domain.Tests` to access the shared helpers (`UserFactory`, `FakeUploadedFile`).

### Packages

| Package | Version | Purpose | Projects |
|---|---|---|---|
| `xunit` | built-in (.NET 10) | Test framework | All |
| `NSubstitute` | 5.3.0 | Mocking interfaces | Application.Tests, Infrastructure.Tests |
| `Shouldly` | 4.2.1 | Expressive assertions (MIT license, free) | All |
| `FluentValidation` | 12.1.1 | `TestValidate()` helper for validator tests | Application.Tests |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.5 | In-memory DB for repository tests | Infrastructure.Tests |
| `Microsoft.Extensions.Configuration` | 10.0.5 | `ConfigurationBuilder` for JWT service tests | Infrastructure.Tests |

### Test Folder Structure

```
tests/
├── DotnetApi.Domain.Tests/
│   ├── Helpers/
│   │   ├── UserFactory.cs          # Creates User instances via reflection
│   │   └── FakeUploadedFile.cs     # Implements IUploadedFile for upload tests
│   ├── Entities/
│   │   └── UserEntityTests.cs
│   └── Exceptions/
│       └── DomainExceptionTests.cs
│
├── DotnetApi.Application.Tests/
│   └── Features/
│       ├── Auth/Commands/
│       │   ├── Login/
│       │   │   ├── LoginCommandHandlerTests.cs
│       │   │   └── LoginCommandValidatorTests.cs
│       │   └── Register/
│       │       ├── RegisterCommandHandlerTests.cs
│       │       └── RegisterCommandValidatorTests.cs
│       └── Users/
│           ├── Commands/
│           │   ├── ChangeCurrentUserPassword/
│           │   │   ├── ChangeCurrentUserPasswordCommandHandlerTests.cs
│           │   │   └── ChangeCurrentUserPasswordCommandValidatorTests.cs
│           │   ├── UpdateCurrentUser/
│           │   │   ├── UpdateCurrentUserCommandHandlerTests.cs
│           │   │   └── UpdateCurrentUserCommandValidatorTests.cs
│           │   └── UploadAvatar/
│           │       ├── UploadAvatarCommandHandlerTests.cs
│           │       └── UploadAvatarCommandValidatorTests.cs
│           └── Queries/GetCurrentUser/
│               └── GetCurrentUserQueryHandlerTests.cs
│
└── DotnetApi.Infrastructure.Tests/
    ├── Repositories/
    │   └── UserRepositoryTests.cs
    └── Services/
        ├── JwtTokenServiceTests.cs
        ├── LocalFileStorageServiceTests.cs
        └── PasswordHasherTests.cs
```

### Test Naming Convention

```
{MethodName}_{Scenario}_Should{ExpectedBehavior}
```

Examples:
```
Create_WithValidInputs_ShouldSetProperties
Handle_WhenEmailAlreadyExists_ShouldThrowValidationException
GenerateToken_ShouldContainCorrectClaims
SaveAsync_ShouldWriteFileToDisk
```

### Helpers

#### UserFactory

Used to create `User` entity instances in tests. Because `User` has private setters and a private constructor, `UserFactory` uses reflection to set properties:

```csharp
public static User Create(
    int id = 1,
    string email = "test@example.com",
    string fullName = "Test User",
    string password = "hashed_password",
    string? avatar = null)
{
    var user = User.Create(email, fullName, password);
    typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);
    if (avatar != null)
        typeof(User).GetProperty(nameof(User.Avatar))!.SetValue(user, avatar);
    return user;
}
```

#### FakeUploadedFile

Implements `IUploadedFile` for testing the avatar upload feature without using `IFormFile`:

```csharp
public sealed class FakeUploadedFile : IUploadedFile
{
    public string FileName { get; init; } = "avatar.jpg";
    public string ContentType { get; init; } = "image/jpeg";
    public long Length { get; init; } = 1024;
    // ...
}
```

### Mocking with NSubstitute

```csharp
// Create mock
private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

// Setup return value
_userRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);

// Setup null return (use type alias to avoid namespace collision)
using User = DotnetApi.Domain.Entities.User;
_userRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
    .Returns((User?)null);

// Verify a call was made
await _userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());

// Verify a call was NOT made
await _fileStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
```

> **Note:** When tests are placed in the `DotnetApi.Tests.Domain` namespace, using `Domain.Entities.User` can cause a namespace ambiguity. Always add the type alias `using User = DotnetApi.Domain.Entities.User;` at the top of handler test files.

### Validator Tests with FluentValidation

```csharp
[Fact]
public void Validate_WithValidData_ShouldNotHaveErrors()
{
    var command = new RegisterCommand("John Doe", "john@example.com", "Password1!", "Password1!");
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
}

[Fact]
public void Validate_WithInvalidEmail_ShouldHaveError()
{
    var command = new RegisterCommand("John", "not-an-email", "Password1!", "Password1!");
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.Email);
}
```

### Infrastructure Tests

Repository tests use EF Core InMemory and implement `IDisposable` to clean up the context:

```csharp
public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _sut = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();
}
```

`LocalFileStorageServiceTests` uses a real temporary directory and cleans it up in `Dispose()`.

### Running Tests

```bash
# Run all tests in the solution
dotnet test

# Run a specific test project
dotnet test tests/DotnetApi.Domain.Tests
dotnet test tests/DotnetApi.Application.Tests
dotnet test tests/DotnetApi.Infrastructure.Tests

# Run with verbose output
dotnet test tests/DotnetApi.Application.Tests --verbosity normal

# Run a specific test class
dotnet test --filter "FullyQualifiedName~UserRepositoryTests"
```