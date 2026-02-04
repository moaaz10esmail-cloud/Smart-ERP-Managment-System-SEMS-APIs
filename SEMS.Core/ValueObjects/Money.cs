namespace SEMS.Core.ValueObjects;

public sealed class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    private Money() { }
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}
