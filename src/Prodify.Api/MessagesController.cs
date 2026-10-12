using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Messaging.Commands;
using Prodify.Application.Messaging.Queries;
using Prodify.Domain.Messaging.Entities;

namespace Prodify.Api.Controllers;

// Messages between customers and sellers. Prodify's staff can read them.

[ApiController]
[Route("api/conversations")]
[Authorize(Roles = Roles.Customer)]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListMyConversationsQuery { As = MessageSender.Customer }, cancellationToken));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetUnreadMessageCountQuery { As = MessageSender.Customer }, cancellationToken));

    // Ask a seller about a product or an order. Returns the conversation's id.
    [HttpPost]
    public async Task<IActionResult> Start(StartConversationCommand command, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetMyConversationQuery { ConversationId = id, As = MessageSender.Customer }, cancellationToken));

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> Reply(Guid id, [FromBody] MessageBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReplyToConversationCommand { ConversationId = id, As = MessageSender.Customer, Message = body.Message ?? "" }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkConversationReadCommand { ConversationId = id, As = MessageSender.Customer }, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/seller-conversations")]
[Authorize(Roles = Roles.Seller)]
public class SellerConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellerConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListMyConversationsQuery { As = MessageSender.Seller }, cancellationToken));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetUnreadMessageCountQuery { As = MessageSender.Seller }, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetMyConversationQuery { ConversationId = id, As = MessageSender.Seller }, cancellationToken));

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> Reply(Guid id, [FromBody] MessageBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReplyToConversationCommand { ConversationId = id, As = MessageSender.Seller, Message = body.Message ?? "" }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkConversationReadCommand { ConversationId = id, As = MessageSender.Seller }, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/admin/conversations")]
[Authorize(Roles = Roles.Admin)]
public class AdminConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // search: an order number, a customer's or store's name, or words from the subject.
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListAdminConversationsQuery { Search = search }, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetAdminConversationQuery { ConversationId = id }, cancellationToken));
}

public record MessageBody(string? Message);
