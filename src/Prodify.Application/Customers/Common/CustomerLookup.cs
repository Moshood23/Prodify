using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Application.Customers.Common;

public static class CustomerLookup
{
    // The logged-in customer, with their addresses.
    public static async Task<Customer> GetCurrentCustomerAsync(
        this IApplicationDbContext context, ICurrentUserService currentUser, CancellationToken cancellationToken)
    {
        var customerId = currentUser.GetRequiredCustomerId();

        return await context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken)
            ?? throw new NotFoundException("Customer", customerId);
    }

    // Someone else's address (or one that doesn't exist) is "not found".
    public static void EnsureHasAddress(this Customer customer, Guid addressId)
    {
        if (customer.Addresses.All(a => a.Id != addressId))
            throw new NotFoundException("Address", addressId);
    }
}