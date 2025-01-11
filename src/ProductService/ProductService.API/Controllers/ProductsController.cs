using ErrorOr;
using MediatR;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands;
using ProductService.Application.Queries;
using ProductService.Domain.Entities;
using System.Text.Json;

namespace ProductService.API.Controllers;
[ApiVersion("1.0")]
[Route("api/v1/products")]
public class ProductsController : ApiController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    // [Authorize]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts([FromQuery] QueryParameters request)
    {
        var query = new GetProductsQuery(request);
        var result = await _mediator.Send(query);
        return result.Match(
            token => Ok(token),
            Problem);
    }
    [HttpPost]
    [Route("create")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateProduct([FromBody] ApiRequestPost<CreateUpdateProductCommandDto> request)
    {
        var result = await _mediator.Send(new CreateProductCommand(request));
        return result.Match(
            token => Ok(token),
            Problem);
    }
}
