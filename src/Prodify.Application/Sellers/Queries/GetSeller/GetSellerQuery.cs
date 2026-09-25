using MediatR;

namespace Prodify.Application.Sellers.Queries.GetSeller;

// Public store information. Contact details stay private.
public class GetSellerQuery : IRequest<PublicSellerDto>
{
    public Guid Id { get; set; }
}

public class PublicSellerDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime JoinedAt { get; set; }
    public int ProductCount { get; set; }
}