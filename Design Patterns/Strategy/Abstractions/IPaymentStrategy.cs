using Models;

namespace Abstractions;

public interface IPaymentStrategy
{
    string PaymentMethod { get; }
    Task<PaymentResult> ProcessPayment(PaymentRequest request, CancellationToken cancellationToken);
}