using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Catalog.Products.Recommendations;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

// "You may also like" on product pages and the customer's "Recently viewed" row.
[ApiController]
[Route("api/products")]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecommendationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}/similar")]
    public async Task<IActionResult> Similar(Guid id, [FromQuery] int take = 8, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSimilarProductsQuery { ProductId = id, Take = take }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpGet("recently-viewed")]
    public async Task<IActionResult> RecentlyViewed([FromQuery] GetRecentlyViewedQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // One product (opened a product page), or the list kept in the browser before logging in.
    [Authorize(Roles = Roles.Customer)]
    [HttpPost("recently-viewed")]
    public async Task<IActionResult> RecordViews(RecordProductViewsCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
