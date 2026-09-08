using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Application.Customers.Commands.RegisterCustomer;

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _context.Customers
            .AnyAsync(c => c.Email == request.Email.Trim().ToLower(), cancellationToken);

        if (emailExists)
            throw new ConflictException($"A customer with email '{request.Email}' already exists.");

        var customer = Customer.Create(request.FirstName, request.LastName, request.Email, request.PhoneNumber);

        _context.Add(customer);

        await _context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}