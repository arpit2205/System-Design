namespace Abstractions;

public interface IPaymentStrategyResolver
{
    IPaymentStrategy ResolveStrategy(string PaymentMethod);
}