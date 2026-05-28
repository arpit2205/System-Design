using MediatR;

namespace Application.Features.Products.Events;

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

public class ProductCreatedEmailHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEmailHandler> _logger;

    public ProductCreatedEmailHandler(ILogger<ProductCreatedEmailHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("EMAIL: Product created: {ProductName}", notification.Name);
        return Task.CompletedTask;
    }
}

public class ProductCreatedAuditHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedAuditHandler> _logger;
    public ProductCreatedAuditHandler(ILogger<ProductCreatedAuditHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AUDIT: Product created with ID: {ProductId}", notification.ProductId);
        return Task.CompletedTask;
    }
}