namespace SEMS.Infrastructure.Services;

public interface IPaymentGateway
{
    Task<bool> ChargeAsync(string cardToken, decimal amount, string currency, CancellationToken cancellationToken = default);
}

public class PaymentGateway : IPaymentGateway
{
    public Task<bool> ChargeAsync(string cardToken, decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

