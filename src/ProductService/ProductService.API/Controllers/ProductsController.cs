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
[Route("material/api/v1/product")]
public class ProductsController : ApiController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
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
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var param = new QueryParameters();
        param.Where = $"id = '{id}'";
        var query = new GetProductsQuery(param);
        var result = await _mediator.Send(query);
        return result.Match(
          product => Ok(product?.FirstOrDefault()),
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
