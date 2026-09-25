using MediatR;

namespace Prodify.Application.Sellers.Commands.RegisterSeller;

public class RegisterSellerCommand : IRequest<RegisterSellerResult>
{
    public string BusinessName { get; set; } = null!;

    // Business contact email; the application form pre-fills it with the account's email.
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Description { get; set; }

    // Where orders are picked up from.
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
}

public class RegisterSellerResult
{
    public Guid SellerId { get; set; }
    public string Status { get; set; } = null!;

    // A fresh token that includes the new "sellerId" and the Seller role.
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}