using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Prodify.Api.Security;

public static class RateLimiting
{
    // Sign-in, sign-up and password reset: slows down password guessing and email spam.
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddProdifyRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        // Per visitor IP, per minute. Many Nigerian mobile users share one IP, so these stay generous;
        // account lockout (5 wrong passwords) protects each account on top of this.
        var authPerMinute = configuration.GetValue("RateLimits:AuthPerMinute", 20);
        var generalPerMinute = configuration.GetValue("RateLimits:GeneralPerMinute", 600);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(ClientIp(context), _ => PerMinute(generalPerMinute)));

            options.AddPolicy(AuthPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(ClientIp(context), _ => PerMinute(authPerMinute)));

            options.OnRejected = async (rejected, cancellationToken) =>
            {
                var response = rejected.HttpContext.Response;
                response.Headers.RetryAfter = "60";
                await response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many attempts. Please wait a minute and try again."
                }, cancellationToken);
            };
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions PerMinute(int permits) => new()
    {
        PermitLimit = permits,
        Window = TimeSpan.FromMinutes(1),
        QueueLimit = 0
    };

    private static string ClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
