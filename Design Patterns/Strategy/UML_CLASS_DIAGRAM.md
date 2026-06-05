# UML Class Diagram - Strategy Pattern Implementation

## Detailed UML Class Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         STRATEGY PATTERN STRUCTURE                           │
└─────────────────────────────────────────────────────────────────────────────┘


                            ┌──────────────────────┐
                            │   PaymentRequest     │
                            ├──────────────────────┤
                            │ - UserId: Guid       │
                            │ - Amount: decimal    │
                            └──────────────────────┘
                                      △
                                      │ uses
                                      │
                ┌─────────────────────┴────────────────────────┐
                │                                              │
                │                                              │
         ┌──────▼──────┐                            ┌──────────▼─────┐
         │PaymentResult│                            │ IPaymentService│
         ├─────────────┤                            ├────────────────┤
         │ - Success   │◄───────────────────────────┤ + Process      │
         │ - Transaction│        returns            │   PaymentAsync │
         │   Id: Guid   │                            └────────┬───────┘
         │ - Message    │                                     △
         └─────────────┘                                      │ implements
                △                                             │
                │ returns                                     │
                │                                    ┌────────┴────────┐
                │                                    │                 │
                │                            ┌───────▼──────────┐
                │                            │  PaymentService  │
                │                            ├──────────────────┤
                │                            │ - _paymentStrategy│
                │                            │   Resolver:      │
                │                            │   IPaymentStrategy│
                │                            │   Resolver       │
                │                            ├──────────────────┤
                │                            │ + ProcessPayment │
                │                            │   Async()        │
                │                            └────────┬─────────┘
                │                                     │
                │                                     │ uses (delegates)
                │                                     │
    ┌───────────┴─────────────┬─────────────────────▼──────────────────┐
    │                         │                                        │
    │                         │                   ┌──────────────────────────┐
    │         ┌───────────────▼─────────────┐     │IPaymentStrategyResolver  │
    │         │  IPaymentStrategy           │     ├──────────────────────────┤
    │         ├───────────────────────────┐ │     │ - _strategies:           │
    │         │ + PaymentMethod: string   │ │     │   IReadOnlyDictionary    │
    │         │ + ProcessPayment()        │ │     │   <string,              │
    │         │   Task<PaymentResult>     │ │     │   IPaymentStrategy>     │
    │         └─────────────────────────┬─┘ │     ├──────────────────────────┤
    │                                   △   │     │ + ResolveStrategy():     │
    │                                   │   │     │   IPaymentStrategy      │
    │                    ┌──────────────┴───┴─────▼─────────────────────────┐
    │                    │                                                  │
    │                    │                                                  │
    │     ┌──────────────┴──────────────┐                 ┌────────────────┴──┐
    │     │                             │                 │                   │
    │     │                      ┌──────▼─────────────────▼──────────────┐   │
    │     │                      │PaymentStrategyResolver               │   │
    │     │                      ├───────────────────────────────────────┤   │
    │     │                      │ - _strategies: IReadOnlyDictionary    │   │
    │     │                      │   <string, IPaymentStrategy>          │   │
    │     │                      ├───────────────────────────────────────┤   │
    │     │                      │ + PaymentStrategyResolver()           │   │
    │     │                      │   (IEnumerable<IPaymentStrategy>)     │   │
    │     │                      │ + ResolveStrategy()                   │   │
    │     │                      │   : IPaymentStrategy                  │   │
    │     │                      └───────────────────────────────────────┘   │
    │     │                                    △                             │
    │     │                                    │ implements                  │
    │     │                                    │                             │
    │     │                                    └─────────────────────────────┘
    │     │
    │     │ implements
    │     │
    │  ┌──▼──────────────────────────────┐    ┌─────────────────────────────────┐
    │  │CreditCardPaymentStrategy        │    │   UpiPaymentStrategy            │
    │  ├─────────────────────────────────┤    ├─────────────────────────────────┤
    │  │ + PaymentMethod: string         │    │ + PaymentMethod: string         │
    │  │   = "Credit card"               │    │   = "UPI"                       │
    │  ├─────────────────────────────────┤    ├─────────────────────────────────┤
    │  │ + ProcessPayment()              │    │ + ProcessPayment()              │
    │  │   : Task<PaymentResult>         │    │   : Task<PaymentResult>         │
    │  │   [Credit Card Logic]           │    │   [UPI Logic]                   │
    │  └─────────────────────────────────┘    └─────────────────────────────────┘
    │                                                         │
    │  [Future Strategies can be added here]                 │
    │  - PayPalPaymentStrategy                               │
    │  - WalletPaymentStrategy                               │
    │  - BankTransferPaymentStrategy                         │
    │                                                         │
    │  All implement IPaymentStrategy interface              │
    │
    └─────────────────────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────────┐
│                        DEPENDENCY RELATIONSHIPS                              │
└─────────────────────────────────────────────────────────────────────────────┘

                             ┌──────────────┐
                             │  Application │
                             ├──────────────┤
                             │ - _payment   │
                             │   Service    │
                             └────────┬─────┘
                                      │
                                      │ depends on (constructor injection)
                                      │
                                      ▼
                          ┌──────────────────────┐
                          │  IPaymentService     │
                          ├──────────────────────┤
                          │ + ProcessPaymentAsync│
                          └──────────┬───────────┘
                                     △
                                     │ implemented by
                                     │
                          ┌──────────▼───────────┐
                          │  PaymentService      │
                          ├──────────────────────┤
                          │ - _paymentStrategy   │
                          │   Resolver           │
                          └──────────┬───────────┘
                                     │
                                     │ depends on (constructor injection)
                                     │
                                     ▼
                          ┌──────────────────────────────┐
                          │IPaymentStrategyResolver      │
                          ├──────────────────────────────┤
                          │ + ResolveStrategy()          │
                          └──────────┬───────────────────┘
                                     △
                                     │ implemented by
                                     │
                          ┌──────────▼───────────────────┐
                          │PaymentStrategyResolver       │
                          ├──────────────────────────────┤
                          │ - _strategies (dictionary)   │
                          └──────────┬───────────────────┘
                                     │
                     ┌───────────────┼───────────────┐
                     │               │               │
                     ▼               ▼               ▼
           ┌──────────────────┐  ┌─────────────┐  ┌──────────────────┐
           │CreditCardPayment │  │  UPI        │  │[Future Strategies]
           │Strategy          │  │Strategy     │  │PayPal,Wallet,etc.│
           └──────────────────┘  └─────────────┘  └──────────────────┘


┌─────────────────────────────────────────────────────────────────────────────┐
│                      DESIGN PATTERN ELEMENTS                                 │
└─────────────────────────────────────────────────────────────────────────────┘

CONTEXT:
  • PaymentService - Contains the business logic that uses strategies
  • Delegates algorithm selection to IPaymentStrategyResolver
  • Delegates algorithm execution to IPaymentStrategy

STRATEGY INTERFACE:
  • IPaymentStrategy - Defines the contract for all payment strategies
  • Methods: ProcessPayment()
  • Properties: PaymentMethod (identifier for resolver)

CONCRETE STRATEGIES:
  • CreditCardPaymentStrategy - Implements credit card payment logic
  • UpiPaymentStrategy - Implements UPI payment logic
  • [Future] PayPalPaymentStrategy, WalletPaymentStrategy, etc.

STRATEGY RESOLVER:
  • PaymentStrategyResolver - Selects appropriate strategy at runtime
  • Uses dictionary lookup for O(1) strategy resolution
  • Case-insensitive matching via StringComparer.OrdinalIgnoreCase

DATA CONTRACTS:
  • PaymentRequest - Input data (UserId, Amount)
  • PaymentResult - Output data (Success, TransactionId, Message)


┌─────────────────────────────────────────────────────────────────────────────┐
│                      EXECUTION FLOW SEQUENCE                                 │
└─────────────────────────────────────────────────────────────────────────────┘

1. Application creates PaymentRequest
   Application
      │
      ├─ Create new PaymentRequest(UserId, Amount)
      │
      └─▶ _paymentService.ProcessPaymentAsync(paymentMethod, request, token)

2. PaymentService receives call
   PaymentService.ProcessPaymentAsync()
      │
      ├─ Validate inputs
      │
      └─▶ _paymentStrategyResolver.ResolveStrategy(paymentMethod)

3. PaymentStrategyResolver selects strategy
   PaymentStrategyResolver.ResolveStrategy()
      │
      ├─ Look up in _strategies dictionary
      │  using paymentMethod as key
      │
      ├─ If found: return strategy instance
      │
      └─ If not found: throw NotSupportedException

4. PaymentService delegates to strategy
   PaymentService.ProcessPaymentAsync()
      │
      ├─ Get resolved strategy
      │
      └─▶ paymentStrategy.ProcessPayment(request, token)

5. Concrete Strategy executes
   CreditCardPaymentStrategy/UpiPaymentStrategy.ProcessPayment()
      │
      ├─ Log payment details
      │
      ├─ Process payment (business logic)
      │
      ├─ Create PaymentResult(Success, TransactionId, Message)
      │
      └─ Return Task<PaymentResult>

6. PaymentService returns result
   PaymentService.ProcessPaymentAsync()
      │
      ├─ Receive PaymentResult from strategy
      │
      └─▶ Return result to Application

7. Application receives result
   Application.RunAsync()
      │
      └─ PaymentResult (Success/Failure status)
```

---

## Key UML Notations Explained

### Class Box Format
```
┌─────────────────────────┐
│     ClassName           │  ← Class name
├─────────────────────────┤
│ - field: Type           │  ← Private attributes
│ + property: Type        │  ← Public attributes/properties
├─────────────────────────┤
│ + Method()              │  ← Public methods
│ - PrivateMethod()       │  ← Private methods
└─────────────────────────┘
```

### Relationship Symbols
```
┌──────────┐
│ Class A  │
└─────┬────┘
      │
      │ ▼ (inheritance/implementation arrow)
      ▼ Filled triangle = implementation
      △ Empty triangle = inheritance
      
┌──────────┐
│ Class B  │
└──────────┘

Association (─): General relationship
Dependency (─┬─▶): One class depends on another
Uses (─▶): Class A uses Class B
Implements (┴─▲): Class implements interface
Composition (◆─): Strong ownership
Aggregation (◇─): Weak ownership
Cardinality (*,1,0..1): How many objects relate
```

---

## Implementation to UML Mapping

| C# Code | UML Representation |
|---------|-------------------|
| `public class CreditCardPaymentStrategy : IPaymentStrategy` | Class box with `: IPaymentStrategy` showing implementation |
| `public string PaymentMethod { get; }` | `+ PaymentMethod: string` property notation |
| `public Task<PaymentResult> ProcessPayment(...)` | `+ ProcessPayment(): Task<PaymentResult>` method notation |
| `private readonly IPaymentStrategyResolver _paymentStrategyResolver;` | Arrow showing dependency from PaymentService to IPaymentStrategyResolver |
| Constructor injection | Dependency arrow with label "depends on (constructor injection)" |
| `new CancellationToken()` | Parameter in method signature |

---

## SOLID Principles Demonstrated in UML

### 1. **Single Responsibility Principle (SRP)**
Each class has one reason to change:
- `CreditCardPaymentStrategy` - Only changes if credit card processing logic changes
- `UpiPaymentStrategy` - Only changes if UPI processing logic changes
- `PaymentService` - Only changes if service orchestration changes

### 2. **Open/Closed Principle (OCP)**
Open for extension, closed for modification:
```
To add PayPal:
  ✅ Create PayPalPaymentStrategy : IPaymentStrategy
  ✅ Register in DI
  ✅ NO changes to existing classes
```

### 3. **Liskov Substitution Principle (LSP)**
All strategies are interchangeable:
```
IPaymentStrategy strategy = new CreditCardPaymentStrategy();
// Can be substituted with:
IPaymentStrategy strategy = new UpiPaymentStrategy();
// Without breaking any code
```

### 4. **Interface Segregation Principle (ISP)**
- `IPaymentStrategy` - Minimal, focused interface
- `IPaymentService` - Clear, single-purpose interface
- Clients don't depend on unused methods

### 5. **Dependency Inversion Principle (DIP)**
High-level modules depend on abstractions:
```
PaymentService (high-level)
         │
         ▼ depends on
    IPaymentStrategyResolver (abstraction)
         △
         │ implemented by
PaymentStrategyResolver (low-level)

NOT: PaymentService → PaymentStrategyResolver (directly)
```

---

## Class Relationships at a Glance

```
┌─────────────────────────────────────────────────────┐
│            RELATIONSHIP SUMMARY                      │
├─────────────────────────────────────────────────────┤
│ PaymentService                                      │
│   ├─ IS-A (implements): IPaymentService             │
│   └─ HAS-A (depends on): IPaymentStrategyResolver   │
│                                                     │
│ PaymentStrategyResolver                             │
│   ├─ IS-A (implements): IPaymentStrategyResolver    │
│   └─ HAS-A (contains): Dictionary of               │
│                        IPaymentStrategy instances   │
│                                                     │
│ CreditCardPaymentStrategy                           │
│   └─ IS-A (implements): IPaymentStrategy            │
│                                                     │
│ UpiPaymentStrategy                                  │
│   └─ IS-A (implements): IPaymentStrategy            │
│                                                     │
│ Application                                         │
│   └─ HAS-A (depends on): IPaymentService            │
│                                                     │
│ PaymentRequest                                      │
│   └─ USED-BY: IPaymentStrategy.ProcessPayment()    │
│                                                     │
│ PaymentResult                                       │
│   └─ RETURNED-BY: IPaymentStrategy.ProcessPayment()│
└─────────────────────────────────────────────────────┘
```

---

## Extensibility Points

The UML diagram clearly shows where new components can be added:

```
IPaymentStrategy
├─ CreditCardPaymentStrategy    (existing)
├─ UpiPaymentStrategy           (existing)
├─ 🔷 PayPalPaymentStrategy     (NEW)
├─ 🔷 WalletPaymentStrategy     (NEW)
├─ 🔷 GooglePayStrategy         (NEW)
├─ 🔷 ApplePayStrategy          (NEW)
└─ 🔷 BankTransferStrategy      (NEW)

All inherit from IPaymentStrategy
All are resolved by PaymentStrategyResolver
All are used transparently by PaymentService
```

### Adding New Strategy (Example: PayPal)

**Only 2 files need to be created/modified:**

1. **Create new strategy class** (NEW FILE):
```csharp
public class PayPalPaymentStrategy : IPaymentStrategy
{
    public string PaymentMethod => "PayPal";
    public Task<PaymentResult> ProcessPayment(PaymentRequest request, CancellationToken cancellationToken)
    {
        // PayPal-specific logic
    }
}
```

2. **Register in DI** (MODIFY Program.cs):
```csharp
builder.Services.AddScoped<IPaymentStrategy, PayPalPaymentStrategy>();
```

**NO other classes need modification.** This demonstrates the **Open/Closed Principle** in action.

---

## Comparison: Without Strategy Pattern

If this was implemented with if/else:

```
┌──────────────────────────────────────────┐
│         PaymentService                   │
├──────────────────────────────────────────┤
│ public async Task<PaymentResult>         │
│ ProcessPaymentAsync(string method, ...)  │
│ {                                        │
│   if (method == "Credit card")           │
│     // 50 lines of credit card logic     │
│   else if (method == "UPI")              │
│     // 50 lines of UPI logic             │
│   else if (method == "PayPal")           │
│     // 50 lines of PayPal logic          │
│   ...                                    │
│ }                                        │
└──────────────────────────────────────────┘

❌ Problem: 500+ lines in one method
❌ Hard to test each payment type independently
❌ Adding new payment method = Modify PaymentService
❌ Violates Open/Closed Principle
```

**With Strategy Pattern:**
```
✅ Each strategy in separate file (SRP)
✅ Easy to test each strategy independently
✅ Add new strategy = New file + 1 DI line
✅ Follows Open/Closed Principle
✅ Clear, readable code structure
```

---

## Summary

This UML diagram represents a **textbook implementation** of the Strategy Pattern with:

- ✅ Clear separation of concerns
- ✅ Proper abstraction using interfaces
- ✅ Centralized strategy resolution
- ✅ Dependency injection for testability
- ✅ SOLID principles compliance
- ✅ Easy extensibility for new payment methods
- ✅ No hardcoded logic in the service layer

Perfect for interviews and real-world applications!
