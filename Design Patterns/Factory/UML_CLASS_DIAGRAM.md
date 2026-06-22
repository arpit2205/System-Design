# Factory Pattern - UML Class Diagram

## Class Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         FACTORY PATTERN STRUCTURE                           │
└─────────────────────────────────────────────────────────────────────────────┘


                              ┌──────────────────────┐
                              │  INotificationFactory│
                              ├──────────────────────┤
                              │ <<interface>>        │
                              ├──────────────────────┤
                              │ + Create(string):    │
                              │   INotificationSender│
                              └──────────────────────┘
                                       ▲
                                       │
                                       │ implements
                                       │
                              ┌────────┴──────────────┐
                              │ NotificationFactory   │
                              ├──────────────────────┤
                              │ - serviceProvider:   │
                              │   IServiceProvider   │
                              ├──────────────────────┤
                              │ + NotificationFactory│
                              │   (IServiceProvider) │
                              │ + Create(string):    │
                              │   INotificationSender│
                              └──────────────────────┘
                                       │
                                       │ uses
                                       ▼
                         ┌─────────────────────────┐
                         │ INotificationSender     │
                         ├─────────────────────────┤
                         │ <<interface>>           │
                         ├─────────────────────────┤
                         │ + Notify(              │
                         │   NotificationRequest) │
                         └──────────┬──────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
                    │ implements    │ implements    │
                    │               │               │
        ┌───────────▼──────┐    ┌──▼─────────────┐
        │ EmailNotification │    │ SmsNotification │
        │      Sender       │    │      Sender     │
        ├───────────────────┤    ├─────────────────┤
        │                   │    │                 │
        │ + Notify(         │    │ + Notify(       │
        │   NotificationReq)│    │   NotificationReq)
        │ : Task            │    │ : Task           │
        └───────────────────┘    └─────────────────┘


┌──────────────────────────────────────────────────────────────────────────────┐
│                              SERVICE LAYER                                   │
└──────────────────────────────────────────────────────────────────────────────┘


                         ┌──────────────────────┐
                         │ INotificationService │
                         ├──────────────────────┤
                         │ <<interface>>        │
                         ├──────────────────────┤
                         │ + SendNotificationAsync│
                         │   (NotificationReq,  │
                         │    CancellationToken)│
                         │   : Task             │
                         └──────────────────────┘
                                  ▲
                                  │
                                  │ implements
                                  │
                         ┌────────┴──────────────┐
                         │ NotificationService   │
                         ├──────────────────────┤
                         │ - _notificationFactory│
                         │   : INotification    │
                         │   Factory            │
                         ├──────────────────────┤
                         │ + NotificationService│
                         │   (INotification     │
                         │   Factory)           │
                         │ + SendNotificationAsync
                         │   (NotificationReq,  │
                         │    CancellationToken)│
                         │   : Task             │
                         └──────────────────────┘
                                  │
                                  │ uses
                                  ▼
                         ┌──────────────────────┐
                         │ INotificationFactory │
                         └──────────────────────┘


┌──────────────────────────────────────────────────────────────────────────────┐
│                           APPLICATION LAYER                                  │
└──────────────────────────────────────────────────────────────────────────────┘


                              ┌────────────────┐
                              │  Application   │
                              ├────────────────┤
                              │ - _notification│
                              │   Service:     │
                              │   INotification│
                              │   Service      │
                              ├────────────────┤
                              │ + Application( │
                              │   INotification│
                              │   Service)     │
                              │ + RunAsync():  │
                              │   Task         │
                              └────────────────┘
                                     │
                                     │ uses
                                     ▼
                              ┌──────────────────────┐
                              │ INotificationService │
                              └──────────────────────┘


┌──────────────────────────────────────────────────────────────────────────────┐
│                            DATA MODELS                                       │
└──────────────────────────────────────────────────────────────────────────────┘


                          ┌─────────────────────┐
                          │ NotificationRequest │
                          ├─────────────────────┤
                          │ <<record>>          │
                          ├─────────────────────┤
                          │ + NotificationType: │
                          │   string            │
                          │ + Sender: string    │
                          │ + Receiver: string  │
                          │ + Message: string   │
                          └─────────────────────┘


┌──────────────────────────────────────────────────────────────────────────────┐
│                        DEPENDENCY INJECTION FLOW                             │
└──────────────────────────────────────────────────────────────────────────────┘


┌──────────────────────────────────────────────────────────────────────────────┐
│ Program.cs (Composition Root)                                                │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  services.AddTransient<Application>()                                       │
│  services.AddSingleton<INotificationFactory, NotificationFactory>()         │
│  services.AddTransient<INotificationService, NotificationService>()         │
│  services.AddScoped<EmailNotificationSender>()                              │
│  services.AddScoped<SmsNotificationSender>()                                │
│                                                                              │
│  Resolution Order:                                                           │
│  Application → INotificationService → INotificationFactory                  │
│               → NotificationFactory → IServiceProvider                       │
│                                    → EmailNotificationSender                │
│                                    → SmsNotificationSender                  │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Interaction Sequence Diagram

```
┌────────────────┐     ┌─────────────────────┐     ┌──────────────────┐
│  Application   │     │ NotificationService │     │ NotificationFactory
└────────┬────────┘     └─────────────┬───────┘     └────────┬──────────┘
         │                           │                      │
         │ SendNotificationAsync()   │                      │
         │──────────────────────────>│                      │
         │                           │ Create()             │
         │                           │─────────────────────>│
         │                           │                      │ GetRequiredService
         │                           │                      │─────────┐
         │                           │                      │<────────┘
         │                           │ return Sender        │
         │                           │<─────────────────────│
         │                           │ Notify(request)      │
         │                           │─────┐                │
         │                           │     │ (creates)      │
         │                           │     └────────────────┤
         │                           │ Task.CompletedTask   │
         │                           │<─────────────────────│
         │ Task                      │                      │
         │<──────────────────────────│                      │
         │                           │                      │
```

## Key Design Elements

### Patterns Applied

1. **Factory Pattern**: `NotificationFactory` creates different implementations based on type parameter
2. **Dependency Injection**: Services are registered and resolved through `IServiceProvider`
3. **Interface Segregation**: Clear separation of concerns with focused interfaces
4. **Open/Closed Principle**: Easy to add new notification types without modifying existing code

### Class Responsibilities

| Class | Responsibility |
|-------|-----------------|
| `INotificationFactory` | Define contract for creating notification senders |
| `NotificationFactory` | Create concrete implementations based on type parameter |
| `INotificationSender` | Define contract for sending notifications |
| `EmailNotificationSender` | Implement email notification sending |
| `SmsNotificationSender` | Implement SMS notification sending |
| `INotificationService` | Define contract for notification service operations |
| `NotificationService` | Use factory to create senders and delegate notification logic |
| `Application` | Orchestrate application flow using notification service |
| `NotificationRequest` | Data transfer object for notification details |

### Dependency Flow

```
Application
    ↓ depends on
INotificationService
    ↓ depends on
INotificationFactory
    ↓ uses
IServiceProvider
    ↓ resolves to
EmailNotificationSender | SmsNotificationSender
```

## Advantages of This Design

✅ **Loose Coupling**: Application depends on abstractions, not concrete classes  
✅ **Easy to Extend**: Add new notification types without modifying existing code  
✅ **Testability**: Interfaces make unit testing straightforward  
✅ **Centralized Creation**: All object creation logic in one factory class  
✅ **Dependency Injection**: Services managed by DI container  

## To Add a New Notification Type

Simply:
1. Create new class implementing `INotificationSender` (e.g., `PushNotificationSender`)
2. Register in DI container: `services.AddScoped<PushNotificationSender>()`
3. Add case in `NotificationFactory.Create()`: `"Push" => serviceProvider.GetRequiredService<PushNotificationSender>()`
