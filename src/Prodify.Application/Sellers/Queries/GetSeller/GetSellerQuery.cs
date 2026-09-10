using MediatR;

namespace Prodify.Application.Sellers.Queries.GetSeller;

public class GetSellerQuery : IRequest<SellerDto>
{
    public Guid Id { get; set; }
}

public class SellerDto
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = null!;
    public List<SellerAddressDto> Addresses { get; set; } = new();
}

public class SellerAddressDto
{
    public Guid Id { get; set; }
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}