using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Finance;
using SEMS.Core.ValueObjects;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using SEMS.Core.Enums;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/finance/payments")]
public class PaymentsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public PaymentsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req)
    {
        if (!Enum.IsDefined(typeof(PaymentDirection), req.Direction))
            return BadRequest("Payment direction must be specified as IN or OUT.");

        var invoice = await _db.Invoices.FindAsync(req.InvoiceId);
        if (invoice is null)
            return BadRequest("Invoice not found for payment.");

        var bankAccount = await _db.BankAccounts.FindAsync(invoice.BankAccountId);
        if (bankAccount is null)
            return BadRequest("Bank account not found for invoice.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var p = new Payment
        {
            InvoiceId = req.InvoiceId,
            Amount = new Money(req.Amount, req.Currency),
            PaidOn = req.PaidOn,
            Direction = req.Direction,
            Status = PaymentStatus.Completed
        };
        _db.Payments.Add(p);

        var t = new Transaction
        {
            BankAccountId = invoice.BankAccountId,
            PaymentId = p.Id,
            Amount = new Money(req.Amount, req.Currency),
            Direction = req.Direction,
            OccurredOn = req.PaidOn,
            Description = $"Manual payment for invoice {invoice.Id}"
        };
        _db.Transactions.Add(t);

        if (bankAccount.Balance.Amount == 0)
        {
            if (req.Direction == PaymentDirection.In)
                bankAccount.Balance = new Money(req.Amount, req.Currency);
            else
                bankAccount.Balance = new Money(-req.Amount, req.Currency);
        }
        else if (bankAccount.Balance.Currency == req.Currency)
        {
            var delta = req.Direction == PaymentDirection.In ? req.Amount : -req.Amount;
            var newAmount = bankAccount.Balance.Amount + delta;
            bankAccount.Balance = new Money(newAmount, bankAccount.Balance.Currency);
        }
        else
        {
            return BadRequest("Currency mismatch between bank account balance and payment.");
        }

        _db.BankAccounts.Update(bankAccount);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Created($"/api/finance/payments/{p.Id}", new { id = p.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentRequest req)
    {
        var p = await _db.Payments.FindAsync(id);
        if (p is null) return NotFound();

        var invoice = await _db.Invoices.FindAsync(p.InvoiceId);
        if (invoice is null) return BadRequest("Invoice not found for payment.");

        var bankAccount = await _db.BankAccounts.FindAsync(invoice.BankAccountId);
        if (bankAccount is null) return BadRequest("Bank account not found for invoice.");

        var t = await _db.Transactions.SingleOrDefaultAsync(x => x.PaymentId == p.Id);
        if (t is null) return BadRequest("Transaction not found for payment.");

        if (!Enum.IsDefined(typeof(PaymentDirection), req.Direction))
            return BadRequest("Payment direction must be specified as IN or OUT.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var oldAmount = p.Amount.Amount;
        var oldCurrency = p.Amount.Currency;
        var oldDirection = p.Direction;

        if (bankAccount.Balance.Currency != oldCurrency)
            return BadRequest("Currency mismatch between bank account balance and existing payment.");

        var undoDelta = oldDirection == PaymentDirection.In ? -oldAmount : oldAmount;
        var undone = bankAccount.Balance.Amount + undoDelta;
        bankAccount.Balance = new Money(undone, bankAccount.Balance.Currency);

        p.Amount = new Money(req.Amount, req.Currency);
        p.Direction = req.Direction;
        p.PaidOn = req.PaidOn;

        t.Amount = new Money(req.Amount, req.Currency);
        t.Direction = req.Direction;
        t.OccurredOn = req.PaidOn;

        if (bankAccount.Balance.Currency != req.Currency)
            return BadRequest("Currency mismatch between bank account balance and payment.");

        var delta = req.Direction == PaymentDirection.In ? req.Amount : -req.Amount;
        var newBalance = bankAccount.Balance.Amount + delta;
        bankAccount.Balance = new Money(newBalance, bankAccount.Balance.Currency);

        _db.BankAccounts.Update(bankAccount);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(p);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _db.Payments.FindAsync(id);
        if (p is null) return NotFound();

        var invoice = await _db.Invoices.FindAsync(p.InvoiceId);
        if (invoice is null) return BadRequest("Invoice not found for payment.");

        var bankAccount = await _db.BankAccounts.FindAsync(invoice.BankAccountId);
        if (bankAccount is null) return BadRequest("Bank account not found for invoice.");

        var t = await _db.Transactions.SingleOrDefaultAsync(x => x.PaymentId == p.Id);
        if (t is null) return BadRequest("Transaction not found for payment.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        if (bankAccount.Balance.Currency != p.Amount.Currency)
            return BadRequest("Currency mismatch between bank account balance and payment.");

        var delta = p.Direction == PaymentDirection.In ? -p.Amount.Amount : p.Amount.Amount;
        var newBalance = bankAccount.Balance.Amount + delta;
        bankAccount.Balance = new Money(newBalance, bankAccount.Balance.Currency);

        _db.Transactions.Remove(t);
        _db.Payments.Remove(p);
        _db.BankAccounts.Update(bankAccount);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Payments.AsQueryable();
        var total = q.Count();
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Payment>(items, total, query.Page, query.PageSize));
    }
}

public sealed class CreatePaymentRequest
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaidOn { get; set; } = DateTime.UtcNow;
    public PaymentDirection Direction { get; set; }
}

public sealed class UpdatePaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PaidOn { get; set; } = DateTime.UtcNow;
    public PaymentDirection Direction { get; set; }
}
