using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Inventory;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/inventory/stocks")]
public class StocksController : ControllerBase
{
    private readonly SemsDbContext _db;
    public StocksController(SemsDbContext db) => _db = db;

    [HttpGet]
    [Authorize]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Stocks.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Stock>(items, total, query.Page, query.PageSize));
    }
}
