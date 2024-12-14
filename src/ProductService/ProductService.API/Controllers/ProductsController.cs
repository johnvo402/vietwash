using MediatR;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ProductService.Application.Commands;
using ProductService.Application.Queries;
using ProductService.Domain.Entities;
using System.Text.Json;

namespace ProductService.API.Controllers;
[ApiVersion("1.0")]
public class ProductsController : ODataController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [EnableQuery]
    // [Authorize]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var result = await _mediator.Send(new GetProductsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {

            return BadRequest(ex.Message);
        }
    }
}
