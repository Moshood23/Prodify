using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Commands.RegisterSeller;

public class RegisterSellerCommandHandler : IRequestHandler<RegisterSellerCommand, RegisterSellerResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public RegisterSellerCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _context = context;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<RegisterSellerResult> Handle(RegisterSellerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("You must be logged in to register as a seller.");

        if (_currentUser.SellerId.HasValue)
            throw new ConflictException("This account is already registered as a seller.");

        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Sellers
            .AnyAsync(s => s.Email == email, cancellationToken);

        if (emailExists)
            throw new ConflictException($"A seller with email '{email}' already exists.");

        // New sellers wait for an admin to approve them before they can list products.
        var seller = Seller.Create(request.BusinessName, request.Email, request.PhoneNumber, request.Description);
        seller.AddAddress(request.AddressLine1, request.City, request.State, "Nigeria", request.PhoneNumber, request.AddressLine2);

        _context.Add(seller);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _identityService.AddSellerAccountAsync(userId, seller.Id, cancellationToken);

        if (!result.Succeeded)
            throw new ConflictException(string.Join(" ", result.Errors));

        return new RegisterSellerResult
        {
            SellerId = seller.Id,
            Status = seller.Status.ToString(),
            Token = result.Token!,
            RefreshToken = result.RefreshToken!
        };
    }
}