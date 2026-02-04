using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Application.Customers;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.CRM;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/crm/customers")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SemsDbContext _db;
    public CustomersController(IMediator mediator, SemsDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Sales")]
    public async Task<IActionResult> Create([FromBody] CreateCustomer command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Get(Guid id)
    {
        var customer = await _mediator.Send(new GetCustomerById(id));
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Sales")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Customers.AsQueryable();
        var total = q.Count();
        q = q.ApplyFiltering(query.SearchTerm);
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Customer>(items, total, query.Page, query.PageSize));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Sales")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Customer update)
    {
        var existing = await _db.Customers.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Name = update.Name;
        existing.Address = update.Address;
        existing.UpdatedAt = DateTime.UtcNow;
        _db.Customers.Update(existing);
        await _db.SaveChangesAsync();
        return Ok(new { id });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Sales")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _db.Customers.FindAsync(id);
        if (existing is null) return NotFound();
        _db.Customers.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
