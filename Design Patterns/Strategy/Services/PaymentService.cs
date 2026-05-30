using Abstractions;
using Models;

namespace Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentStrategyResolver _paymentStrategyResolver;

    public PaymentService(IPaymentStrategyResolver paymentStrategyResolver)
    {
        _paymentStrategyResolver = paymentStrategyResolver;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(string PaymentMethod, PaymentRequest request, CancellationToken cancellationToken)
    {
        var paymentStrategy = _paymentStrategyResolver.ResolveStrategy(PaymentMethod);
        var result = await paymentStrategy.ProcessPayment(request, cancellationToken);
        return result;
    }
}