using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMS.Application.Inventory;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/inventory/products")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    public InventoryController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProduct command)
    {
        var id = await _mediator.Send(command);
        return Created($"/api/inventory/products/{id}", new { id });
    }
}
