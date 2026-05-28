# MediatR CQRS Pattern - Comprehensive Guide

## Table of Contents
1. [Overview](#overview)
2. [CQRS Pattern Fundamentals](#cqrs-pattern-fundamentals)
3. [MediatR Framework](#mediatr-framework)
4. [Project Architecture](#project-architecture)
5. [Core Implementation Details](#core-implementation-details)
6. [Key Features Implemented](#key-features-implemented)
7. [Best Practices & Design Patterns](#best-practices--design-patterns)
8. [Interview Preparation Points](#interview-preparation-points)

---

## Overview

This project demonstrates a professional implementation of the **CQRS (Command Query Responsibility Segregation)** pattern using **MediatR** in a .NET 10 ASP.NET Core application. The implementation focuses on separation of concerns, maintainability, validation, and clean architecture principles.

**Technology Stack:**
- .NET 10.0
- ASP.NET Core
- MediatR 14.1.0
- FluentValidation 12.1.1
- Scalar (OpenAPI/Swagger)

---

## CQRS Pattern Fundamentals

### What is CQRS?

CQRS is an architectural pattern that separates **reading (Query)** and **writing (Command)** operations into different models and handlers.

### Core Principles

1. **Command Side (Write)**
   - Handles operations that **modify state**
   - Examples: Create, Update, Delete
   - Returns minimal data (usually ID or status)
   - May trigger side effects (events, notifications)

2. **Query Side (Read)**
   - Handles operations that **retrieve data**
   - Does not modify state
   - Can be optimized for read performance
   - Returns data transfer objects (DTOs)

### Benefits of CQRS

```
┌─────────────────────────────────────────────────────┐
│              Traditional Monolithic Approach         │
│  ┌──────────────────────────────────────────────┐   │
│  │  Services (Mixed Read & Write Operations)    │   │
│  │  - Complex logic handling both concerns      │   │
│  │  - Difficult to optimize independently       │   │
│  │  - Testing is complex                        │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘

                         vs

┌──────────────────────────────────────────────────────┐
│            CQRS Architecture (Separation)            │
│  ┌────────────────────┐    ┌────────────────────┐   │
│  │  Commands (Write)  │    │   Queries (Read)   │   │
│  │  ┌──────────────┐  │    │  ┌──────────────┐  │   │
│  │  │ CreateProduct│  │    │  │GetAllProducts│  │   │
│  │  │ UpdateProduct│  │    │  │GetProductById│  │   │
│  │  │ DeleteProduct│  │    │  └──────────────┘  │   │
│  │  └──────────────┘  │    │                    │   │
│  │                    │    │ (Optimized for     │   │
│  │ (Business Logic)   │    │  Read Performance) │   │
│  └────────────────────┘    └────────────────────┘   │
│           ↓                          ↓               │
│      Repository/DB              Read Model/Cache    │
└──────────────────────────────────────────────────────┘
```

**Key Advantages:**
- ✅ Independent optimization of read and write operations
- ✅ Simplified testing and debugging
- ✅ Better scalability (can scale reads and writes separately)
- ✅ Clear separation of concerns
- ✅ Easier to maintain and extend

---

## MediatR Framework

### What is MediatR?

MediatR is a .NET library that implements the **Mediator pattern** and **Command Query Responsibility Segregation (CQRS)**. It provides:

1. **Request/Response Messaging**: Decouples senders from handlers
2. **Pipeline Behaviors**: Cross-cutting concerns (logging, validation)
3. **Notifications**: Publish/Subscribe pattern for events
4. **Dependency Injection**: Built-in DI container support

### MediatR Core Concepts

```csharp
// 1. REQUEST (Command or Query)
public record CreateProductCommand(CreateProductDto dto) : IRequest<Guid>;

// 2. HANDLER
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Business logic
        return productId;
    }
}

// 3. MEDIATOR (Central Hub)
var result = await _mediator.Send(new CreateProductCommand(...));

// 4. NOTIFICATION (Event)
public class ProductCreatedEvent : INotification
{
    public Guid ProductId { get; set; }
    // ...
}

// 5. NOTIFICATION HANDLER
public class ProductCreatedEmailHandler : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken ct)
    {
        // Send email
        return Task.CompletedTask;
    }
}
```

### Benefits of MediatR

| Feature | Benefit |
|---------|---------|
| **Decoupling** | Senders don't need to know about handlers |
| **Pipeline Behaviors** | Easy to add cross-cutting concerns |
| **Type-Safe** | Compile-time checking with generics |
| **Testability** | Mock the mediator or individual handlers |
| **Extensibility** | Plugin architecture for behaviors |

---

## Project Architecture

### Directory Structure

```
MediatrCQRS/
├── Application/                          # Application layer
│   ├── Behaviors/                        # Pipeline behaviors
│   │   ├── LoggingBehavior.cs           # Cross-cutting concern: Logging
│   │   └── ValidationBehavior.cs         # Cross-cutting concern: Validation
│   ├── Common/
│   │   └── Interfaces/
│   │       └── IProductRepository.cs     # Repository abstraction
│   ├── Controllers/
│   │   └── ProductsController.cs         # API endpoints
│   ├── Extensions/
│   │   ├── DependencyInjection.cs        # DI container setup
│   │   └── BuilderExtensions.cs          # App builder extensions
│   ├── Features/                         # Feature folder structure
│   │   └── Products/
│   │       ├── Commands/
│   │       │   ├── CreateProductCommand.cs
│   │       │   └── CreateProductCommandValidator.cs
│   │       ├── Queries/
│   │       │   ├── GetAllProductsQuery.cs
│   │       │   └── GetProductByIdQuery.cs
│   │       └── Events/
│   │           └── ProductCreatedEvent.cs
│   └── Middlewares/
│       └── ExceptionMiddleware.cs        # Global error handling
├── Domain/                               # Domain layer
│   ├── Dtos/                             # Data Transfer Objects
│   │   ├── CreateProductDto.cs
│   │   └── ProductDto.cs
│   └── Entities/                         # Domain entities
│       └── Product.cs
├── Infrastructure/                       # Infrastructure layer
│   └── Repositories/
│       └── ProductRepository.cs          # Data access implementation
├── Program.cs                            # Application entry point
├── appsettings.json                      # Configuration
└── MediatrCQRS.csproj                    # Project file
```

### Layered Architecture Pattern

```
┌────────────────────────────────────────────────────┐
│         Presentation Layer (Controllers)            │
│    ProductsController → Uses IMediator             │
└────────────────────────────────────────────────────┘
                         ↓
┌────────────────────────���───────────────────────────┐
│    Application Layer (Commands, Queries, Handlers)  │
│  ├─ Commands: CreateProductCommand                │
│  ├─ Queries: GetProductByIdQuery                  │
│  ├─ Behaviors: Logging, Validation                │
│  └─ Events: ProductCreatedEvent                   │
└────────────────────────────────────────────────────┘
                         ↓
┌────────────────────────────────────────────────────┐
│       Domain Layer (Entities, DTOs, Interfaces)     │
│  ├─ Entities: Product                             │
│  ├─ DTOs: ProductDto, CreateProductDto            │
│  └─ Interfaces: IProductRepository                │
└────────────────────────────────────────────────────┘
                         ↓
┌────────────────────────────────────────────────────┐
│    Infrastructure Layer (Data Access)              │
│  └─ Repositories: ProductRepository               │
│     (In-memory storage in this example)            │
└────────────────────────────────────────────────────┘
```

---

## Core Implementation Details

### 1. Domain Layer

#### Entity Definition
```csharp
// Domain/Entities/Product.cs
namespace Domain.Entites;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**Purpose:** Core business object with minimal logic, representing a Product in the system.

#### Data Transfer Objects (DTOs)
```csharp
// Domain/Dtos/CreateProductDto.cs
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Domain/Dtos/ProductDto.cs
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**Purpose:** 
- `CreateProductDto`: Input model for creating products
- `ProductDto`: Output model for returning product data (read model)

**Why DTOs?**
- Decouple API contracts from domain models
- Prevent exposing internal implementation details
- Allow independent evolution of API and domain models

---

### 2. Application Layer - Commands

#### CreateProductCommand (Request)
```csharp
// Application/Features/Products/Commands/CreateProductCommand.cs
public sealed record CreateProductCommand(CreateProductDto createProductDto) 
    : IRequest<Guid>;
```

**Details:**
- Uses C# **record type** (immutable by default)
- Implements `IRequest<Guid>` (MediatR interface)
- Returns the product ID upon successful creation
- Acts as a data carrier and command definition

#### CreateProductCommandHandler (Handler)
```csharp
public class CreateProductCommandHandler(
    IProductRepository productRepository, 
    IMediator mediator) 
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // 1. Create entity from DTO
        var product = new Product 
        { 
            Id = Guid.NewGuid(), 
            Name = request.createProductDto.Name, 
            Price = request.createProductDto.Price 
        };

        // 2. Persist to repository
        await productRepository.AddAsync(product);

        // 3. Publish domain event (notification)
        await mediator.Publish(
            new ProductCreatedEvent(product.Id, product.Name, product.Price));

        // 4. Return newly created product ID
        return product.Id;
    }
}
```

**Key Responsibilities:**
1. **Orchestration**: Coordinates business logic
2. **Domain Logic**: Creates domain entities
3. **Persistence**: Delegates to repository
4. **Event Publishing**: Triggers side effects via notifications

#### CreateProductCommandValidator (Fluent Validation)
```csharp
public class CreateProductCommandValidator 
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.createProductDto.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(100);

        RuleFor(x => x.createProductDto.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero");
    }
}
```

**Features:**
- Uses **FluentValidation** for fluent, readable validation
- Automatically discovered by the validation behavior
- Validates business rules (non-null, max length, price > 0)

---

### 3. Application Layer - Queries

#### GetProductByIdQuery (Request)
```csharp
public sealed record GetProductByIdQuery(Guid id) 
    : IRequest<ProductDto?>;
```

**Characteristics:**
- Immutable record type
- Takes product ID as input
- Returns nullable ProductDto (handles not-found case)
- No side effects

#### GetProductByIdQueryHandler (Handler)
```csharp
public class GetProductByIdQueryHandler(
    IProductRepository productRepository) 
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request, 
        CancellationToken ct)
    {
        // 1. Fetch from repository
        var product = await productRepository.GetByIdAsync(request.id);

        // 2. Return null if not found
        if (product is null) return null;

        // 3. Map to DTO
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}
```

**Design Pattern: Null Object Pattern**
- Returns `null` instead of throwing exceptions
- Allows controller to handle gracefully with appropriate HTTP status

#### GetAllProductsQuery (Request)
```csharp
public sealed record GetAllProductsQuery 
    : IRequest<List<ProductDto>>;
```

#### GetAllProductsQueryHandler (Handler)
```csharp
public class GetAllProductsQueryHandler(
    IProductRepository productRepository) 
    : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(
        GetAllProductsQuery request, 
        CancellationToken ct)
    {
        var products = await productRepository.GetAllAsync();

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        }).ToList();

        return productDtos;
    }
}
```

**Key Point:** Queries return read models (DTOs) optimized for UI consumption.

---

### 4. Application Layer - Events (Notifications)

#### ProductCreatedEvent
```csharp
public class ProductCreatedEvent : INotification
{
    public Guid ProductId { get; }
    public string Name { get; }
    public decimal Price { get; }

    public ProductCreatedEvent(Guid productId, string name, decimal price)
    {
        ProductId = productId;
        Name = name;
        Price = price;
    }
}
```

**Purpose:**
- Represents domain event triggered after product creation
- Decouples side effects from command handler
- Enables multiple subscribers (email, audit, analytics, etc.)

#### Event Handlers

**ProductCreatedEmailHandler**
```csharp
public class ProductCreatedEmailHandler 
    : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEmailHandler> _logger;

    public ProductCreatedEmailHandler(
        ILogger<ProductCreatedEmailHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        ProductCreatedEvent notification, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "EMAIL: Product created: {ProductName}", 
            notification.Name);
        return Task.CompletedTask;
    }
}
```

**ProductCreatedAuditHandler**
```csharp
public class ProductCreatedAuditHandler 
    : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedAuditHandler> _logger;

    public ProductCreatedAuditHandler(
        ILogger<ProductCreatedAuditHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        ProductCreatedEvent notification, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AUDIT: Product created with ID: {ProductId}", 
            notification.ProductId);
        return Task.CompletedTask;
    }
}
```

**Event Handling Flow:**
```
CreateProductCommand
        ↓
CreateProductCommandHandler
        ↓
await mediator.Publish(ProductCreatedEvent)
        ↓ (Parallel Execution)
    ┌───────────────────────────────────────┐
    │                                       │
    ↓                                       ↓
ProductCreatedEmailHandler          ProductCreatedAuditHandler
(Send Notification Email)            (Log Audit Trail)
```

**Benefits:**
- **Loose Coupling**: Email and audit logic separated
- **Extensibility**: Add new handlers without modifying command handler
- **Single Responsibility**: Each handler has one concern
- **Fire & Forget**: Handlers execute independently

---

### 5. Application Layer - Pipeline Behaviors

#### ValidationBehavior
```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Find all validators for this request type
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            // 2. Run all validators in parallel
            var validationResults = await Task.WhenAll(
                _validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            // 3. Collect all validation failures
            var failures = validationResults
                .Where(r => !r.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            // 4. Throw if any validation failed
            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        // 5. Proceed to handler if validation passes
        return await next();
    }
}
```

**Pipeline Architecture:**
```
HTTP Request (ProductsController)
        ↓
_mediator.Send(CreateProductCommand)
        ↓
ValidationBehavior (Intercepts)
  ├─ Checks if validators exist
  ├─ Runs all validators in parallel
  ├─ Throws ValidationException if failures
  └─ Calls next() if valid
        ↓
LoggingBehavior (Intercepts)
  ├─ Logs "Handling request: CreateProductCommand"
  └─ Calls next()
        ↓
CreateProductCommandHandler (Actual Handler)
  ├─ Creates product
  ├─ Persists to repository
  └─ Publishes ProductCreatedEvent
        ↓
LoggingBehavior (After Handler)
  └─ Logs "Completed request: CreateProductCommand"
        ↓
HTTP Response
```

#### LoggingBehavior
```csharp
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Log before execution
        _logger.LogInformation("Handling request: {RequestName}", requestName);

        var response = await next();

        // Log after execution
        _logger.LogInformation("Completed request: {RequestName}", requestName);

        return response;
    }
}
```

**Key Concepts:**
- **Pipeline Pattern**: Behaviors wrap request processing
- **Cross-Cutting Concerns**: Logging and validation without cluttering handlers
- **Reusability**: Applies to all commands and queries
- **Order Matters**: Validators run before logging in this setup

---

### 6. Infrastructure Layer

#### IProductRepository (Interface)
```csharp
public interface IProductRepository
{
    Task AddAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetAllAsync();
}
```

**Design Pattern: Repository Pattern**
- Abstracts data access logic
- Allows swapping implementations (in-memory, SQL, NoSQL)
- Simplifies testing via mocking

#### ProductRepository (Implementation)
```csharp
public class ProductRepository : IProductRepository
{
    // In-memory storage (for demo purposes)
    private static readonly List<Product> _products = [];

    public Task AddAsync(Product product)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task<List<Product>> GetAllAsync()
    {
        return Task.FromResult(_products);
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }
}
```

**Current Implementation:**
- Uses `List<Product>` for in-memory storage
- Thread-unsafe (suitable for single-request scenarios)

**Real-World Alternatives:**
```csharp
// SQL Database
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }
}

// MongoDB
public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _collection;
    
    public async Task AddAsync(Product product)
    {
        await _collection.InsertOneAsync(product);
    }
}
```

---

### 7. Dependency Injection & Configuration

#### DependencyInjection.cs
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // 1. Register Repository
        services.AddSingleton<IProductRepository, ProductRepository>();

        // 2. Register MediatR (Handlers, Behaviors)
        services.AddMediatR(cfg =>
        {   
            cfg.RegisterServicesFromAssembly(
                Assembly.GetExecutingAssembly());
        });

        // 3. Register Logging Behavior
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>)
        );

        // 4. Register FluentValidation Validators
        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly());

        // 5. Register Validation Behavior
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );

        return services;
    }
}
```

**Registration Breakdown:**

| Service | Lifetime | Reason |
|---------|----------|--------|
| `IProductRepository` | Singleton | In-memory data consistency |
| `MediatR Handlers` | Auto-registered | Per request pattern |
| `LoggingBehavior` | Transient | New instance per request |
| `ValidationBehavior` | Transient | New instance per request |

#### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Core services
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddApplicationServices();  // Custom extension

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseApplicationMiddleware();  // Exception handling
app.UseHttpsRedirection();

app.Run();
```

---

### 8. Presentation Layer (API Controllers)

#### ProductsController
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Query: Get single product
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(product);
    }

    // Query: Get all products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _mediator.Send(new GetAllProductsQuery());
        return Ok(products);
    }

    // Command: Create product
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto createProductDto)
    {
        var productId = await _mediator.Send(
            new CreateProductCommand(createProductDto));
        return Ok(productId);
    }
}
```

**Controller Responsibilities:**
1. **Route Mapping**: Define HTTP endpoints
2. **Model Binding**: Convert HTTP requests to DTOs
3. **Mediator Delegation**: Dispatch to MediatR
4. **Response Mapping**: Convert domain responses to HTTP responses

**Why Thin Controllers?**
- Business logic moved to handlers
- Controllers become simple routing layers
- Easier to test handlers independently

---

### 9. Error Handling & Middleware

#### ExceptionMiddleware
```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleServerException(context, ex);
        }
    }

    // Handle validation errors (400)
    private static async Task HandleValidationException(
        HttpContext context, 
        ValidationException ex)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var response = new
        {
            Title = "Validation Failed",
            Errors = ex.Errors.Select(x => new
            {
                x.PropertyName,
                x.ErrorMessage
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }

    // Handle server errors (500)
    private static async Task HandleServerException(
        HttpContext context, 
        Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            Title = "Server Error",
            Detail = ex.Message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}
```

**Error Handling Flow:**
```
ValidationBehavior throws ValidationException
            ↓
ExceptionMiddleware catches it
            ↓
HandleValidationException (400 Bad Request)
    {
        "Title": "Validation Failed",
        "Errors": [
            {
                "PropertyName": "createProductDto.Price",
                "ErrorMessage": "Price must be greater than zero"
            }
        ]
    }
```

---

## Key Features Implemented

### 1. **Complete CQRS Separation**
- ✅ Commands: `CreateProductCommand` (write operation)
- ✅ Queries: `GetProductByIdQuery`, `GetAllProductsQuery` (read operations)
- ✅ Independent optimization possible
- ✅ Clear intent in code

### 2. **MediatR Integration**
- ✅ Request/Response pattern with `IRequest<T>`
- ✅ Handlers implementing `IRequestHandler<TRequest, TResponse>`
- ✅ Notifications with `INotification` and `INotificationHandler<T>`
- ✅ Pipeline behaviors for cross-cutting concerns

### 3. **Fluent Validation**
- ✅ `CreateProductCommandValidator` for input validation
- ✅ Business rule enforcement (price > 0, name not empty)
- ✅ Declarative validation rules
- ✅ Automatic discovery and execution

### 4. **Pipeline Behaviors (Middleware Pattern)**
- ✅ **ValidationBehavior**: Validates all requests
- ✅ **LoggingBehavior**: Logs request/response lifecycle
- ✅ Transparent cross-cutting concerns
- ✅ Composable and reusable

### 5. **Domain Events & Notifications**
- ✅ `ProductCreatedEvent` published after command execution
- ✅ Multiple event subscribers without coupling
- ✅ `ProductCreatedEmailHandler` for notifications
- ✅ `ProductCreatedAuditHandler` for audit logging
- ✅ Fire-and-forget async execution

### 6. **Repository Pattern**
- ✅ Abstract `IProductRepository` interface
- ✅ Decoupled data access logic
- ✅ Easy to mock for testing
- ✅ Swappable implementations (SQL, NoSQL, etc.)

### 7. **Clean Architecture**
- ✅ Clear separation: Domain → Application → Infrastructure
- ✅ DTOs prevent domain model leakage
- ✅ Dependency injection for loose coupling
- ✅ Single Responsibility Principle

### 8. **Global Exception Handling**
- ✅ Middleware-based exception catching
- ✅ Validation errors (400 Bad Request)
- ✅ Server errors (500 Internal Server Error)
- ✅ Consistent error response format

### 9. **Async/Await Throughout**
- ✅ All handlers are async
- ✅ Non-blocking I/O operations
- ✅ `CancellationToken` support for request cancellation
- ✅ Proper async composition

### 10. **Dependency Injection Configuration**
- ✅ Extension method pattern for clean setup
- ✅ Reflection-based automatic registration
- ✅ Proper lifetime management (Singleton, Transient)
- ✅ Centralized configuration

---

## Best Practices & Design Patterns

### 1. **Mediator Pattern**
```csharp
// Without Mediator (Tight Coupling)
public class ProductsController
{
    private readonly CreateProductCommandHandler _handler1;
    private readonly GetProductByIdQueryHandler _handler2;
    private readonly IProductRepository _repository;
    
    public async Task CreateProduct(CreateProductDto dto)
    {
        // Direct dependency on multiple handlers
        return await _handler1.Handle(new CreateProductCommand(dto), CancellationToken.None);
    }
}

// With Mediator (Loose Coupling)
public class ProductsController
{
    private readonly IMediator _mediator;
    
    public async Task CreateProduct(CreateProductDto dto)
    {
        // Single dependency, handler discovery automatic
        return await _mediator.Send(new CreateProductCommand(dto));
    }
}
```

### 2. **Repository Pattern**
Abstracts data access, making the system testable and flexible.

```csharp
// Unit Test Example
[TestFixture]
public class CreateProductCommandHandlerTests
{
    private Mock<IProductRepository> _mockRepository;
    private Mock<IMediator> _mockMediator;
    private CreateProductCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockMediator = new Mock<IMediator>();
        _handler = new CreateProductCommandHandler(_mockRepository.Object, _mockMediator.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_CreatesProduct()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Widget", Price = 9.99m };
        var command = new CreateProductCommand(dto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<ProductCreatedEvent>(), CancellationToken.None), Times.Once);
        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }
}
```

### 3. **Decorator Pattern (Pipeline Behaviors)**
Behaviors wrap request handling without modifying handler code.

```csharp
// Adding a new behavior (e.g., Caching)
public class CachingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Check cache, return if hit
        var cacheKey = GenerateKey(request);
        if (_cache.TryGetValue(cacheKey, out var cached))
            return (TResponse)cached;

        // Call next handler
        var result = await next();

        // Store in cache
        _cache.Set(cacheKey, result);
        return result;
    }
}

// Register in DependencyInjection.cs
services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(CachingBehavior<,>)
);
```

### 4. **Observer Pattern (Domain Events)**
Multiple handlers subscribe to a single event.

```csharp
// Event Publisher (Command Handler)
await mediator.Publish(new ProductCreatedEvent(...));

// Multiple Subscribers
public class ProductCreatedEmailHandler : INotificationHandler<ProductCreatedEvent> { ... }
public class ProductCreatedAuditHandler : INotificationHandler<ProductCreatedEvent> { ... }
public class ProductCreatedAnalyticsHandler : INotificationHandler<ProductCreatedEvent> { ... }
public class ProductCreatedInventoryHandler : INotificationHandler<ProductCreatedEvent> { ... }
```

### 5. **SOLID Principles Application**

| Principle | Implementation |
|-----------|-----------------|
| **Single Responsibility** | Each handler has one job (Create, Query, Log, Validate) |
| **Open/Closed** | Add behaviors/handlers without modifying existing code |
| **Liskov Substitution** | All handlers implement `IRequestHandler<>` interchangeably |
| **Interface Segregation** | Handlers depend on `IProductRepository`, not concrete implementation |
| **Dependency Inversion** | Depend on `IMediator`, not concrete mediator |

---

## Interview Preparation Points

### Q1: What is CQRS, and why would you use it?

**Answer:**
CQRS (Command Query Responsibility Segregation) separates read (query) and write (command) operations into distinct models and handlers.

**Why use it:**
1. **Independent Scaling**: Read and write operations can be scaled separately
2. **Performance Optimization**: Queries can use read-optimized models (caching, denormalization)
3. **Clear Intent**: Code explicitly shows whether operation modifies state
4. **Simplified Testing**: Handlers are independently testable
5. **Complex Domain**: Helps manage complexity in large systems

**Real-World Example:**
E-commerce platform where product listing (reads) happens 1000x more frequently than inventory updates (writes). CQRS allows separate databases: read replicas for queries, optimized write DB for commands.

---

### Q2: Explain the Mediator Pattern and its benefits.

**Answer:**
The Mediator Pattern defines an object that encapsulates how a set of objects interact, promoting loose coupling.

**Benefits:**
1. **Decoupling**: Objects don't need to know about each other directly
2. **Centralized Control**: Communication logic in one place
3. **Reusability**: Same handlers can be used with different mediators
4. **Testability**: Mock the mediator to test in isolation

**In MediatR:**
```csharp
// Without Mediator: Controller knows about all handlers
var handler1 = new CreateProductCommandHandler(...);
var handler2 = new GetProductByIdQueryHandler(...);
var result = handler1.Handle(command, ct);

// With Mediator: Controller only knows about IMediator
var result = _mediator.Send(new CreateProductCommand(...));
// Mediator internally discovers and invokes the handler
```

---

### Q3: What are Pipeline Behaviors, and how do they work?

**Answer:**
Pipeline Behaviors are interceptors that wrap request handling, implementing the Decorator Pattern.

**How they work:**
```
Request → Behavior1 (Before) → Behavior2 (Before) → Handler → Behavior2 (After) → Behavior1 (After) → Response
```

**Use Cases:**
1. **Logging**: Log request/response
2. **Validation**: Validate input before handler
3. **Caching**: Check cache before executing handler
4. **Authorization**: Authorize request before processing
5. **Transaction Management**: Wrap handler in transaction

**Code Example:**
```csharp
public class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {RequestName}", typeof(TRequest).Name);
        var response = await next();  // Call next in pipeline
        _logger.LogInformation("Completed {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

---

### Q4: How does validation work in this implementation?

**Answer:**
Validation happens through a combination of FluentValidation and a ValidationBehavior:

1. **Validator Definition**: `CreateProductCommandValidator` defines rules
2. **Automatic Discovery**: `AddValidatorsFromAssembly()` finds all validators
3. **Behavior Interception**: ValidationBehavior runs before handler
4. **Parallel Execution**: All validators for a request run in parallel
5. **Error Aggregation**: All failures collected and thrown as `ValidationException`

**Advantages:**
- Validators are reusable
- Validation logic separate from handler
- Rules are testable independently
- Can add new rules without touching handler

---

### Q5: What's the difference between Commands and Queries?

**Answer:**

| Aspect | Command | Query |
|--------|---------|-------|
| **Purpose** | Modifies state | Retrieves data |
| **Returns** | Minimal (ID, status) | Data (DTOs) |
| **Side Effects** | Yes (triggers events) | No |
| **Caching** | Not recommended | Recommended |
| **Idempotence** | Not guaranteed | Idempotent |
| **Examples** | CreateProduct, DeleteOrder | GetProduct, ListOrders |

**Example:**
```csharp
// COMMAND: Modifies state, publishes event
public sealed record CreateProductCommand(CreateProductDto dto) : IRequest<Guid>;

// QUERY: Reads state, no modifications
public sealed record GetProductByIdQuery(Guid id) : IRequest<ProductDto?>;
```

---

### Q6: Explain domain events and their purpose.

**Answer:**
Domain events represent something significant that happened in the domain, like "ProductCreated" or "OrderShipped".

**Purpose:**
1. **Decoupling**: Command handler doesn't know about side effects
2. **Auditability**: Complete history of what happened
3. **Real-Time Updates**: Subscribers can react immediately
4. **Integration**: Other systems can subscribe to events

**Example Flow:**
```csharp
// 1. Command Handler publishes event
await mediator.Publish(new ProductCreatedEvent(productId, name, price));

// 2. Multiple handlers react independently
public class ProductCreatedEmailHandler : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent @event, CancellationToken ct)
    {
        // Send email
        return Task.CompletedTask;
    }
}

public class ProductCreatedAnalyticsHandler : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent @event, CancellationToken ct)
    {
        // Track analytics
        return Task.CompletedTask;
    }
}
```

---

### Q7: How would you handle errors and exceptions?

**Answer:**
The implementation uses global exception handling via middleware:

1. **Validation Errors** (400):
   - Thrown by ValidationBehavior
   - Caught by ExceptionMiddleware
   - Returns structured error response

2. **Server Errors** (500):
   - Unhandled exceptions
   - Caught by ExceptionMiddleware
   - Returns error details

**Example Error Response:**
```json
{
    "Title": "Validation Failed",
    "Errors": [
        {
            "PropertyName": "Name",
            "ErrorMessage": "Product name is required"
        }
    ]
}
```

**Extension: Custom Exceptions**
```csharp
public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId) 
        : base($"Product {productId} not found") { }
}

// In middleware
catch (ProductNotFoundException ex)
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
}
```

---

### Q8: How would you extend this architecture with caching?

**Answer:**
Add a caching behavior to the pipeline:

```csharp
public class CachingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only cache queries
        if (request is not IQuery)
        {
            return await next();
        }

        var cacheKey = GenerateCacheKey(request);
        var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (cachedResult != null)
        {
            _logger.LogInformation("Cache hit for {RequestName}", typeof(TRequest).Name);
            return JsonSerializer.Deserialize<TResponse>(cachedResult);
        }

        var result = await next();
        
        await _cache.SetStringAsync(
            cacheKey, 
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) },
            cancellationToken);

        return result;
    }

    private string GenerateCacheKey(TRequest request) => 
        $"{typeof(TRequest).Name}_{JsonSerializer.Serialize(request)}";
}

// Register in DependencyInjection.cs
services.AddStackExchangeRedisCache(options => options.Configuration = "localhost:6379");
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
```

---

### Q9: How would you add authorization?

**Answer:**
```csharp
public class AuthorizationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISecuredCommand
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        if (!user.IsInRole(request.RequiredRole))
        {
            throw new UnauthorizedAccessException($"User lacks {request.RequiredRole} role");
        }

        return await next();
    }
}

// Mark commands requiring auth
public interface ISecuredCommand
{
    string RequiredRole { get; }
}

// Implement in command
public sealed record CreateProductCommand(CreateProductDto dto) 
    : IRequest<Guid>, ISecuredCommand
{
    public string RequiredRole => "Admin";
}
```

---

### Q10: How would you test this architecture?

**Answer:**
The architecture is highly testable:

```csharp
[TestFixture]
public class CreateProductCommandHandlerTests
{
    private Mock<IProductRepository> _repositoryMock;
    private Mock<IMediator> _mediatorMock;
    private CreateProductCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new CreateProductCommandHandler(_repositoryMock.Object, _mediatorMock.Object);
    }

    [Test]
    public async Task Handle_WithValidDto_CreatesProductAndPublishesEvent()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Widget", Price = 10.00m };
        var command = new CreateProductCommand(dto);
        var productId = Guid.NewGuid();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Product>(p => 
                p.Name == "Widget" && p.Price == 10.00m)), 
            Times.Once);

        _mediatorMock.Verify(
            m => m.Publish(
                It.IsAny<ProductCreatedEvent>(), 
                It.IsAny<CancellationToken>()), 
            Times.Once);

        Assert.That(result, Is.Not.EqualTo(Guid.Empty));
    }
}

// Test Validator
[TestFixture]
public class CreateProductCommandValidatorTests
{
    private CreateProductCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateProductCommandValidator();
    }

    [Test]
    public async Task Validate_WithEmptyName_ReturnsError()
    {
        var command = new CreateProductCommand(
            new CreateProductDto { Name = "", Price = 10.00m });

        var result = await _validator.ValidateAsync(command);

        Assert.IsFalse(result.IsValid);
        Assert.That(result.Errors, Has.One.Matches<ValidationFailure>(
            f => f.PropertyName.Contains("Name")));
    }

    [Test]
    public async Task Validate_WithNegativePrice_ReturnsError()
    {
        var command = new CreateProductCommand(
            new CreateProductDto { Name = "Widget", Price = -10.00m });

        var result = await _validator.ValidateAsync(command);

        Assert.IsFalse(result.IsValid);
    }
}
```

---

## Summary

This MediatR CQRS implementation demonstrates:

✅ **Clean Architecture**: Clear separation of concerns across layers
✅ **CQRS Pattern**: Commands and queries handled independently
✅ **MediatR Benefits**: Decoupled request handling with pipeline behaviors
✅ **Validation**: Fluent validation with automatic discovery
✅ **Error Handling**: Global exception handling middleware
✅ **Domain Events**: Event-driven architecture with multiple subscribers
✅ **Repository Pattern**: Abstracted data access for flexibility
✅ **Testability**: Highly mockable components with dependency injection
✅ **Best Practices**: SOLID principles, design patterns, async/await
✅ **Production Ready**: Exception handling, logging, middleware pipeline

This architecture scales well for complex applications and serves as a foundation for enterprise-level systems.

---

**For Interview Success**: Understand each component's responsibility, be able to explain the flow of a request through the pipeline, and be prepared to discuss trade-offs and extensions (caching, authorization, event sourcing, SAGA pattern, etc.).
