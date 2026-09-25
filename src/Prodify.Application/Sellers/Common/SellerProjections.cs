using System.Linq.Expressions;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Common;

public static class SellerProjections
{
    public static readonly Expression<Func<Seller, SellerProfileDto>> ToProfile = s => new SellerProfileDto
    {
        Id = s.Id,
        BusinessName = s.BusinessName,
        Email = s.Email,
        PhoneNumber = s.PhoneNumber,
        Description = s.Description,
        Status = s.Status.ToString(),
        StatusReason = s.StatusReason,
        StatusChangedAt = s.StatusChangedAt,
        CreatedAt = s.CreatedAt,
        PickupAddress = s.Addresses
            .Select(a => new SellerAddressDto
            {
                Id = a.Id,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                PhoneNumber = a.PhoneNumber
            })
            .FirstOrDefault()
    };
}