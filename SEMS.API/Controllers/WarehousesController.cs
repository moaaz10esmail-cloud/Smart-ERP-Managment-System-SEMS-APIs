using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Inventory;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/inventory/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly SemsDbContext _db;
    public WarehousesController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Warehouse req)
    {
        _db.Warehouses.Add(req);
        await _db.SaveChangesAsync();
        return Created($"/api/inventory/warehouses/{req.Id}", new { id = req.Id });
    }

    [HttpGet]
    [Authorize]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Warehouses.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Warehouse>(items, total, query.Page, query.PageSize));
    }
}
