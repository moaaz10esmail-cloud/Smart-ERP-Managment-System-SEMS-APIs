using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Core.Enums;
using SEMS.Core.HR;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/hr/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly SemsDbContext _db;
    public AttendanceController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceRequest req)
    {
        var entity = new Attendance
        {
            EmployeeId = req.EmployeeId,
            Date = req.Date,
            Status = Enum.TryParse<AttendanceStatus>(req.Status, true, out var st) ? st : AttendanceStatus.Present
        };
        _db.Attendances.Add(entity);
        await _db.SaveChangesAsync();
        return Created($"/api/hr/attendance/{entity.Id}", new { id = entity.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Attendances.AsQueryable();
        var total = q.Count();
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Attendance>(items, total, query.Page, query.PageSize));
    }
}

public sealed class CreateAttendanceRequest
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public string Status { get; set; } = "Present";
}
