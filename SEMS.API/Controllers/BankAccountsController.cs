using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Finance;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/finance/bankaccounts")]
public class BankAccountsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public BankAccountsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] BankAccount req)
    {
        _db.BankAccounts.Add(req);
        await _db.SaveChangesAsync();
        return Created($"/api/finance/bankaccounts/{req.Id}", new { id = req.Id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BankAccount req)
    {
        var existing = await _db.BankAccounts.FindAsync(id);
        if (existing is null) return NotFound();
        existing.BankName = req.BankName;
        existing.AccountNumber = req.AccountNumber;
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _db.BankAccounts.FindAsync(id);
        if (existing is null) return NotFound();

        var hasTransactions = await _db.Transactions.AnyAsync(t => t.BankAccountId == id);
        if (hasTransactions) return BadRequest("Cannot delete bank account with financial transactions.");

        _db.BankAccounts.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.BankAccounts.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<BankAccount>(items, total, query.Page, query.PageSize));
    }
}
