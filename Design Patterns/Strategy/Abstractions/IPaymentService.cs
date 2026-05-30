using Models;

namespace Abstractions;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(string PaymentMethod, PaymentRequest request, CancellationToken cancellationToken);
}