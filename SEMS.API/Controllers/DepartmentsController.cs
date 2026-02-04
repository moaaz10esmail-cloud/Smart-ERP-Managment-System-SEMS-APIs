using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEMS.Core.Common;
using SEMS.Core.HR;
using SEMS.Infrastructure.Extensions;
using SEMS.Infrastructure.Persistence;

namespace SEMS.API.Controllers;

public sealed record DepartmentRequest(string Name, Guid? TenantId);
public sealed record DepartmentDto(Guid Id, Guid? TenantId, string Name);

[ApiController]
[Route("api/v1/hr/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public DepartmentsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] DepartmentRequest req)
    {
        var entity = new Department
        {
            Name = req.Name,
            TenantId = req.TenantId
        };
        _db.Departments.Add(entity);
        await _db.SaveChangesAsync();
        var dto = new DepartmentDto(entity.Id, entity.TenantId, entity.Name);
        return Created($"/api/v1/hr/departments/{entity.Id}", dto);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Departments.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking()
            .Select(d => new DepartmentDto(d.Id, d.TenantId, d.Name))
            .ToList();
        return Ok(new PagedResult<DepartmentDto>(items, total, query.Page, query.PageSize));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Get(Guid id)
    {
        var dept = await _db.Departments.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DepartmentDto(d.Id, d.TenantId, d.Name))
            .FirstOrDefaultAsync();
        if (dept is null) return NotFound();
        return Ok(dept);
    }
}
