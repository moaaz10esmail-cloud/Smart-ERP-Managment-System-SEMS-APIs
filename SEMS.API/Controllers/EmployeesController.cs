using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEMS.Application.Employees;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.HR;
using SEMS.Core.Common;
using SEMS.Core.Identity;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/hr/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SemsDbContext _db;
    public EmployeesController(IMediator mediator, SemsDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    public sealed class CreateEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string? RoleId { get; set; }
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Employees.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var deptExists = await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId);
        if (!deptExists)
        {
            return BadRequest(new { error = "Invalid DepartmentId" });
        }

        Guid roleIdToUse;
        var parsedRole = string.IsNullOrWhiteSpace(request.RoleId) ? (Guid?)null : (Guid.TryParse(request.RoleId, out var rid) ? rid : (Guid?)null);
        if (parsedRole is null)
        {
            var defaultRoleId = await _db.Roles
                .Where(r => r.Name == "Employee")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
            if (defaultRoleId == Guid.Empty)
            {
                var newRole = new Role { Name = "Employee" };
                _db.Roles.Add(newRole);
                await _db.SaveChangesAsync();
                defaultRoleId = newRole.Id;
            }
            roleIdToUse = defaultRoleId;
        }
        else
        {
            var roleExists = await _db.Roles.AnyAsync(r => r.Id == parsedRole.Value);
            if (!roleExists)
            {
                return BadRequest(new { error = "Invalid RoleId" });
            }
            roleIdToUse = parsedRole.Value;
        }

        var resolved = new CreateEmployee(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.DepartmentId,
            roleIdToUse
        );
        var id = await _mediator.Send(resolved);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Employees.View)]
    public async Task<IActionResult> Get(Guid id)
    {
        var emp = await _mediator.Send(new GetEmployeeById(id));
        if (emp is null) return NotFound();
        return Ok(emp);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Employees.View)]
    public async Task<IActionResult> List([FromQuery] PagedQuery query)
    {
        var result = await _mediator.Send(new GetEmployees(query));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Employees.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Employee update)
    {
        var existing = await _db.Employees.FindAsync(id);
        if (existing is null) return NotFound();
        existing.FirstName = update.FirstName;
        existing.LastName = update.LastName;
        existing.DepartmentId = update.DepartmentId;
        existing.RoleId = update.RoleId;
        existing.UpdatedAt = DateTime.UtcNow;
        _db.Employees.Update(existing);
        await _db.SaveChangesAsync();
        return Ok(new { id });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Employees.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _db.Employees.FindAsync(id);
        if (existing is null) return NotFound();
        _db.Employees.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
