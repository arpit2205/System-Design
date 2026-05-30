using Abstractions;
using Models;

namespace Strategies;

public class UpiPaymentStrategy : IPaymentStrategy
{
    public string PaymentMethod => "UPI";

    public Task<PaymentResult> ProcessPayment(PaymentRequest request, CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"Processing {PaymentMethod} payment...");
        System.Console.WriteLine($"Amount: {request.Amount} [UserId: {request.UserId}]");

        var result = new PaymentResult { TransactionId = Guid.NewGuid(), Success = true, Message = "UPI transaction completed successfully" };

        return Task.FromResult(result);
    }
}