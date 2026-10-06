using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Admin.Queries.GetAdminDashboard;
using Prodify.Application.Common.Security;
using Prodify.Application.Reports.Queries.GetSalesChart;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = Roles.Admin)]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    // Whole-shop sales per day, for the dashboard chart (?days=7|30|90).
    [HttpGet("sales")]
    public async Task<IActionResult> Sales([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSalesChartQuery { Days = days }, cancellationToken);
        return Ok(result);
    }
}