# CQRS Pattern - UML Class Diagram

## Overview
This UML Class Diagram represents the CQRS (Command Query Responsibility Segregation) pattern implementation using MediatR in this project.

## UML Class Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                    MediatR Core Interfaces                                          │
├─────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                     │
│  ┌────────────────────────────┐         ┌────────────────────────────┐                             │
│  │     <<interface>>           │         │     <<interface>>          │                             │
│  │       IRequest<T>           │         │     INotification          │                             │
│  ├────────────────────────────┤         ├────────────────────────────┤                             │
│  │ - (Generic Request Type)    │         │ - (Event Type)             │                             │
│  └────────────────────────────┘         └────────────────────────────┘                             │
│           ▲                                       ▲                                                 │
│           │                                       │                                                 │
│           │                                       │                                                 │
│  ┌────────┴──────────────────────┐    ┌─────────┴─────────────────────┐                           │
│  │                               │    │                               │                            │
│  │ Commands                       │    │ Events                        │                            │
│  │ (Mutate State)                 │    │ (Domain Events)               │                            │
│  │                               │    │                               │                            │
│  └────────────────────────────────┘    └───────────────────────────────┘                           │
│                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                    Commands & Handlers                                              │
├─────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                     │
│  ┌──────────────────────────────────────┐                                                          │
│  │      CreateProductCommand             │                                                          │
│  │  (implements IRequest<Guid>)          │                                                          │
│  ├──────────────────────────────────────┤                                                          │
│  │ - createProductDto: CreateProductDto  │                                                          │
│  └──────────────────────────────────────┘                                                          │
│           │                                                                                         │
│           │ handles                                                                                │
│           ▼                                                                                         │
│  ┌──────────────────────────────────────────────────────┐                                          │
│  │  CreateProductCommandHandler                         │                                          │
│  │ (implements IRequestHandler<CreateProductCommand,   │                                          │
│  │           Guid>)                                     │                                          │
│  ├──────────────────────────────────────────────────────┤                                          │
│  │ - productRepository: IProductRepository              │                                          │
│  │ - mediator: IMediator                                │                                          │
│  ├──────────────────────────────────────────────────────┤                                          │
│  │ + Handle(request, ct): Task<Guid>                    │                                          │
│  │   • Creates new Product entity                       │                                          │
│  │   • Persists to repository                           │                                          │
│  │   • Publishes ProductCreatedEvent notification       │                                          │
│  │   • Returns Product ID                               │                                          │
│  └──────────────────────────────────────────────────────┘                                          │
│                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                     Queries & Handlers                                              │
├─────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                     │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────────┐               │
│  │   GetAllProductsQuery                │  │   GetProductByIdQuery                │               │
│  │ (implements IRequest<List<ProductD│  │ (implements IRequest<ProductDto?>)    │               │
│  │           to>>)                     │  │                                      │               │
│  ├──────────────────────────────────────┤  ├──────────────────────────────────────┤               │
│  │                                      │  │ - id: Guid                           │               │
│  └──────────────────────────────────────┘  └──────────────────────────────────────┘               │
│           │                                          │                                             │
│           │ handles                                  │ handles                                     │
│           ▼                                          ▼                                             │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────────────────┐      │
│  │ GetAllProductsQueryHandler           │  │ GetProductByIdQueryHandler                   │      │
│  │ (implements IRequestHandler<GetAll   │  │ (implements IRequestHandler<GetProductByIdQ  │      │
│  │    ProductsQuery, List<ProductDto>>) │  │      uery, ProductDto?>)                     │      │
│  ├──────────────────────────────────────┤  ├──────────────────────────────────────────────┤      │
│  │ - productRepository:                 │  │ - productRepository: IProductRepository      │      │
│  │   IProductRepository                 │  ├──────────────────────────────────────────────┤      │
│  ├──────────────────────────────────────┤  │ + Handle(request, ct): Task<ProductDto?>    │      │
│  │ + Handle(request, ct):               │  │   • Retrieves product by ID                 │      │
│  │   Task<List<ProductDto>>             │  │   • Maps to DTO if exists                   │      │
│  │   • Retrieves all products           │  │   • Returns null if not found               │      │
│  │   • Maps to ProductDto list          │  └──────────────────────────────────────────────┘      │
│  │   • Returns DTO list                 │                                                        │
│  └──────────────────────────────────────┘                                                        │
│                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                  Domain Model & DTOs                                                │
├─────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                     │
│  ┌──────────────────────────────────────┐  ┌──────────────────────────────────────┐               │
│  │         Product                      │  │      ProductDto                      │               │
│  │     (Domain Entity)                  │  │    (Data Transfer Object)            │               │
│  ├──────────────────────────────────────┤  ├──────────────────────────────────────┤               │
│  │ - Id: Guid                           │  │ - Id: Guid                           │               │
│  │ - Name: string                       │  │ - Name: string                       │               │
│  │ - Price: decimal                     │  │ - Price: decimal                     │               │
│  └──────────────────────────────────────┘  └──────────────────────────────────────┘               │
│           ▲                                                                                        │
│           │                                                                                        │
│           │ maps to                                                                               │
│           │                                                                                        │
│  ┌────────┴──────────────────────────────────────────────────────────────────────────────┐       │
│  │                                                                                         │       │
│  │  ┌──────────────────────────────────────┐                                              │       │
│  │  │  CreateProductDto                    │                                              │       │
│  │  │  (Input DTO)                         │                                              │       │
│  │  ├──────────────────────────────────────┤                                              │       │
│  │  │ - Name: string                       │                                              │       │
│  │  │ - Price: decimal                     │                                              │       │
│  │  └──────────────────────────────────────┘                                              │       │
│  │                                                                                         │       │
│  └─────────────────────────────────────────────────────────────────────────────────────────┘      │
│                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                  Events & Event Handlers                                            │
├─────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐                                  │
│  │           ProductCreatedEvent                                │                                  │
│  │      (implements INotification)                              │                                  │
│  ├──────────────────────────────────────────────────────────────┤                                  │
│  │ - ProductId: Guid                                            │                                  │
│  │ - Name: string                                               │                                  │
│  │ - Price: decimal                                             │                                  │
│  └──────────────────────────────────────────────────────────────┘                                  │
│           │                                                                                         │
│           │ published by CreateProductCommandHandler                                              │
│           │                                                                                         │
│           │ handles                                                                                │
│           ├─────────────────────┬──────────────────────┐                                          │
│           ▼                     ▼                      ▼                                           │
│  ┌────────────────────────────────────────┐  ┌───────────────────────────────────────┐           │
│  │ ProductCreatedEmailHandler             │  │ ProductCreatedAuditHandler            │           │
│  │ (implements INotificationHandler       │  │ (implements INotificationHandler      │           │
│  │  <ProductCreatedEvent>)                │  │  <ProductCreatedEvent>)               │           │
│  ├────────────────────────────────────────┤  ├───────────────────────────────────────┤           │
│  │ - logger: ILogger                      │  │ - logger: ILogger                     │           │
│  ├────────────────────────────────────────┤  ├───────────────────────────────────────┤           │
│  │ + Handle(notification, ct):            │  │ + Handle(notification, ct):           │           │
│  │   Task                                 │  │   Task                                │           │
│  │   • Logs email notification            │  │   • Logs audit information            │           │
│  │   • (Could send actual email)          │  │   • Records product creation event    │           │
│  └────────────────────────────────────────┘  └───────────────────────────────────────┘           │
│                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                 Repository Pattern                                                  │
├─────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐                                  │
│  │        <<interface>>                                         │                                  │
│  │      IProductRepository                                      │                                  │
│  ├──────────────────────────────────────────────────────────────┤                                  │
│  │ + AddAsync(product: Product): Task                           │                                  │
│  │ + GetAllAsync(): Task<List<Product>>                         │                                  │
│  │ + GetByIdAsync(id: Guid): Task<Product?>                     │                                  │
│  └──────────────────────────────────────────────────────────────┘                                  │
│           ▲                                                                                         │
│           │ implements                                                                             │
│           │                                                                                         │
│  ┌────────┴──────────────────────────────────────────────────────────────┐                        │
│  │                                                                         │                        │
│  │  ┌──────────────────────────────────────────────────────────────┐    │                        │
│  │  │        ProductRepository                                     │    │                        │
│  │  │    (Concrete Implementation)                                 │    │                        │
│  │  ├──────────────────────────────────────────────────────────────┤    │                        │
│  │  │ - _products: static List<Product>                            │    │                        │
│  │  ├──────────────────────────────────────────────────────────────┤    │                        │
│  │  │ + AddAsync(product: Product): Task                           │    │                        │
│  │  │   • Adds product to in-memory list                           │    │                        │
│  │  │                                                              │    │                        │
│  │  │ + GetAllAsync(): Task<List<Product>>                         │    │                        │
│  │  │   • Returns all products from list                           │    │                        │
│  │  │                                                              │    │                        │
│  │  │ + GetByIdAsync(id: Guid): Task<Product?>                     │    │                        │
│  │  │   • Finds product by ID                                      │    │                        │
│  │  │   • Returns null if not found                                │    │                        │
│  │  └──────────────────────────────────────────────────────────────┘    │                        │
│  │                                                                         │                        │
│  └─────────────────────────────────────────────────────────────────────────┘                      │
│                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                Pipeline Behaviors (Cross-Cutting Concerns)                         │
├─────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐                                  │
│  │        <<interface>>                                         │                                  │
│  │    IPipelineBehavior<TRequest, TResponse>                    │                                  │
│  ├──────────────────────────────────────────────────────────────┤                                  │
│  │ + Handle(request, next, ct):                                 │                                  │
│  │   Task<TResponse>                                            │                                  │
│  └──────────────────────────────────────────────────────────────┘                                  │
│           ▲                                                                                         │
│           │                                                                                         │
│           │ implements                                                                             │
│           ├──────────────────────────────┬────────────────────────┐                               │
│           │                              │                        │                                │
│  ┌────────▼──────────────────────────┐  ┌▼──────────────────────┐  │                              │
│  │   LoggingBehavior<TRequest,       │  │ValidationBehavior...  │  │                              │
│  │              TResponse>            │  │(mentioned in codebase)│  │                              │
│  ├────────────────────────────────────┤  └──────────────────────┘  │                              │
│  │ - logger: ILogger                 │                             │                              │
│  ├────────────────────────────────────┤                             │                              │
│  │ + Handle(request, next, ct):       │                             │                              │
│  │   Task<TResponse>                 │                             │                              │
│  │   • Logs request name              │                             │                              │
│  │   • Calls next handler             │                             │                              │
│  │   • Logs completion                │                             │                              │
│  └────────────────────────────────────┘                             │                              │
│                                                                      │                              │
└─────────────────────────────────────────────────────────────────────┘                             │
                                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

## Key Components Explained

### 1. **Command Pattern (Write Side)**
- **CreateProductCommand**: Encapsulates the request to create a new product
- **CreateProductCommandHandler**: Processes the command, creates the entity, persists it, and publishes events

### 2. **Query Pattern (Read Side)**
- **GetAllProductsQuery**: Retrieves all products without modifying state
- **GetProductByIdQuery**: Retrieves a specific product by ID
- Both handlers implement read-only operations with no side effects

### 3. **Event-Driven Architecture**
- **ProductCreatedEvent**: Domain event published after successful product creation
- **ProductCreatedEmailHandler**: Handles email notifications
- **ProductCreatedAuditHandler**: Handles audit logging
- Multiple handlers can subscribe to the same event for different concerns

### 4. **Repository Pattern**
- **IProductRepository**: Interface defining data access contracts
- **ProductRepository**: In-memory implementation with Add, GetAll, and GetById operations

### 5. **Cross-Cutting Concerns (Pipeline Behaviors)**
- **LoggingBehavior**: Wraps requests to provide logging
- **ValidationBehavior**: Would validate command/query data before processing

### 6. **Data Transfer Objects (DTOs)**
- **ProductDto**: Output DTO for query results
- **CreateProductDto**: Input DTO for command parameters

## Architecture Benefits

✅ **Separation of Concerns**: Commands handle writes, Queries handle reads  
✅ **Scalability**: Read and write models can be optimized independently  
✅ **Testability**: Each component is independently testable  
✅ **Flexibility**: Event handlers can be added/removed without modifying core logic  
✅ **Maintainability**: Clear responsibility separation makes code easier to understand  
✅ **Performance**: Can implement different storage/caching strategies per read/write model
