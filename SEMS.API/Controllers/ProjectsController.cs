using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Application.Projects;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SEMS.Infrastructure.Persistence.SemsDbContext _db;
    public ProjectsController(IMediator mediator, SemsDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Create([FromBody] CreateProject command)
    {
        var id = await _mediator.Send(command);
        return Created($"/api/projects/{id}", new { id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,ProjectManager")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Projects.AsQueryable();
        var total = q.Count();
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<SEMS.Core.Projects.Project>(items, total, query.Page, query.PageSize));
    }
}
