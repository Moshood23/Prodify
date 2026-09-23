using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Common.Security;

public static class CurrentUserServiceExtensions
{
    public static bool IsAdmin(this ICurrentUserService currentUser) =>
        currentUser.IsInRole(Roles.Admin);

    public static Guid GetRequiredCustomerId(this ICurrentUserService currentUser) =>
        currentUser.CustomerId
        ?? throw new ForbiddenAccessException("This action requires a customer account.");
}