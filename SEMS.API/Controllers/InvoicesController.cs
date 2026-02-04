using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Application.Invoices;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Finance;
using SEMS.Core.Common;
using SEMS.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using SEMS.Core.Enums;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/finance/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SemsDbContext _db;
    public InvoicesController(IMediator mediator, SemsDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] CreateInvoice command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(MarkPaid), new { id }, new { id });
    }

    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> MarkPaid(Guid id)
    {
        var ok = await _mediator.Send(new MarkInvoicePaid(id));
        if (!ok) return NotFound();
        return Ok();
    }
    
    [HttpGet("prerequisites")]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult Prerequisites()
    {
        var customers = _db.Customers
            .AsNoTracking()
            .Select(c => new { id = c.Id, name = c.Name })
            .ToList();
        
        var bankAccounts = _db.BankAccounts
            .AsNoTracking()
            .Select(b => new { id = b.Id, bankName = b.BankName, accountNumber = b.AccountNumber })
            .ToList();
        
        return Ok(new { customers, bankAccounts });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Get(Guid id)
    {
        var inv = await _db.Invoices.FindAsync(id);
        if (inv is null) return NotFound();
        return Ok(inv);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Invoice req)
    {
        var inv = await _db.Invoices.FindAsync(id);
        if (inv is null) return NotFound();
        if (inv.Status == InvoiceStatus.Paid) return BadRequest("Cannot modify a paid invoice.");

        inv.CustomerId = req.CustomerId;
        inv.BankAccountId = req.BankAccountId;
        inv.Total = req.Total;
        inv.DueDate = req.DueDate;

        await _db.SaveChangesAsync();
        return Ok(inv);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Finance")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var inv = await _db.Invoices.FindAsync(id);
        if (inv is null) return NotFound();
        if (inv.Status == InvoiceStatus.Paid) return BadRequest("Cannot delete a paid invoice.");

        var hasPayments = await _db.Payments.AnyAsync(p => p.InvoiceId == id);
        if (hasPayments) return BadRequest("Cannot delete invoice with payments.");

        _db.Invoices.Remove(inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Finance")]
    public IActionResult List([FromQuery] PagedQuery query)
    {
        var q = _db.Invoices.AsQueryable();
        var total = q.Count();
        q = q.ApplySorting(query.SortBy, query.SortDirection);
        q = q.ApplyPaging(query);
        var items = q.AsNoTracking().ToList();
        return Ok(new PagedResult<Invoice>(items, total, query.Page, query.PageSize));
    }
}
