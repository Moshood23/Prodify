using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Promotions.Vouchers.Commands.CreateVoucher;
using Prodify.Application.Promotions.Vouchers.Commands.UpdateVoucher;
using Prodify.Application.Promotions.Vouchers.Queries.ListVouchers;

namespace Prodify.Api.Controllers;

// Voucher codes customers use at checkout. Prodify pays the discount.
[ApiController]
[Route("api/admin/vouchers")]
[Authorize(Roles = Roles.Admin)]
public class AdminVouchersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminVouchersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListVouchersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVoucherCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateVoucherCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
