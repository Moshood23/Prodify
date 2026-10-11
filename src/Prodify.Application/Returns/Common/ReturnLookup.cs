using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Common;

public static class ReturnLookup
{
    // The return (with its items) and its order number. Anyone the filter doesn't allow gets "not found".
    public static async Task<(ReturnRequest Request, string OrderNumber)> GetReturnAsync(
        this IApplicationDbContext context, Guid id, Func<ReturnRequest, bool> allowed, CancellationToken cancellationToken)
    {
        var request = await context.ReturnRequests
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (request is null || !allowed(request))
            throw new NotFoundException("Return", id);

        var orderNumber = await context.Orders
            .Where(o => o.Id == request.OrderId)
            .Select(o => o.OrderNumber.Value)
            .FirstAsync(cancellationToken);

        return (request, orderNumber);
    }
}
