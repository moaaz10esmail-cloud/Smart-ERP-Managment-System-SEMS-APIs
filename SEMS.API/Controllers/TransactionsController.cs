using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Finance;
using SEMS.Core.ValueObjects;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Enums;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/finance/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public TransactionsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest req)
    {
        if (!Enum.IsDefined(typeof(PaymentDirection), req.Direction))
            return BadRequest("Transaction direction must be specified as IN or OUT.");

        var bankAccount = await _db.BankAccounts.FindAsync(req.BankAccountId);
        if (bankAccount is null)
            return BadRequest("Bank account not found for transaction.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var t = new Transaction
        {
            BankAccountId = req.BankAccountId,
            Amount = new Money(req.Amount, req.Currency),
            Direction = req.Direction,
            OccurredOn = req.OccurredOn,
            Description = req.Description,
            BudgetId = req.BudgetId
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
            return BadRequest("Currency mismatch between bank account balance and transaction.");
        }

        _db.BankAccounts.Update(bankAccount);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Created($"/api/finance/transactions/{t.Id}", new { id = t.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var items = _db.Transactions.OrderByDescending(a => a.OccurredOn)
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(items);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest req)
    {
        var t = await _db.Transactions.FindAsync(id);
        if (t is null) return NotFound();

        if (t.PaymentId.HasValue || t.ExpenseId.HasValue)
            return BadRequest("Cannot update transaction linked to a payment or expense.");

        if (!Enum.IsDefined(typeof(PaymentDirection), req.Direction))
            return BadRequest("Transaction direction must be specified as IN or OUT.");

        var oldBankAccount = await _db.BankAccounts.FindAsync(t.BankAccountId);
        if (oldBankAccount is null) return BadRequest("Bank account not found for transaction.");

        var newBankAccount = oldBankAccount;
        if (req.BankAccountId != t.BankAccountId)
        {
            newBankAccount = await _db.BankAccounts.FindAsync(req.BankAccountId);
            if (newBankAccount is null) return BadRequest("New bank account not found for transaction.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var oldAmount = t.Amount.Amount;
        var oldCurrency = t.Amount.Currency;
        var oldDirection = t.Direction;

        if (oldBankAccount.Balance.Currency != oldCurrency)
            return BadRequest("Currency mismatch between bank account balance and existing transaction.");

        var undoDelta = oldDirection == PaymentDirection.In ? -oldAmount : oldAmount;
        var oldNewBalance = oldBankAccount.Balance.Amount + undoDelta;
        oldBankAccount.Balance = new Money(oldNewBalance, oldBankAccount.Balance.Currency);

        t.BankAccountId = req.BankAccountId;
        t.Amount = new Money(req.Amount, req.Currency);
        t.Direction = req.Direction;
        t.OccurredOn = req.OccurredOn;
        t.Description = req.Description;
        t.BudgetId = req.BudgetId;

        if (newBankAccount.Balance.Currency != req.Currency)
            return BadRequest("Currency mismatch between bank account balance and transaction.");

        var delta = req.Direction == PaymentDirection.In ? req.Amount : -req.Amount;
        var newBalance = newBankAccount.Balance.Amount + delta;
        newBankAccount.Balance = new Money(newBalance, newBankAccount.Balance.Currency);

        _db.BankAccounts.Update(oldBankAccount);
        if (newBankAccount.Id != oldBankAccount.Id)
            _db.BankAccounts.Update(newBankAccount);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(t);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var t = await _db.Transactions.FindAsync(id);
        if (t is null) return NotFound();

        if (t.PaymentId.HasValue || t.ExpenseId.HasValue)
            return BadRequest("Cannot delete transaction linked to a payment or expense.");

        var bankAccount = await _db.BankAccounts.FindAsync(t.BankAccountId);
        if (bankAccount is null) return BadRequest("Bank account not found for transaction.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        if (bankAccount.Balance.Currency != t.Amount.Currency)
            return BadRequest("Currency mismatch between bank account balance and transaction.");

        var delta = t.Direction == PaymentDirection.In ? -t.Amount.Amount : t.Amount.Amount;
        var newBalance = bankAccount.Balance.Amount + delta;
        bankAccount.Balance = new Money(newBalance, bankAccount.Balance.Currency);

        _db.Transactions.Remove(t);
        _db.BankAccounts.Update(bankAccount);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }
}

public sealed class CreateTransactionRequest
{
    public Guid BankAccountId { get; set; }
    public Guid? BudgetId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public PaymentDirection Direction { get; set; }
}

public sealed class UpdateTransactionRequest
{
    public Guid BankAccountId { get; set; }
    public Guid? BudgetId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public PaymentDirection Direction { get; set; }
}
