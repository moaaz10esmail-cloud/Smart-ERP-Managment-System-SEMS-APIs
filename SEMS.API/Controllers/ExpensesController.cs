using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Finance;
using SEMS.Core.ValueObjects;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/finance/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly SemsDbContext _db;
    public ExpensesController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] CreateExpenseRequest req)
    {
        var bankAccount = await _db.BankAccounts.FindAsync(req.BankAccountId);
        if (bankAccount is null)
            return BadRequest("Bank account not found for expense.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var e = new Expense
        {
            Category = req.Category,
            Amount = new Money(req.Amount, req.Currency),
            IncurredOn = req.IncurredOn
        };
        _db.Expenses.Add(e);

        var t = new Transaction
        {
            BankAccountId = req.BankAccountId,
            ExpenseId = e.Id,
            BudgetId = req.BudgetId,
            Amount = new Money(req.Amount, req.Currency),
            Direction = PaymentDirection.Out,
            OccurredOn = req.IncurredOn,
            Description = req.Category
        };
        _db.Transactions.Add(t);

        if (bankAccount.Balance.Amount == 0)
        {
            bankAccount.Balance = new Money(-req.Amount, req.Currency);
        }
        else if (bankAccount.Balance.Currency == req.Currency)
        {
            var newAmount = bankAccount.Balance.Amount - req.Amount;
            bankAccount.Balance = new Money(newAmount, bankAccount.Balance.Currency);
        }
        else
        {
            return BadRequest("Currency mismatch between bank account balance and expense.");
        }

        _db.BankAccounts.Update(bankAccount);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Created($"/api/finance/expenses/{e.Id}", new { id = e.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var items = _db.Expenses.OrderByDescending(a => a.IncurredOn)
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(items);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseRequest req)
    {
        var e = await _db.Expenses.FindAsync(id);
        if (e is null) return NotFound();

        var t = await _db.Transactions.SingleOrDefaultAsync(x => x.ExpenseId == e.Id);
        if (t is null) return BadRequest("Transaction not found for expense.");

        var bankAccount = await _db.BankAccounts.FindAsync(t.BankAccountId);
        if (bankAccount is null) return BadRequest("Bank account not found for expense.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var oldAmount = e.Amount.Amount;
        var oldCurrency = e.Amount.Currency;

        if (bankAccount.Balance.Currency != oldCurrency)
            return BadRequest("Currency mismatch between bank account balance and existing expense.");

        var undone = bankAccount.Balance.Amount + oldAmount;
        bankAccount.Balance = new Money(undone, bankAccount.Balance.Currency);

        e.Category = req.Category;
        e.Amount = new Money(req.Amount, req.Currency);
        e.IncurredOn = req.IncurredOn;

        t.Amount = new Money(req.Amount, req.Currency);
        t.BudgetId = req.BudgetId;
        t.OccurredOn = req.IncurredOn;

        if (bankAccount.Balance.Currency != req.Currency)
            return BadRequest("Currency mismatch between bank account balance and expense.");

        var newBalance = bankAccount.Balance.Amount - req.Amount;
        bankAccount.Balance = new Money(newBalance, bankAccount.Balance.Currency);

        _db.BankAccounts.Update(bankAccount);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(e);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var e = await _db.Expenses.FindAsync(id);
        if (e is null) return NotFound();

        var t = await _db.Transactions.SingleOrDefaultAsync(x => x.ExpenseId == e.Id);
        if (t is null) return BadRequest("Transaction not found for expense.");

        var bankAccount = await _db.BankAccounts.FindAsync(t.BankAccountId);
        if (bankAccount is null) return BadRequest("Bank account not found for expense.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        if (bankAccount.Balance.Currency != e.Amount.Currency)
            return BadRequest("Currency mismatch between bank account balance and expense.");

        var newBalance = bankAccount.Balance.Amount + e.Amount.Amount;
        bankAccount.Balance = new Money(newBalance, bankAccount.Balance.Currency);

        _db.Transactions.Remove(t);
        _db.Expenses.Remove(e);
        _db.BankAccounts.Update(bankAccount);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }
}

public sealed class CreateExpenseRequest
{
    public Guid BankAccountId { get; set; }
    public Guid? BudgetId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime IncurredOn { get; set; } = DateTime.UtcNow;
}

public sealed class UpdateExpenseRequest
{
    public Guid? BudgetId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime IncurredOn { get; set; } = DateTime.UtcNow;
}

