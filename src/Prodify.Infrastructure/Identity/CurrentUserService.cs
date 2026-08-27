using Microsoft.AspNetCore.Http;
using Prodify.Application.Common.Interfaces;
using System.Security.Claims;

namespace Prodify.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? CustomerId
    {
        get
        {
            var value = User?.FindFirstValue("customerId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? SellerId
    {
        get
        {
            var value = User?.FindFirstValue("sellerId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}