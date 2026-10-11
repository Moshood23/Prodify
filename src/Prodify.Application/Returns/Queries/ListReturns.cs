using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Returns.Common;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Queries;

public static class ReturnFilters
{
    private const int MaxResults = 100;

    // "open" = waiting on someone (Requested or Escalated); a ReturnStatus name; or empty for all.
    public static IQueryable<ReturnRequest> WithStatus(this IQueryable<ReturnRequest> returns, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return returns;

        if (string.Equals(status, "open", StringComparison.OrdinalIgnoreCase))
            return returns.Where(r => r.Status == ReturnStatus.Requested || r.Status == ReturnStatus.Escalated);

        return Enum.TryParse<ReturnStatus>(status, ignoreCase: true, out var parsed)
            ? returns.Where(r => r.Status == parsed)
            : returns.Where(_ => false);
    }

    public static int Limit => MaxResults;
}

// The signed-in seller's returns.
public class ListSellerReturnsQuery : IRequest<List<ReturnDto>>
{
    public string? Status { get; set; }
}

public class ListSellerReturnsQueryHandler : IRequestHandler<ListSellerReturnsQuery, List<ReturnDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListSellerReturnsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<List<ReturnDto>> Handle(ListSellerReturnsQuery request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();
        var returns = _context.ReturnRequests.Where(r => r.SellerId == sellerId).WithStatus(request.Status);
        return _context.ToReturnDtosAsync(returns, ReturnFilters.Limit, cancellationToken);
    }
}

// Every seller's returns, for Prodify's staff.
public class ListAdminReturnsQuery : IRequest<List<ReturnDto>>
{
    public string? Status { get; set; }
}

public class ListAdminReturnsQueryHandler : IRequestHandler<ListAdminReturnsQuery, List<ReturnDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminReturnsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<ReturnDto>> Handle(ListAdminReturnsQuery request, CancellationToken cancellationToken) =>
        _context.ToReturnDtosAsync(_context.ReturnRequests.WithStatus(request.Status), ReturnFilters.Limit, cancellationToken);
}
