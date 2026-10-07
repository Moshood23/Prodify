using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Prodify.Api.Security;
using Prodify.Application.Identity.Commands.Login;
using Prodify.Application.Identity.Commands.Logout;
using Prodify.Application.Identity.Commands.RefreshToken;
using Prodify.Application.Identity.Commands.Register;
using Prodify.Application.Identity.Commands.ChangePassword;
using Prodify.Application.Identity.Commands.ForgotPassword;
using Prodify.Application.Identity.Commands.ResetPassword;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [EnableRateLimiting(RateLimiting.AuthPolicy)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [EnableRateLimiting(RateLimiting.AuthPolicy)]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // No [Authorize]: the access token has usually expired when the client calls this.
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result is null)
            return Unauthorized(new { title = "Invalid or expired refresh token.", status = 401 });

        return Ok(result);
    }

    // Returns new tokens; the user's other sessions are logged out.
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // Always 204, whether or not the email has an account (so it can't be used to look accounts up).
    [EnableRateLimiting(RateLimiting.AuthPolicy)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // With the token from the reset email. The user then logs in with the new password.
    [EnableRateLimiting(RateLimiting.AuthPolicy)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }


    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
