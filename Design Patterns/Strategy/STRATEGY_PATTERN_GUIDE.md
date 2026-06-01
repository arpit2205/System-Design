# The Strategy Pattern: Comprehensive Guide & Implementation Analysis

## Table of Contents
1. [Pattern Overview](#pattern-overview)
2. [Real-World Use Cases](#real-world-use-cases)
3. [Implementation Structure](#implementation-structure)
4. [Your Implementation Breakdown](#your-implementation-breakdown)
5. [Key Design Principles](#key-design-principles)
6. [Interview Q&A](#interview-qa)
7. [Common Pitfalls & Best Practices](#common-pitfalls--best-practices)

---

## Pattern Overview

### Definition
The **Strategy Pattern** is a behavioral design pattern that defines a family of algorithms, encapsulates each one, and makes them interchangeable. It lets the algorithm vary independently from clients that use it.

### Core Concept
Instead of implementing different algorithms directly in a class (using if/else or switch statements), you:
- Define separate strategy interfaces
- Create concrete implementations for each algorithm
- Allow clients to choose which strategy to use at runtime
- Keep the client code unaware of the specific algorithm being used

### UML Diagram Concept
```
┌─────────────┐
│   Context   │
│  (Service)  │
└──────┬──────┘
       │ uses
       ▼
┌──────────────────┐
│ <<interface>>    │
│   Strategy       │
│ ─────────────────│
│ + Execute()      │
└────────┬─────────┘
         │ implements
    ┌────┴──────────────┐
    ▼                   ▼
┌─────────────┐   ┌──────────────┐
│ Strategy A  │   │  Strategy B  │
│ + Execute() │   │  + Execute() │
└─────────────┘   └──────────────┘
```

---

## Real-World Use Cases

| Use Case | Example | Benefit |
|----------|---------|---------|
| **Payment Processing** | Credit Card, UPI, Wallet, Bank Transfer | Choose payment method at runtime |
| **Sorting Algorithms** | Quicksort, Mergesort, Bubblesort | Select based on data size |
| **Compression** | ZIP, RAR, GZIP | Dynamically change compression method |
| **Authentication** | OAuth, JWT, Basic Auth, SAML | Pick auth strategy per request |
| **Notification** | Email, SMS, Push Notification | Route message through different channels |
| **Pricing Rules** | Student Discount, Senior Discount, Seasonal Sale | Apply different pricing strategies |

---

## Implementation Structure

### Component Breakdown

#### 1. **Strategy Interface (Contract Definition)**
```
IPaymentStrategy
├── PaymentMethod (property identifying the strategy)
└── ProcessPayment() (algorithm implementation)
```

#### 2. **Concrete Strategies (Algorithm Implementations)**
```
CreditCardPaymentStrategy ─┐
                           ├─ implements IPaymentStrategy
UpiPaymentStrategy ────────┤
                           ├─ (more can be added)
[Future: WalletPaymentStrategy]
```

#### 3. **Strategy Resolver (Runtime Selection)**
```
IPaymentStrategyResolver
└── ResolveStrategy(paymentMethod)
    └── Returns appropriate IPaymentStrategy instance
```

#### 4. **Service Layer (Context/Client)**
```
IPaymentService (interface)
└── PaymentService (implementation)
    ├── Uses IPaymentStrategyResolver
    ├── Delegates to resolved strategy
    └── Returns result to caller
```

#### 5. **Dependency Injection Setup**
```
Program.cs (Composition Root)
├── Register all strategies
├── Register resolver
└── Register service
```

---

## Your Implementation Breakdown

### 1. **Model Classes** (Data Contracts)

```csharp
// PaymentRequest.cs - Input contract
public class PaymentRequest
{
    public Guid UserId { get; set; }        // Who's paying
    public decimal Amount { get; set; }     // How much
}

// PaymentResult.cs - Output contract
public class PaymentResult
{
    public bool Success { get; set; }           // Transaction status
    public Guid TransactionId { get; set; }     // Unique identifier
    public string Message { get; set; }         // Status message
}
```

**Interview Point:** These models are decoupled from strategies—each strategy returns the same result type regardless of implementation.

---

### 2. **Strategy Contracts (Abstractions)**

```csharp
// IPaymentStrategy.cs - Strategy interface
public interface IPaymentStrategy
{
    string PaymentMethod { get; }  // Identifier for resolver
    Task<PaymentResult> ProcessPayment(
        PaymentRequest request, 
        CancellationToken cancellationToken
    );
}
```

**Interview Point:** The `PaymentMethod` property is crucial—it acts as a key for the resolver to match strategies with payment method names.

---

### 3. **Concrete Strategy Implementations**

#### Credit Card Strategy
```csharp
public class CreditCardPaymentStrategy : IPaymentStrategy
{
    public string PaymentMethod => "Credit card";  // Unique identifier

    public Task<PaymentResult> ProcessPayment(
        PaymentRequest request, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Processing {PaymentMethod} payment...");
        Console.WriteLine($"Amount: {request.Amount} [UserId: {request.UserId}]");

        var result = new PaymentResult 
        { 
            TransactionId = Guid.NewGuid(), 
            Success = true, 
            Message = "Credit card transaction completed successfully" 
        };

        return Task.FromResult(result);
    }
}
```

#### UPI Strategy
```csharp
public class UpiPaymentStrategy : IPaymentStrategy
{
    public string PaymentMethod => "UPI";  // Different identifier

    public Task<PaymentResult> ProcessPayment(
        PaymentRequest request, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Processing {PaymentMethod} payment...");
        Console.WriteLine($"Amount: {request.Amount} [UserId: {request.UserId}]");

        var result = new PaymentResult 
        { 
            TransactionId = Guid.NewGuid(), 
            Success = true, 
            Message = "UPI transaction completed successfully" 
        };

        return Task.FromResult(result);
    }
}
```

**Interview Points:**
- Each strategy is independent and doesn't know about others
- They share the same interface but different implementations
- Easy to add new payment methods without modifying existing code (Open/Closed Principle)
- Each strategy encapsulates its own algorithm details (logging, validation, etc.)

---

### 4. **Strategy Resolver (Runtime Strategy Selection)**

```csharp
public class PaymentStrategyResolver : IPaymentStrategyResolver
{
    private readonly IReadOnlyDictionary<string, IPaymentStrategy> _strategies;

    public PaymentStrategyResolver(IEnumerable<IPaymentStrategy> strategies)
    {
        // Build a case-insensitive dictionary: "Credit card" → CreditCardPaymentStrategy
        _strategies = strategies.ToDictionary(
            x => x.PaymentMethod, 
            StringComparer.OrdinalIgnoreCase  // Key feature: case-insensitive
        );
    }

    public IPaymentStrategy ResolveStrategy(string PaymentMethod)
    {
        if (!_strategies.TryGetValue(PaymentMethod, out var strategy))
        {
            throw new NotSupportedException(
                "ERROR: Payment method not supported"
            );
        }

        return strategy;
    }
}
```

**How It Works:**
1. **Constructor:** All registered strategies are collected and stored in a dictionary
2. **Key:** Strategy name (e.g., "Credit card", "UPI")
3. **Value:** Strategy instance
4. **Resolution:** When asked for a payment method, it returns the corresponding strategy
5. **Error Handling:** Throws if strategy not found

**Interview Point:** This is the **Strategy Selector** or **Context Switcher**—it determines which algorithm to use based on input.

---

### 5. **Service Layer (Context)**

```csharp
public class PaymentService : IPaymentService
{
    private readonly IPaymentStrategyResolver _paymentStrategyResolver;

    public PaymentService(IPaymentStrategyResolver paymentStrategyResolver)
    {
        _paymentStrategyResolver = paymentStrategyResolver;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        string PaymentMethod, 
        PaymentRequest request, 
        CancellationToken cancellationToken)
    {
        // Step 1: Get the appropriate strategy
        var paymentStrategy = _paymentStrategyResolver.ResolveStrategy(PaymentMethod);

        // Step 2: Delegate to the strategy
        var result = await paymentStrategy.ProcessPayment(request, cancellationToken);

        // Step 3: Return the result
        return result;
    }
}
```

**Key Flow:**
1. Service receives a payment method name
2. Service doesn't care HOW to process (algorithm is abstracted)
3. Service asks resolver for the right strategy
4. Service delegates the actual work to that strategy
5. Service returns the result

**Interview Point:** The service is **context-agnostic**—it doesn't know about credit cards, UPI, etc. It just uses strategies interchangeably.

---

### 6. **Dependency Injection Setup (Composition Root)**

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Register all strategies
builder.Services.AddScoped<IPaymentStrategy, CreditCardPaymentStrategy>();
builder.Services.AddScoped<IPaymentStrategy, UpiPaymentStrategy>();

// Register the resolver (it will receive all strategies via constructor injection)
builder.Services.AddScoped<IPaymentStrategyResolver, PaymentStrategyResolver>();

// Register the service
builder.Services.AddSingleton<IPaymentService, PaymentService>();

// Register the application
builder.Services.AddTransient<Application>();

var host = builder.Build();
var app = host.Services.GetRequiredService<Application>();
await app.RunAsync();
```

**Magic Moment:** When `PaymentStrategyResolver` is instantiated, the DI container automatically injects all registered `IPaymentStrategy` implementations. This is called **collection injection** in .NET.

---

### 7. **Client Usage (Application)**

```csharp
public class Application
{
    private readonly IPaymentService _paymentService;

    public Application(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public Task RunAsync()
    {
        var creditCardPaymentRequest = new PaymentRequest 
        { 
            UserId = Guid.NewGuid(), 
            Amount = 49900 
        };
        
        var upiPaymentRequest = new PaymentRequest 
        { 
            UserId = Guid.NewGuid(), 
            Amount = 5000 
        };

        CancellationToken token = new CancellationToken();

        // Choose different strategies at runtime
        _paymentService.ProcessPaymentAsync("Credit card", creditCardPaymentRequest, token);
        _paymentService.ProcessPaymentAsync("Upi", upiPaymentRequest, token);

        return Task.CompletedTask;
    }
}
```

**Interview Point:** The client only knows about `IPaymentService`. It doesn't import or reference any concrete strategy classes. This is **complete decoupling**.

---

## Key Design Principles

### 1. **Separation of Concerns**
- **Each strategy handles one algorithm** (SRP - Single Responsibility Principle)
- Strategies don't depend on each other
- Service doesn't implement payment logic directly

### 2. **Open/Closed Principle**
```
✅ Open for extension: Add new payment methods without modifying existing code
   - Create PayPalPaymentStrategy : IPaymentStrategy
   - Register in DI
   - Done! No changes to PaymentService

❌ Closed for modification: Don't change PaymentService, PaymentStrategyResolver, etc.
```

### 3. **Liskov Substitution Principle**
Any `IPaymentStrategy` implementation can be used wherever `IPaymentStrategy` is expected. The service doesn't care which concrete strategy is provided.

### 4. **Dependency Inversion**
- Concrete strategies depend on `IPaymentStrategy` interface (abstraction)
- Service depends on `IPaymentStrategyResolver` interface (abstraction)
- High-level modules (Service) don't depend on low-level modules (Concrete strategies)

### 5. **Runtime Flexibility**
Strategies are selected **at runtime**, not compile-time. The system adapts to user choices dynamically.

---

## Interview Q&A

### Q1: "What is the Strategy Pattern and why use it?"

**Answer:**
The Strategy Pattern encapsulates different algorithms into separate classes and makes them interchangeable. Instead of using if/else statements throughout the code, you:

1. Define a common interface (IPaymentStrategy)
2. Create concrete implementations for each algorithm
3. Let the client choose which strategy to use at runtime
4. The selection logic is centralized in a resolver

**Benefits:**
- Eliminates if/else spaghetti code
- Easy to add new strategies without modifying existing code
- Each algorithm is isolated and testable
- Strategies can be swapped at runtime
- Improves code maintainability and readability

---

### Q2: "How does your implementation handle adding a new payment method?"

**Answer:**
Very simply! I would only need to:

1. Create a new class implementing `IPaymentStrategy`:
```csharp
public class PayPalPaymentStrategy : IPaymentStrategy
{
    public string PaymentMethod => "PayPal";
    public Task<PaymentResult> ProcessPayment(PaymentRequest request, CancellationToken token)
    {
        // PayPal-specific logic
    }
}
```

2. Register it in DI:
```csharp
builder.Services.AddScoped<IPaymentStrategy, PayPalPaymentStrategy>();
```

That's it! No changes to `PaymentService`, `PaymentStrategyResolver`, or any other existing code. This demonstrates the **Open/Closed Principle**.

---

### Q3: "What's the role of `PaymentStrategyResolver`?"

**Answer:**
The resolver is the **strategy selector**. It:

1. **Maintains a registry** of all available strategies (using a dictionary)
2. **Maps payment method names** to strategy instances
3. **Resolves the correct strategy** at runtime based on the payment method name
4. **Throws an exception** if an unsupported method is requested

Without the resolver, the service would need to hardcode all if/else logic:
```csharp
// BAD: Without resolver
if (method == "Credit card") strategy = new CreditCardPaymentStrategy();
else if (method == "UPI") strategy = new UpiPaymentStrategy();
else throw new Exception();
```

**With the resolver**, the mapping is automatic and extensible.

---

### Q4: "Why use interfaces for `IPaymentService` and `IPaymentStrategyResolver`?"

**Answer:**
Interfaces enable:

1. **Dependency Inversion:** High-level modules depend on abstractions, not concrete implementations
2. **Testability:** You can mock these interfaces for unit testing
3. **Flexibility:** You can swap implementations without changing calling code
4. **Loose Coupling:** The application class doesn't know or care about concrete implementations

Example test:
```csharp
[Test]
public async Task ProcessPayment_ValidMethod_ReturnsSuccess()
{
    var mockResolver = new Mock<IPaymentStrategyResolver>();
    var mockStrategy = new Mock<IPaymentStrategy>();
    mockStrategy.Setup(s => s.ProcessPayment(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PaymentResult { Success = true });
    
    mockResolver.Setup(r => r.ResolveStrategy("Credit card"))
        .Returns(mockStrategy.Object);
    
    var service = new PaymentService(mockResolver.Object);
    var result = await service.ProcessPaymentAsync("Credit card", request, token);
    
    Assert.IsTrue(result.Success);
}
```

---

### Q5: "How does DI collection injection work in your setup?"

**Answer:**
When you register multiple implementations of the same interface:
```csharp
builder.Services.AddScoped<IPaymentStrategy, CreditCardPaymentStrategy>();
builder.Services.AddScoped<IPaymentStrategy, UpiPaymentStrategy>();
```

And then inject `IEnumerable<IPaymentStrategy>`:
```csharp
public PaymentStrategyResolver(IEnumerable<IPaymentStrategy> strategies)
```

The DI container **automatically collects all registered implementations** and passes them as a collection. The resolver then builds a dictionary from this collection.

**This is elegant because:**
- No manual strategy registration in the resolver
- Adding new strategies only requires DI registration
- The resolver stays generic and reusable

---

### Q6: "What would you change to improve this implementation?"

**Answer:**
Good follow-up points:

1. **Error Handling:** Add more specific exceptions and logging:
```csharp
public IPaymentStrategy ResolveStrategy(string PaymentMethod)
{
    if (string.IsNullOrWhiteSpace(PaymentMethod))
        throw new ArgumentException("Payment method cannot be empty");
    
    if (!_strategies.TryGetValue(PaymentMethod, out var strategy))
        throw new NotSupportedException($"Payment method '{PaymentMethod}' is not supported");
    
    return strategy;
}
```

2. **Async Initialization:** Some strategies might need async setup (DB connections, etc.)

3. **Logging/Audit Trail:**
```csharp
public async Task<PaymentResult> ProcessPaymentAsync(...)
{
    _logger.LogInformation($"Processing payment via {PaymentMethod}");
    try
    {
        var result = await paymentStrategy.ProcessPayment(request, cancellationToken);
        _logger.LogInformation($"Payment processed: {result.TransactionId}");
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Payment processing failed: {ex.Message}");
        throw;
    }
}
```

4. **Retry Logic:** Wrap strategy calls with retry policies (Polly library)

5. **Metrics/Monitoring:** Track which strategies are used most

---

### Q7: "Compare Strategy vs. Factory Pattern"

**Answer:**

| Aspect | Strategy | Factory |
|--------|----------|---------|
| **Purpose** | Encapsulate changeable algorithms | Create objects without specifying exact classes |
| **When to Use** | Multiple ways to do the same thing | Multiple types to be created |
| **Focus** | **What algorithm to execute** | **What object to create** |
| **Example** | Different payment processing algorithms | Creating different types of loggers |
| **Client Awareness** | Chooses which strategy to use | Client doesn't know which concrete object created |
| **Changeability** | Algorithm can change at runtime | Type is chosen at creation time |

**In your code:**
- `PaymentStrategyResolver` is **Factory-like** (creates strategies)
- `IPaymentStrategy` implementations are **Strategy Pattern** (interchangeable algorithms)

---

### Q8: "Compare Strategy vs. Decorator vs. Template Method"

**Answer:**

| Pattern | Purpose | Structure | When to Use |
|---------|---------|-----------|------------|
| **Strategy** | Encapsulate alternate algorithms | Interface + concrete implementations | Different ways to do the same thing |
| **Decorator** | Add responsibilities dynamically | Wraps another object | Enhance object behavior without subclassing |
| **Template Method** | Define algorithm skeleton | Base class with abstract methods | When steps are common but some vary |

**Example:**
```csharp
// Strategy: Different payment methods (CHOOSE ONE)
public interface IPaymentStrategy { Task<PaymentResult> Process(...); }

// Decorator: Add security layer to ANY strategy
public class SecurePaymentDecorator : IPaymentStrategy
{
    private readonly IPaymentStrategy _innerStrategy;
    
    public async Task<PaymentResult> Process(...)
    {
        ValidateSecurity();  // Added responsibility
        return await _innerStrategy.Process(...);
    }
}

// Template Method: All payments follow same steps
public abstract class PaymentProcessor
{
    public final void Process()
    {
        ValidateRequest();      // Common
        SpecificPaymentLogic(); // Override this
        LogTransaction();       // Common
    }
    protected abstract void SpecificPaymentLogic();
}
```

---

## Common Pitfalls & Best Practices

### ❌ Pitfall 1: Over-Stratification
**Problem:** Creating a strategy for every tiny variation
```csharp
// BAD: Unnecessary strategies for minor differences
class CreditCardAmexStrategy : IPaymentStrategy { }
class CreditCardVisaStrategy : IPaymentStrategy { }
class CreditCardMastercardStrategy : IPaymentStrategy { }
```

**Solution:** Use configuration or parameters instead:
```csharp
// GOOD: One credit card strategy with card type parameter
class CreditCardPaymentStrategy : IPaymentStrategy
{
    public async Task<PaymentResult> ProcessPayment(PaymentRequest request, ...)
    {
        var cardType = DetermineCardType(request.CardNumber);
        // Process based on card type if needed
    }
}
```

---

### ❌ Pitfall 2: Missing Error Handling
**Problem:** Resolver throws generic exceptions
```csharp
// BAD: No context about what went wrong
throw new NotSupportedException("ERROR: Payment method not supported");
```

**Solution:** Provide detailed context:
```csharp
// GOOD: Clear error message
if (!_strategies.TryGetValue(PaymentMethod, out var strategy))
{
    var supported = string.Join(", ", _strategies.Keys);
    throw new NotSupportedException(
        $"Payment method '{PaymentMethod}' is not supported. " +
        $"Supported methods: {supported}"
    );
}
```

---

### ❌ Pitfall 3: Stateful Strategies
**Problem:** Strategies holding mutable state
```csharp
// BAD: Strategy has state
public class PaymentStrategy : IPaymentStrategy
{
    private int _retryCount;  // Mutable state!
    public Task<PaymentResult> ProcessPayment(...) { ... }
}
```

**Solution:** Keep strategies stateless (or use scoped lifetime):
```csharp
// GOOD: Stateless or inject state as needed
public class PaymentStrategy : IPaymentStrategy
{
    private readonly IPaymentRetryPolicy _retryPolicy;  // Injected
    public Task<PaymentResult> ProcessPayment(...) { ... }
}
```

---

### ✅ Best Practice 1: Clear Naming
- `IPaymentStrategy` clearly indicates it's a strategy
- `PaymentStrategyResolver` clearly indicates it resolves strategies
- Method names like `ProcessPayment`, `ResolveStrategy` are explicit

---

### ✅ Best Practice 2: Fail Fast
```csharp
public PaymentStrategyResolver(IEnumerable<IPaymentStrategy> strategies)
{
    if (strategies == null)
        throw new ArgumentNullException(nameof(strategies));
    
    var strategyList = strategies.ToList();
    if (!strategyList.Any())
        throw new InvalidOperationException("At least one payment strategy must be registered");
    
    _strategies = strategyList.ToDictionary(x => x.PaymentMethod, StringComparer.OrdinalIgnoreCase);
}
```

---

### ✅ Best Practice 3: Use Dependency Injection
- Never create strategy instances with `new`
- Inject dependencies through constructors
- Let DI container manage lifecycles
- Makes testing easier

---

### ✅ Best Practice 4: Strategy Validation
```csharp
public async Task<PaymentResult> ProcessPaymentAsync(
    string PaymentMethod, 
    PaymentRequest request, 
    CancellationToken cancellationToken)
{
    // Validate inputs
    if (string.IsNullOrWhiteSpace(PaymentMethod))
        throw new ArgumentException("Payment method is required");
    
    if (request == null)
        throw new ArgumentNullException(nameof(request));
    
    if (request.Amount <= 0)
        throw new ArgumentException("Amount must be positive");
    
    // Get and use strategy
    var strategy = _paymentStrategyResolver.ResolveStrategy(PaymentMethod);
    return await strategy.ProcessPayment(request, cancellationToken);
}
```

---

## Summary for Interview

### One-Liner
"The Strategy Pattern lets me encapsulate different payment algorithms (credit card, UPI, etc.) into separate classes implementing a common interface, so I can switch between them at runtime without changing the client code."

### Three Key Points
1. **Encapsulation:** Each payment method is isolated in its own class
2. **Interchangeability:** Strategies can be swapped via a resolver without client code changes
3. **Extensibility:** Adding new payment methods requires only creating a new strategy and registering it in DI

### Your Implementation's Strengths
1. ✅ Clean separation between abstraction (`IPaymentStrategy`) and implementations
2. ✅ Centralized strategy selection via `PaymentStrategyResolver`
3. ✅ Proper DI setup allowing easy testing and extensibility
4. ✅ Async/await support with `CancellationToken` for cancellation support
5. ✅ Error handling for unsupported payment methods
6. ✅ No hardcoded if/else logic in the service layer

### What to Emphasize
- "The resolver automatically discovers all registered strategies without hardcoding"
- "Adding PayPal would only require creating a new class and one line of DI registration"
- "This follows SOLID principles, especially Open/Closed and Dependency Inversion"
- "The service doesn't know about specific payment methods—it's completely decoupled"

---

This implementation is a **textbook example of the Strategy Pattern**—well-structured, maintainable, and interview-ready!
