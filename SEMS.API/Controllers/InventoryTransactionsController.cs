using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Inventory;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/inventory/transactions")]
public class InventoryTransactionsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public InventoryTransactionsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] InventoryTransaction req)
    {
        _db.InventoryTransactions.Add(req);
        await _db.SaveChangesAsync();
        return Created($"/api/inventory/transactions/{req.Id}", new { id = req.Id });
    }

    [HttpGet]
    [Authorize]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.InventoryTransactions.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<InventoryTransaction>(items, total, query.Page, query.PageSize));
    }
}
