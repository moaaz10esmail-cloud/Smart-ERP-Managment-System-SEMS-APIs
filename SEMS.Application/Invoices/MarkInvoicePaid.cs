using MediatR;
using SEMS.Core.Common;
using SEMS.Core.Finance;
using SEMS.Core.DomainEvents;
using SEMS.Core.Enums;
using SEMS.Core.ValueObjects;

namespace SEMS.Application.Invoices;

public sealed record MarkInvoicePaid(Guid InvoiceId) : IRequest<bool>;

public sealed class MarkInvoicePaidHandler : IRequestHandler<MarkInvoicePaid, bool>
{
    private readonly IRepository<Invoice> _repo;
    private readonly IRepository<Payment> _payments;
    private readonly IRepository<BankAccount> _bankAccounts;
    private readonly IRepository<Transaction> _transactions;
    private readonly IUnitOfWork _uow;
    public MarkInvoicePaidHandler(IRepository<Invoice> repo, IRepository<Payment> payments, IRepository<BankAccount> bankAccounts, IRepository<Transaction> transactions, IUnitOfWork uow)
    {
        _repo = repo;
        _payments = payments;
        _bankAccounts = bankAccounts;
        _transactions = transactions;
        _uow = uow;
    }
    public async Task<bool> Handle(MarkInvoicePaid request, CancellationToken cancellationToken)
    {
        var inv = await _repo.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (inv is null) return false;
        var payment = new Payment
        {
            InvoiceId = inv.Id,
            Amount = inv.Total,
            Status = PaymentStatus.Completed,
            Direction = PaymentDirection.In,
            PaidOn = DateTime.UtcNow
        };
        await _payments.AddAsync(payment, cancellationToken);

        var bankAccount = await _bankAccounts.GetByIdAsync(inv.BankAccountId, cancellationToken);
        if (bankAccount is null) throw new InvalidOperationException("Bank account must exist before paying an invoice.");

        var transaction = new Transaction
        {
            BankAccountId = inv.BankAccountId,
            PaymentId = payment.Id,
            Amount = inv.Total,
            Direction = PaymentDirection.In,
            OccurredOn = payment.PaidOn,
            Description = $"Invoice payment {inv.Id}"
        };
        await _transactions.AddAsync(transaction, cancellationToken);

        if (bankAccount.Balance.Amount == 0)
        {
            bankAccount.Balance = new Money(inv.Total.Amount, inv.Total.Currency);
        }
        else if (bankAccount.Balance.Currency == inv.Total.Currency)
        {
            var newAmount = bankAccount.Balance.Amount + inv.Total.Amount;
            bankAccount.Balance = new Money(newAmount, bankAccount.Balance.Currency);
        }
        else
        {
            throw new InvalidOperationException("Currency mismatch between bank account balance and invoice total.");
        }

        inv.Status = InvoiceStatus.Paid;
        inv.AddDomainEvent(new InvoicePaid(inv.Id));
        await _repo.UpdateAsync(inv, cancellationToken);
        await _bankAccounts.UpdateAsync(bankAccount, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
