using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.HR;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/hr/leaverequests")]
public class LeaveRequestsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public LeaveRequestsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] LeaveRequest req)
    {
        _db.LeaveRequests.Add(req);
        await _db.SaveChangesAsync();
        return Created($"/api/hr/leaverequests/{req.Id}", new { id = req.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.LeaveRequests.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<LeaveRequest>(items, total, query.Page, query.PageSize));
    }
}
