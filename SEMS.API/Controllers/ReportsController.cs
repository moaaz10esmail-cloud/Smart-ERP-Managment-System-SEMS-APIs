using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Reports;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly SemsDbContext _db;
    public ReportsController(SemsDbContext db) => _db = db;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] Report model)
    {
        _db.Reports.Add(model);
        await _db.SaveChangesAsync();
        return Created($"/api/reports/{model.Id}", new { id = model.Id });
    }
}
