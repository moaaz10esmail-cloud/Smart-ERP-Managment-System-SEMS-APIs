using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Finance;
using SEMS.Core.ValueObjects;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/finance/budgets")]
public class BudgetsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public BudgetsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequest req)
    {
        var b = new Budget { Name = req.Name, Amount = new Money(req.Amount, req.Currency), PeriodStart = req.PeriodStart, PeriodEnd = req.PeriodEnd };
        _db.Budgets.Add(b);
        await _db.SaveChangesAsync();
        return Created($"/api/finance/budgets/{b.Id}", new { id = b.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateBudgetRequest req)
    {
        var b = await _db.Budgets.FindAsync(id);
        if (b is null) return NotFound();
        b.Name = req.Name;
        b.Amount = new Money(req.Amount, req.Currency);
        b.PeriodStart = req.PeriodStart;
        b.PeriodEnd = req.PeriodEnd;
        await _db.SaveChangesAsync();
        return Ok(b);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var b = await _db.Budgets.FindAsync(id);
        if (b is null) return NotFound();
        var hasTransactions = await _db.Transactions.AnyAsync(t => t.BudgetId == id);
        if (hasTransactions) return BadRequest("Cannot delete budget with financial transactions.");
        _db.Budgets.Remove(b);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Budgets.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Budget>(items, total, query.Page, query.PageSize));
    }

    [HttpGet("analysis")]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult Analysis()
    {
        var budgets = _db.Budgets.AsNoTracking().ToList();
        var results = new List<object>(budgets.Count);

        foreach (var b in budgets)
        {
            var expensesAmount = _db.Expenses
                .Where(e => e.Category == b.Name && e.IncurredOn >= b.PeriodStart && e.IncurredOn <= b.PeriodEnd)
                .Select(e => e.Amount.Amount)
                .Sum();

            var budgetAmount = b.Amount.Amount;
            var remaining = budgetAmount - expensesAmount;
            var status = remaining >= 0 ? "Under Budget" : "Over Budget";

            results.Add(new
            {
                department = b.Name,
                budgetAmount,
                expensesAmount,
                remaining,
                status
            });
        }

        return Ok(results);
    }
}

public sealed class CreateBudgetRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PeriodStart { get; set; } = DateTime.UtcNow.Date;
    public DateTime PeriodEnd { get; set; } = DateTime.UtcNow.Date.AddMonths(1);
}
