using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Notifications.Commands.MarkAsRead;
using Prodify.Application.Notifications.Queries.GetNotifications;
using Microsoft.AspNetCore.Authorization;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] GetNotificationsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAsReadCommand { NotificationId = id }, cancellationToken);
        return NoContent();
    }
}