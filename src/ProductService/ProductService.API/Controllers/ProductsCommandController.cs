using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands;


namespace ProductService.API.Controllers;
[ApiVersion("1.0")]
[Route("api/v1/products")]
[ApiController]
public class ProductsCommandController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsCommandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {

        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

}
