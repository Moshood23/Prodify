using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Commands.RegisterSeller;

public class RegisterSellerCommandHandler : IRequestHandler<RegisterSellerCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterSellerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterSellerCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Sellers
            .AnyAsync(s => s.Email == request.Email.Trim().ToLower(), cancellationToken);

        if (emailExists)
            throw new ConflictException($"A seller with email '{request.Email}' already exists.");

        var seller = Seller.Create(request.BusinessName, request.Email, request.PhoneNumber);

        _context.Add(seller);


        return seller.Id;
    }
}