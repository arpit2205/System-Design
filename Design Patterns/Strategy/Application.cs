using Abstractions;
using Models;

public class Application
{
    private readonly IPaymentService _paymentService;
    public Application(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    public Task RunAsync()
    {
        var creditCardPaymentRequest = new PaymentRequest { UserId = Guid.NewGuid(), Amount = 49900 };
        var upiPaymentRequest = new PaymentRequest { UserId = Guid.NewGuid(), Amount = 5000 };
        CancellationToken token = new CancellationToken();

        _paymentService.ProcessPaymentAsync("Credit card", creditCardPaymentRequest, token);
        _paymentService.ProcessPaymentAsync("Upi", upiPaymentRequest, token);

        return Task.CompletedTask;
    }
}