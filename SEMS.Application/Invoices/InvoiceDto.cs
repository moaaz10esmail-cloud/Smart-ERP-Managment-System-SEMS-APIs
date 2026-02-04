namespace SEMS.Application.Invoices;

public sealed class InvoiceDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
}

