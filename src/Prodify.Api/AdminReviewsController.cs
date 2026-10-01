using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Admin.Reviews.Commands.SetReviewHidden;
using Prodify.Application.Admin.Reviews.Queries.ListAdminReviews;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

// Product reviews, for admins: check them and hide abusive ones.
[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = Roles.Admin)]
public class AdminReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListAdminReviewsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    public class HideReviewRequest
    {
        public string Reason { get; set; } = null!;
    }

    [HttpPost("{id:guid}/hide")]
    public async Task<IActionResult> Hide(Guid id, HideReviewRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetReviewHiddenCommand { ReviewId = id, Hidden = true, Reason = request.Reason }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unhide")]
    public async Task<IActionResult> Unhide(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetReviewHiddenCommand { ReviewId = id, Hidden = false }, cancellationToken);
        return NoContent();
    }
}
