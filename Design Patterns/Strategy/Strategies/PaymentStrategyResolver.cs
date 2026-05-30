using Abstractions;

namespace Strategies;

public class PaymentStrategyResolver : IPaymentStrategyResolver
{
    private readonly IReadOnlyDictionary<string, IPaymentStrategy> _strategies;

    public PaymentStrategyResolver(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.PaymentMethod, StringComparer.OrdinalIgnoreCase);
    }
    public IPaymentStrategy ResolveStrategy(string PaymentMethod)
    {
        if (!_strategies.TryGetValue(PaymentMethod, out var strategy))
        {
            throw new NotSupportedException("ERROR: Payment method not supported");
        }

        return strategy;
    }
}