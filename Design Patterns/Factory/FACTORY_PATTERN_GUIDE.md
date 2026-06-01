# Factory Pattern Implementation Guide

## Table of Contents
1. [Overview](#overview)
2. [What is the Factory Pattern?](#what-is-the-factory-pattern)
3. [Implementation in This Repository](#implementation-in-this-repository)
4. [Design Structure](#design-structure)
5. [Key Components Explained](#key-components-explained)
6. [How It Works - Step by Step](#how-it-works---step-by-step)
7. [Advantages and Disadvantages](#advantages-and-disadvantages)
8. [Comparison: Factory vs Strategy vs Resolver](#comparison-factory-vs-strategy-vs-resolver)
9. [Interview Q&A](#interview-qa)
10. [Best Practices](#best-practices)

---

## Overview

The Factory Pattern is a **Creational Design Pattern** that provides an interface for creating objects without specifying their exact classes. It encapsulates object creation logic, making your code more flexible, maintainable, and testable.

**Key Principle**: "Encapsulate what varies" → In this case, object creation varies based on the notification type.

---

## What is the Factory Pattern?

### Definition
The Factory Pattern defines a method or class that creates objects of different types based on some input parameter, without exposing the concrete classes to the client.

### Problem It Solves
```csharp
// ❌ Without Factory Pattern - Tight Coupling
if (notificationType == "Email")
{
    sender = new EmailNotificationSender();
}
else if (notificationType == "Sms")
{
    sender = new SmsNotificationSender();
}
else if (notificationType == "Push")
{
    sender = new PushNotificationSender();
}

// Issues:
// 1. Client code has to know about all concrete classes
// 2. Adding new notification type requires modifying client code (OCP violation)
// 3. Hard to test
// 4. Dependencies are tightly coupled
```

### Solution with Factory Pattern
```csharp
// ✅ With Factory Pattern - Loose Coupling
INotificationSender sender = _factory.Create(notificationType);

// Benefits:
// 1. Client doesn't know about concrete classes
// 2. Easy to add new types without changing client code
// 3. Easy to mock for testing
// 4. Centralized creation logic
```

---

## Implementation in This Repository

### Folder Structure
```
Design Patterns/
└── Factory/
    ├── Abstractions/
    │   ├── INotificationFactory.cs      (Factory interface)
    │   ├── INotificationSender.cs       (Product interface)
    │   └── INotificationService.cs      (Service interface)
    ├── Concrete/
    │   ├── EmailNotificationSender.cs   (Concrete product 1)
    │   └── SmsNotificationSender.cs     (Concrete product 2)
    ├── Factories/
    │   └── NotificationFactory.cs       (Concrete factory)
    ├── Models/
    │   └── NotificationRequest.cs       (Data model)
    ├── Services/
    │   └── NotificationService.cs       (Consumer of factory)
    ├── Application.cs                    (Entry point)
    ├── Program.cs                        (DI configuration)
    └── Factory.csproj                    (Project file)
```

### Class Hierarchy

```
┌─────────────────────────────────────┐
│     INotificationFactory            │
│  + Create(string): INotificationSender
└──────────────┬──────────────────────┘
               │ implements
               │
┌──────────────▼──────────────────────┐
│   NotificationFactory               │
│  - serviceProvider: IServiceProvider │
│  + Create(string): INotificationSender
└──────────────────────────────────────┘

┌─────────────────────────────────────┐
│    INotificationSender              │
│  + Notify(request): Task            │
└──────────────┬──────────────────────┘
               │ implements
       ┌───────┴────────┐
       │                │
┌──────▼──────┐   ┌─────▼──────────┐
│ EmailNotif. │   │ SmsNotif.      │
│ Sender      │   │ Sender         │
└─────────────┘   └────────────────┘
```

---

## Design Structure

### 1. **INotificationFactory.cs** - Factory Interface
```csharp
public interface INotificationFactory
{
    INotificationSender Create(string notificationType);
}
```

**Purpose**: Defines the contract for creating notification senders.

**Why interface?**
- Allows dependency injection
- Makes it easy to create mock factories for testing
- Defines a contract that any factory implementation must follow

---

### 2. **INotificationSender.cs** - Product Interface
```csharp
public interface INotificationSender
{
    Task Notify(NotificationRequest request);
}
```

**Purpose**: Defines the contract for all notification senders.

**Why interface?**
- The factory returns `INotificationSender`, not concrete types
- Client code depends on abstraction, not implementation
- Easy to add new notification types

---

### 3. **Concrete Implementations** - EmailNotificationSender & SmsNotificationSender
```csharp
public class EmailNotificationSender : INotificationSender
{
    public Task Notify(NotificationRequest request)
    {
        // Email-specific logic
        Console.WriteLine($"[EMAIL] {request.Message}");
        return Task.CompletedTask;
    }
}

public class SmsNotificationSender : INotificationSender
{
    public Task Notify(NotificationRequest request)
    {
        // SMS-specific logic
        Console.WriteLine($"[SMS] {request.Message}");
        return Task.CompletedTask;
    }
}
```

**Purpose**: Concrete implementations of the `INotificationSender` interface.

**Each class contains:**
- Its own specific logic for sending notifications
- Implementation of the `Notify` method
- Can have its own dependencies

---

### 4. **NotificationFactory.cs** - The Factory
```csharp
public class NotificationFactory : INotificationFactory
{
    private readonly IServiceProvider serviceProvider;

    public NotificationFactory(IServiceProvider provider)
    {
        serviceProvider = provider;
    }

    public INotificationSender Create(string notificationType)
    {
        return notificationType switch
        {
            "Email" => serviceProvider.GetRequiredService<EmailNotificationSender>(),
            "Sms" => serviceProvider.GetRequiredService<SmsNotificationSender>(),
            _ => throw new NotSupportedException($"Unknown notification type: {notificationType}")
        };
    }
}
```

**Key Implementation Details:**

| Aspect | Details |
|--------|---------|
| **Pattern Used** | Switch expression (C# 8.0+) |
| **Service Resolution** | Uses `IServiceProvider` from .NET DI container |
| **Advantage** | Leverages DI to get services, ensures proper object lifecycle management |
| **Extensibility** | Adding new type requires only adding a new case in switch |
| **Error Handling** | Throws `NotSupportedException` for unknown types |

**Why use IServiceProvider?**
- Objects are created by the DI container, not manually instantiated
- Ensures proper lifecycle management (Scoped, Transient, Singleton)
- Dependencies of concrete senders are automatically resolved
- Easy to test by mocking the service provider

---

### 5. **NotificationService.cs** - Consumer of Factory
```csharp
public class NotificationService : INotificationService
{
    private readonly INotificationFactory _notificationFactory;

    public NotificationService(INotificationFactory notificationFactory)
    {
        _notificationFactory = notificationFactory;
    }

    public async Task SendNotificationAsync(NotificationRequest request, CancellationToken token)
    {
        INotificationSender sender = _notificationFactory.Create(request.NotificationType);
        await sender.Notify(request);
    }
}
```

**Responsibilities:**
- Uses the factory to create appropriate sender
- Doesn't know or care about concrete implementations
- Focuses on orchestration logic

**Inversion of Control (IoC):**
- Depends on `INotificationFactory` (abstraction), not concrete factory
- Factory is injected, not created
- Perfect for testing and flexible composition

---

### 6. **Program.cs** - Dependency Injection Configuration
```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions();

// Register services to DI container
builder.Services.AddTransient<Application>();
builder.Services.AddSingleton<INotificationFactory, NotificationFactory>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddScoped<EmailNotificationSender>();
builder.Services.AddScoped<SmsNotificationSender>();
```

**Lifecycle Management:**
- `AddSingleton`: One instance for entire application lifetime (Factory)
- `AddTransient`: New instance each time (Application)
- `AddScoped`: One instance per request/scope (Senders)

---

## How It Works - Step by Step

### Execution Flow

```
┌────────────────────────────────────────────────────────────┐
│ 1. Application starts (Program.cs)                         │
│    - DI Container registers all services                   │
└─────────────────────┬──────────────────────────────────────┘
                      │
┌─────────────────────▼──────────────────────────────────────┐
│ 2. Application.RunAsync() is called                        │
│    - Creates NotificationRequest("Email", ...)             │
│    - Calls _notificationService.SendNotificationAsync()   │
└─────────────────────┬──────────────────────────────────────┘
                      │
┌─────────────────────▼──────────────────────────────────────┐
│ 3. NotificationService.SendNotificationAsync()            │
│    - Calls _notificationFactory.Create("Email")          │
└─────────────────────┬──────────────────────────────────────┘
                      │
┌─────────────────────▼──────────────────────────────────────┐
│ 4. NotificationFactory.Create("Email")                    │
│    - Matches "Email" case in switch expression            │
│    - Calls serviceProvider.GetRequiredService<...>()     │
│    - Returns EmailNotificationSender instance            │
└─────────────────────┬──────────────────────────────────────┘
                      │
┌─────────────────────▼──────────────────────────────────────┐
│ 5. sender.Notify(request) is called                        │
│    - EmailNotificationSender.Notify() executes           │
│    - Prints: "[NOTIFICATION] Type=Email | From=... | To=..."
│    - Returns completed task                               │
└────────────────────────────────────────────────────────────┘
```

### Code Flow Example
```csharp
// Step 1: Create request
var request = new NotificationRequest("Email", "Arpit", "Bot", "Hello");

// Step 2: Service receives request
await _notificationService.SendNotificationAsync(request, token);

// Step 3: Service uses factory
INotificationSender sender = _notificationFactory.Create("Email");
// ↓ Factory returns EmailNotificationSender

// Step 4: Use the created sender
await sender.Notify(request);
// ↓ EmailNotificationSender prints the message
```

---

## Advantages and Disadvantages

### ✅ Advantages

| Advantage | Explanation |
|-----------|-------------|
| **Loose Coupling** | Client code doesn't depend on concrete classes |
| **Open/Closed Principle** | Open for extension (add new senders), closed for modification (factory logic stable) |
| **Centralized Creation** | All object creation logic in one place (factory) |
| **Easy Testing** | Mock the factory interface in unit tests |
| **Scalability** | Adding 10 new notification types requires minimal changes |
| **Flexibility** | Creation logic can be changed without affecting clients |
| **DI Integration** | Works seamlessly with dependency injection containers |
| **Single Responsibility** | Factory handles creation, Service handles orchestration |

### ❌ Disadvantages

| Disadvantage | Explanation | Solution |
|-------------|-------------|----------|
| **Extra Layer** | Adds another layer of abstraction | Justified in complex systems |
| **Complexity** | Might be overkill for simple scenarios | Use only when needed |
| **Single Switch/If** | All creation logic in one method | Can become a "God Method" |
| **Rigid String Matching** | Type is determined by string, not type-safe | Use enums or strong typing |
| **Maintenance** | As new types grow, switch becomes larger | Consider Abstract Factory or Registry Pattern |

### Potential Improvements

**1. Use Enum for Type Safety**
```csharp
public enum NotificationType
{
    Email,
    Sms,
    Push
}

public INotificationSender Create(NotificationType type)
{
    return type switch
    {
        NotificationType.Email => serviceProvider.GetRequiredService<EmailNotificationSender>(),
        NotificationType.Sms => serviceProvider.GetRequiredService<SmsNotificationSender>(),
        _ => throw new NotSupportedException()
    };
}
```

**2. Use Registry Pattern for Scalability**
```csharp
private readonly Dictionary<string, Type> _registry = new()
{
    { "Email", typeof(EmailNotificationSender) },
    { "Sms", typeof(SmsNotificationSender) }
};

public INotificationSender Create(string type)
{
    if (_registry.TryGetValue(type, out var senderType))
    {
        return (INotificationSender)serviceProvider.GetRequiredService(senderType);
    }
    throw new NotSupportedException($"Unknown type: {type}");
}
```

**3. Use Attributes for Auto-Registration**
```csharp
[NotificationSenderAttribute("Email")]
public class EmailNotificationSender : INotificationSender { }

[NotificationSenderAttribute("Sms")]
public class SmsNotificationSender : INotificationSender { }

// Auto-discover and register in DI container
```

---

## Comparison: Factory vs Strategy vs Resolver

### Quick Comparison Table

| Aspect | Factory | Strategy | Resolver |
|--------|---------|----------|----------|
| **Purpose** | Creates different objects | Switches between algorithms | Discovers and creates objects |
| **When to use** | Object creation varies | Algorithm/behavior varies | Runtime type resolution needed |
| **Returns** | Different concrete types | Same interface, different logic | Object instance based on key |
| **Coupling** | Decouples creation | Decouples algorithm | Decouples discovery |
| **Complexity** | Low to Medium | Low | Medium to High |
| **OCP Violation** | Yes, in some cases | No, fully compliant | No, fully compliant |
| **Single Responsibility** | Creates objects | Encapsulates algorithms | Discovers objects |

### Detailed Comparison

---

## **1. FACTORY PATTERN** *(Current Implementation)*

### Definition
Creates different types of objects based on input parameters.

### Your Implementation
```csharp
// Factory
public INotificationSender Create(string notificationType)
{
    return notificationType switch
    {
        "Email" => serviceProvider.GetRequiredService<EmailNotificationSender>(),
        "Sms" => serviceProvider.GetRequiredService<SmsNotificationSender>(),
        _ => throw new NotSupportedException()
    };
}

// Usage
INotificationSender sender = _factory.Create("Email");
await sender.Notify(request);
```

### Key Characteristics
- **Focus**: Object creation
- **Responsibility**: Deciding which concrete class to instantiate
- **Returns**: Different types (all implementing same interface)
- **Decision Factor**: Type string

### Pros
✅ Simple and straightforward  
✅ Centralizes object creation  
✅ Easy to understand  
✅ Good for simple-to-medium scenarios  

### Cons
❌ Switch/if statements can grow  
❌ String-based type checking (not type-safe)  
❌ Violates OCP if not using advanced patterns  
❌ Tight coupling to concrete types in factory  

### When to Use
- Creating objects of different types
- Object type varies based on configuration
- Want to decouple client from concrete classes
- Simple to medium number of types

### Real-World Analogy
Pizza Restaurant:
- Customer orders "Margherita" or "Pepperoni"
- Chef (factory) creates the right type of pizza
- Customer receives a Pizza (interface), doesn't care how it was made

---

## **2. STRATEGY PATTERN**

### Definition
Encapsulates different algorithms/behaviors and makes them interchangeable.

### Implementation Example
```csharp
// Strategy Interface
public interface INotificationStrategy
{
    Task SendAsync(NotificationRequest request);
}

// Concrete Strategies
public class EmailStrategy : INotificationStrategy
{
    public Task SendAsync(NotificationRequest request)
    {
        Console.WriteLine($"[EMAIL] {request.Message}");
        return Task.CompletedTask;
    }
}

public class SmsStrategy : INotificationStrategy
{
    public Task SendAsync(NotificationRequest request)
    {
        Console.WriteLine($"[SMS] {request.Message}");
        return Task.CompletedTask;
    }
}

// Context (uses strategy)
public class NotificationContext
{
    private INotificationStrategy _strategy;

    public NotificationContext(INotificationStrategy strategy)
    {
        _strategy = strategy;
    }

    public void SetStrategy(INotificationStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task ExecuteAsync(NotificationRequest request)
    {
        await _strategy.SendAsync(request);
    }
}

// Usage
var strategy = new EmailStrategy(); // or get from factory
var context = new NotificationContext(strategy);
await context.ExecuteAsync(request);
```

### Key Characteristics
- **Focus**: Algorithm/behavior variation
- **Responsibility**: Encapsulating algorithms so they're interchangeable
- **Returns**: Same interface type
- **Decision Factor**: Runtime choice of algorithm
- **Flexibility**: Can switch strategies at runtime

### Pros
✅ Runtime algorithm switching  
✅ Easy to add new algorithms  
✅ Encapsulates algorithm logic  
✅ Fully compliant with OCP  
✅ Context doesn't know implementation details  

### Cons
❌ Adds extra layer of indirection  
❌ More classes needed  
❌ Might be overkill for simple behavior  

### When to Use
- Multiple algorithms for same problem
- Need to switch algorithms at runtime
- Algorithm selection logic is complex
- Want to isolate algorithm implementations

### Real-World Analogy
Payment Processing:
- You have different payment methods (Credit Card, PayPal, Apple Pay)
- Each implements "PaymentStrategy"
- At checkout, you choose which strategy to use
- System uses selected strategy to process payment

### Difference from Factory in Your Code
```csharp
// FACTORY: Creates different TYPES based on what we need to create
// "What type of sender do I need?" → Factory decides → Returns EmailSender or SmsSender
var sender = _factory.Create("Email");

// STRATEGY: Switches between different ALGORITHMS for same task
// "How should I send this notification?" → Different implementations → But same interface
var strategy = new EmailStrategy();
var context = new NotificationContext(strategy);
// Can later change: context.SetStrategy(new SmsStrategy());
```

---

## **3. RESOLVER PATTERN** (Advanced)

### Definition
Automatically discovers and resolves dependencies/implementations at runtime.

### Implementation Example
```csharp
// Marker interface or attribute
[AttributeUsage(AttributeTargets.Class)]
public class NotificationResolverAttribute : Attribute
{
    public string Type { get; set; }
    public NotificationResolverAttribute(string type) => Type = type;
}

// Concrete implementations with metadata
[NotificationResolver("Email")]
public class EmailNotificationSender : INotificationSender
{
    public Task Notify(NotificationRequest request) => Task.CompletedTask;
}

[NotificationResolver("Sms")]
public class SmsNotificationSender : INotificationSender
{
    public Task Notify(NotificationRequest request) => Task.CompletedTask;
}

// Resolver (discovers and resolves)
public class NotificationResolver : INotificationFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _registry = new();

    public NotificationResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        DiscoverImplementations();
    }

    private void DiscoverImplementations()
    {
        // Find all types implementing INotificationSender
        var senderType = typeof(INotificationSender);
        var implementingTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => senderType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        foreach (var type in implementingTypes)
        {
            var attribute = type.GetCustomAttribute<NotificationResolverAttribute>();
            if (attribute != null)
            {
                _registry[attribute.Type] = type;
            }
        }
    }

    public INotificationSender Create(string notificationType)
    {
        if (_registry.TryGetValue(notificationType, out var senderType))
        {
            return (INotificationSender)_serviceProvider.GetRequiredService(senderType);
        }
        throw new NotSupportedException($"Unknown notification type: {notificationType}");
    }
}

// Usage - Same as factory, but discovers types automatically!
var resolver = new NotificationResolver(serviceProvider);
var sender = resolver.Create("Email");
```

### Key Characteristics
- **Focus**: Auto-discovery and resolution
- **Responsibility**: Finding and instantiating implementations
- **Returns**: Discovered types
- **Decision Factor**: Attributes, conventions, or reflection
- **Flexibility**: Automatic type discovery

### Pros
✅ No manual registration needed  
✅ Highly scalable  
✅ Automatically discovers new implementations  
✅ Metadata-driven (using attributes)  
✅ Reflection-based discovery  
✅ Great for plugin architectures  

### Cons
❌ Reflection performance overhead  
❌ More complex to implement  
❌ Harder to debug  
❌ Requires careful design  
❌ Can be "magic" (implicit behavior)  

### When to Use
- Plugin-based architectures
- Need auto-discovery of implementations
- Large number of implementations
- Want zero manual registration
- Building extensible frameworks

### Real-World Analogy
App Store:
- Developers publish apps (implementations)
- App Store automatically discovers all available apps
- When you search for a type of app, Store resolves and shows you all available implementations
- You choose which one to use

---

## **Side-by-Side Comparison in Code**

### Scenario: Adding new notification type "PushNotification"

#### Factory Approach (Your Code)
```csharp
// 1. Create concrete class
public class PushNotificationSender : INotificationSender
{
    public Task Notify(NotificationRequest request) { /* ... */ }
}

// 2. Register in DI
builder.Services.AddScoped<PushNotificationSender>();

// 3. Update factory
public INotificationSender Create(string notificationType)
{
    return notificationType switch
    {
        "Email" => serviceProvider.GetRequiredService<EmailNotificationSender>(),
        "Sms" => serviceProvider.GetRequiredService<SmsNotificationSender>(),
        "Push" => serviceProvider.GetRequiredService<PushNotificationSender>(), // NEW
        _ => throw new NotSupportedException()
    };
}

// ⚠️ Factory code MUST be modified (OCP violation)
```

#### Strategy Approach
```csharp
// 1. Create concrete class
[NotificationResolver("Push")]
public class PushStrategy : INotificationStrategy
{
    public Task SendAsync(NotificationRequest request) { /* ... */ }
}

// 2. Register in DI
builder.Services.AddScoped<PushStrategy>();

// 3. No changes to existing code! Context works with any strategy.
// Strategy selection happens externally (factory or resolver)

// ✅ Existing code unchanged (OCP compliant)
```

#### Resolver Approach
```csharp
// 1. Create concrete class with attribute
[NotificationResolver("Push")]
public class PushNotificationSender : INotificationSender
{
    public Task Notify(NotificationRequest request) { /* ... */ }
}

// 2. Register in DI
builder.Services.AddScoped<PushNotificationSender>();

// 3. No code changes needed! Resolver auto-discovers.
var sender = resolver.Create("Push"); // Works automatically!

// ✅ Fully compliant with OCP
// ✅ No factory modification
// ✅ Auto-discovered
```

---

## **Decision Tree: Which Pattern to Use?**

```
Is object creation varying?
│
├─ YES → Do you need object creation?
│         │
│         ├─ YES → FACTORY PATTERN
│         │        - Simple object creation
│         │        - Decouples client from concrete classes
│         │        - Good: Create EmailSender vs SmsSender
│         │
│         └─ NO → Continue...
│
└─ NO → Is behavior/algorithm varying?
         │
         ├─ YES → Do you need runtime switching?
         │         │
         │         ├─ YES → STRATEGY PATTERN
         │         │        - Switch between algorithms
         │         │        - Runtime flexibility
         │         │        - Good: Different email backends
         │         │
         │         └─ NO → Continue...
         │
         └─ NO → Do you need auto-discovery?
                  │
                  ├─ YES → RESOLVER PATTERN
                  │        - Auto-discover implementations
                  │        - Plugin architectures
                  │        - Highly scalable
                  │
                  └─ NO → Use simple direct instantiation
```

---

## **When Your Factory Could Be a Resolver**

Your current implementation:
```csharp
public INotificationSender Create(string notificationType)
{
    return notificationType switch
    {
        "Email" => serviceProvider.GetRequiredService<EmailNotificationSender>(),
        "Sms" => serviceProvider.GetRequiredService<SmsNotificationSender>(),
        _ => throw new NotSupportedException()
    };
}
```

**Could become a Resolver:**
- ✅ If you have 20+ notification types
- ✅ If notifications are added frequently
- ✅ If you want plugin support
- ✅ If types are loaded from configuration/database

**Stay with Factory if:**
- ✅ You have 2-5 notification types
- ✅ Types rarely change
- ✅ Performance is critical (no reflection)
- ✅ Simplicity is preferred

---

## Interview Q&A

### Q1: What is the Factory Pattern and why would you use it?

**Answer:**
The Factory Pattern is a creational design pattern that provides an interface for creating objects without exposing the concrete classes. You use it to:

1. **Decouple object creation from usage**: Client code doesn't need to know about `EmailNotificationSender` or `SmsNotificationSender`.

2. **Centralize creation logic**: All creation logic is in one place (the factory), making it easier to maintain and modify.

3. **Achieve the Open/Closed Principle**: You can add new notification types without modifying existing code.

4. **Facilitate testing**: You can mock the factory in unit tests.

In my implementation, the `NotificationFactory` decides which sender (`EmailNotificationSender` or `SmsNotificationSender`) to create based on the notification type, without the client knowing about these concrete classes.

---

### Q2: How is your implementation different from hard-coding the if/else statements in the client?

**Answer:**

**Hard-coded approach (❌ BAD):**
```csharp
public async Task SendNotificationAsync(NotificationRequest request, CancellationToken token)
{
    if (request.NotificationType == "Email")
    {
        var sender = new EmailNotificationSender();
        await sender.Notify(request);
    }
    else if (request.NotificationType == "Sms")
    {
        var sender = new SmsNotificationSender();
        await sender.Notify(request);
    }
}
```

**Issues:**
- Client knows about concrete classes
- Hard to test (can't mock concrete senders)
- Adding new type requires modifying NotificationService
- Violates Single Responsibility Principle

**Factory approach (✅ GOOD):**
```csharp
public async Task SendNotificationAsync(NotificationRequest request, CancellationToken token)
{
    INotificationSender sender = _notificationFactory.Create(request.NotificationType);
    await sender.Notify(request);
}
```

**Benefits:**
- Client only depends on `INotificationFactory` and `INotificationSender`
- Easy to test (mock the factory)
- Adding new type only requires changing the factory
- Single Responsibility: Service doesn't know creation logic

---

### Q3: Can you walk me through how an email notification gets sent in your implementation?

**Answer:**

1. **Application.cs** creates a `NotificationRequest` with type "Email"
2. **NotificationService** calls `_notificationFactory.Create("Email")`
3. **NotificationFactory** matches "Email" in the switch expression and returns `serviceProvider.GetRequiredService<EmailNotificationSender>()`
4. **EmailNotificationSender** instance is returned to NotificationService
5. NotificationService calls `sender.Notify(request)`
6. **EmailNotificationSender.Notify()** executes and prints the message

**Key point:** At no step does NotificationService directly reference `EmailNotificationSender`. It only knows about `INotificationSender` interface.

---

### Q4: How does dependency injection work in your implementation?

**Answer:**

In **Program.cs**, we register all services:
```csharp
builder.Services.AddSingleton<INotificationFactory, NotificationFactory>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddScoped<EmailNotificationSender>();
builder.Services.AddScoped<SmsNotificationSender>();
```

**What happens:**
1. When `Application` needs `INotificationService`, DI provides `NotificationService`
2. `NotificationService` constructor requires `INotificationFactory`, so DI provides `NotificationFactory`
3. `NotificationFactory` constructor requires `IServiceProvider`, so DI provides it
4. When `NotificationFactory.Create()` is called, it uses `serviceProvider.GetRequiredService<EmailNotificationSender>()` to get instances

**Benefits:**
- Objects are created by DI container, not manually
- Lifecycle is managed (Singleton, Scoped, Transient)
- Easy to test by providing mock implementations
- Dependencies are automatically resolved

---

### Q5: What would happen if we added a new notification type, like "Push"?

**Answer:**

**Steps:**
1. Create `PushNotificationSender : INotificationSender`
2. Register in DI: `builder.Services.AddScoped<PushNotificationSender>()`
3. Add case in factory: `"Push" => serviceProvider.GetRequiredService<PushNotificationSender>()`
4. Call with `notificationType: "Push"`

**What doesn't change:**
- `NotificationService` code - unchanged
- `Application` code - unchanged
- All existing notification types - unaffected
- Client code using the service - unchanged

**This demonstrates the Open/Closed Principle:** Code is open for extension (adding Push) but closed for modification (existing code doesn't need changes).

---

### Q6: How does your Factory Pattern differ from Strategy Pattern?

**Answer:**

| Aspect | Factory | Strategy |
|--------|---------|----------|
| **Purpose** | Creates different types | Encapsulates different algorithms |
| **Problem Solved** | "What to create?" | "Which algorithm to use?" |
| **In your code** | Decides: EmailSender vs SmsSender | Would be: Different email backends |
| **Coupling** | Decouples creation | Decouples algorithm logic |
| **Example** | Notification Type (Email/SMS) | Email backend (Gmail/SendGrid) |

**Factory Example (Your Code):**
```csharp
// Factory decides WHICH TYPE to create
var sender = _factory.Create("Email"); // EmailNotificationSender
// vs
var sender = _factory.Create("Sms");   // SmsNotificationSender
```

**Strategy Example:**
```csharp
// Strategy encapsulates different algorithms
var context = new NotificationContext(new GmailStrategy());
// vs
var context = new NotificationContext(new SendGridStrategy());
// Both send emails, but using different services
```

---

### Q7: Could this be implemented using Strategy Pattern instead? Should it?

**Answer:**

**Yes, it could be:**
```csharp
public interface INotificationStrategy
{
    Task SendAsync(NotificationRequest request);
}

public class EmailStrategy : INotificationStrategy { /* ... */ }
public class SmsStrategy : INotificationStrategy { /* ... */ }

public class NotificationContext
{
    private INotificationStrategy _strategy;
    public NotificationContext(INotificationStrategy strategy) => _strategy = strategy;
    public Task SendAsync(NotificationRequest request) => _strategy.SendAsync(request);
}
```

**Should we?** No, because:

- **Factory is more appropriate here** because:
  - We're deciding **which type to create**, not which algorithm to use
  - Different sender types have different characteristics
  - Each sender is fundamentally different (Email vs SMS)

- **Strategy would be better if:**
  - We had multiple backends for same type (Gmail, SendGrid, Mailgun for Email)
  - We needed runtime algorithm switching
  - The difference is in HOW we send, not WHAT we send

---

### Q8: What are potential issues with your current implementation?

**Answer:**

1. **String-based type checking**
   - Not type-safe
   - Typos cause runtime errors
   - Solution: Use enum
   ```csharp
   public enum NotificationType { Email, Sms }
   public INotificationSender Create(NotificationType type)
   ```

2. **Growing switch statement**
   - With 20+ types, becomes unwieldy
   - Solution: Use Registry or Resolver pattern
   ```csharp
   private Dictionary<string, Type> _registry = new();
   ```

3. **OCP Violation**
   - Adding new type requires modifying factory
   - Solution: Resolver pattern or attributes

4. **Tight coupling to concrete types**
   - Even though factory hides them, factory knows about them
   - Solution: Reflection-based discovery

---

### Q9: How would you improve this to support 50+ notification types?

**Answer:**

**Use Registry + Attributes approach:**

```csharp
// Mark implementations
[NotificationSender("Email")]
public class EmailNotificationSender : INotificationSender { }

[NotificationSender("Sms")]
public class SmsNotificationSender : INotificationSender { }

// Auto-discovering factory
public class DiscoveringNotificationFactory : INotificationFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _registry = new();

    public DiscoveringNotificationFactory(IServiceProvider provider)
    {
        _serviceProvider = provider;
        DiscoverImplementations();
    }

    private void DiscoverImplementations()
    {
        var senderType = typeof(INotificationSender);
        var implementations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => senderType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var impl in implementations)
        {
            var attr = impl.GetCustomAttribute<NotificationSenderAttribute>();
            if (attr != null)
                _registry[attr.Type] = impl;
        }
    }

    public INotificationSender Create(string type)
    {
        if (_registry.TryGetValue(type, out var implType))
            return (INotificationSender)_serviceProvider.GetRequiredService(implType);
        throw new NotSupportedException(type);
    }
}
```

**Advantages for 50+ types:**
- ✅ Automatic discovery - no manual registration
- ✅ Adding new type: Create class + add attribute
- ✅ Factory code never changes
- ✅ Fully OCP compliant

---

### Q10: How would you unit test the NotificationService?

**Answer:**

```csharp
[TestClass]
public class NotificationServiceTests
{
    [TestMethod]
    public async Task SendNotificationAsync_CallsFactoryAndSender()
    {
        // Arrange
        var mockSender = new Mock<INotificationSender>();
        var mockFactory = new Mock<INotificationFactory>();
        mockFactory
            .Setup(f => f.Create("Email"))
            .Returns(mockSender.Object);

        var service = new NotificationService(mockFactory.Object);
        var request = new NotificationRequest("Email", "From", "To", "Message");

        // Act
        await service.SendNotificationAsync(request, CancellationToken.None);

        // Assert
        mockFactory.Verify(f => f.Create("Email"), Times.Once);
        mockSender.Verify(s => s.Notify(request), Times.Once);
    }

    [TestMethod]
    public async Task SendNotificationAsync_UsesCorrectSender()
    {
        // Arrange
        var mockEmailSender = new Mock<INotificationSender>();
        var mockSmsSender = new Mock<INotificationSender>();
        var mockFactory = new Mock<INotificationFactory>();

        mockFactory.Setup(f => f.Create("Email")).Returns(mockEmailSender.Object);
        mockFactory.Setup(f => f.Create("Sms")).Returns(mockSmsSender.Object);

        var service = new NotificationService(mockFactory.Object);
        var emailRequest = new NotificationRequest("Email", "From", "To", "Message");

        // Act
        await service.SendNotificationAsync(emailRequest, CancellationToken.None);

        // Assert
        mockFactory.Verify(f => f.Create("Email"), Times.Once);
        mockEmailSender.Verify(s => s.Notify(It.IsAny<NotificationRequest>()), Times.Once);
        mockSmsSender.Verify(s => s.Notify(It.IsAny<NotificationRequest>()), Times.Never);
    }
}
```

**Key benefits:**
- Factory is mocked, so we control what sender is created
- We verify factory is called with correct type
- We verify correct sender's Notify method is called
- No real sending happens in tests

---

## Best Practices

### ✅ Do's

1. **Use abstractions (interfaces)**
   ```csharp
   ✅ GOOD: public interface INotificationFactory
   ❌ BAD:  public class NotificationFactory
   ```

2. **Combine with Dependency Injection**
   ```csharp
   ✅ GOOD: Register in DI container, inject dependencies
   ❌ BAD:  Create factories manually
   ```

3. **Use meaningful names**
   ```csharp
   ✅ GOOD: NotificationFactory, INotificationFactory
   ❌ BAD:  Factory, Sender
   ```

4. **Single Responsibility**
   ```csharp
   ✅ GOOD: Factory creates, Service orchestrates
   ❌ BAD:  Service both creates and orchestrates
   ```

5. **Handle errors gracefully**
   ```csharp
   ✅ GOOD: throw new NotSupportedException($"Unknown type: {type}")
   ❌ BAD:  return null; // or silently fail
   ```

6. **Make factories immutable**
   ```csharp
   ✅ GOOD: private readonly IServiceProvider serviceProvider;
   ❌ BAD:  private IServiceProvider serviceProvider; // mutable
   ```

### ❌ Don'ts

1. **Don't expose concrete classes**
   ```csharp
   ❌ BAD:  return new EmailNotificationSender();
   ✅ GOOD: serviceProvider.GetRequiredService<EmailNotificationSender>();
   ```

2. **Don't have factory logic in client code**
   ```csharp
   ❌ BAD:  if (type == "Email") sender = new EmailNotificationSender();
   ✅ GOOD: sender = factory.Create(type);
   ```

3. **Don't make factories static**
   ```csharp
   ❌ BAD:  public static INotificationSender Create()
   ✅ GOOD: public INotificationSender Create()
   ```

4. **Don't ignore errors**
   ```csharp
   ❌ BAD:  if (!registry.ContainsKey(type)) return null;
   ✅ GOOD: if (!registry.ContainsKey(type)) throw new NotSupportedException();
   ```

5. **Don't mix Factory with complex initialization**
   ```csharp
   ❌ BAD:  Factory does creation + configuration + validation
   ✅ GOOD: Factory creates, initialization happens in constructor/method
   ```

6. **Don't overuse Factory for simple creation**
   ```csharp
   ❌ BAD:  Use factory for single object with no variations
   ✅ GOOD: Use factory when type varies based on input
   ```

---

## Summary

### Key Takeaways

| Concept | Explanation |
|---------|-------------|
| **Factory Pattern** | Encapsulates object creation, decouples client from concrete classes |
| **Your Implementation** | Uses `NotificationFactory` to create `EmailNotificationSender` or `SmsNotificationSender` based on type |
| **Benefits** | Loose coupling, centralized logic, easy testing, scalability |
| **vs Strategy** | Factory creates different TYPES; Strategy encapsulates different ALGORITHMS |
| **vs Resolver** | Factory uses switch/if; Resolver auto-discovers using reflection |
| **For Production** | Add type safety (enum), consider Resolver for 50+ types, combine with DI |

### When to Use Factory Pattern
- ✅ Object creation logic varies by type
- ✅ Want to decouple creation from usage
- ✅ Plan to add new types in future
- ✅ Need centralized object creation
- ✅ Building extensible systems

### Your Implementation is Perfect For
- ✅ 5-20 notification types
- ✅ Types known at development time
- ✅ Clear type classification
- ✅ Team prefers simple, understandable code
- ✅ Performance critical (no reflection)

---

## Further Reading

### Related Patterns
- **Abstract Factory**: For creating families of related objects
- **Builder**: For complex object construction
- **Singleton**: For ensuring single instance
- **Prototype**: For object cloning
- **Strategy Pattern**: For algorithm encapsulation
- **Resolver/Service Locator**: For automatic discovery
- **Registry Pattern**: For managing implementations

### SOLID Principles Applied
- **S**ingle Responsibility: Factory creates, Service uses
- **O**pen/Closed: Open for extension (new types), closed for modification
- **L**iskov Substitution: All senders are substitutable `INotificationSender`
- **I**nterface Segregation: Focused interfaces (`INotificationFactory`, `INotificationSender`)
- **D**ependency Inversion: Depends on abstractions, not concrete types

---

**Document Last Updated:** June 2026  
**Repository:** arpit2205/System-Design  
**Pattern:** Factory Pattern  
**Language:** C# (.NET)
