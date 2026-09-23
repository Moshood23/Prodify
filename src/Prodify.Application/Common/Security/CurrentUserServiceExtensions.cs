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

    public static Guid GetRequiredSellerId(this ICurrentUserService currentUser) =>
        currentUser.SellerId
        ?? throw new ForbiddenAccessException("This action requires a seller account.");

    // Admins manage everything; a seller manages only what belongs to their own seller account.
    public static bool CanManageSeller(this ICurrentUserService currentUser, Guid sellerId) =>
        currentUser.IsAdmin() || currentUser.SellerId == sellerId;
}