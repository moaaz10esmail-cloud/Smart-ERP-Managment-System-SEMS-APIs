using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Inventory;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/inventory/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly SemsDbContext _db;
    public SuppliersController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] Supplier req)
    {
        _db.Suppliers.Add(req);
        await _db.SaveChangesAsync();
        return Created($"/api/inventory/suppliers/{req.Id}", new { id = req.Id });
    }

    [HttpGet]
    [Authorize]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Suppliers.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Supplier>(items, total, query.Page, query.PageSize));
    }
}
