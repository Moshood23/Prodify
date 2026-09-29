using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Shipping.DeliveryFees.Commands.UpdateDeliveryFee;

public class UpdateDeliveryFeeCommandHandler : IRequestHandler<UpdateDeliveryFeeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeliveryFeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDeliveryFeeCommand request, CancellationToken cancellationToken)
    {
        var state = request.State.Trim();
        var fee = await _context.DeliveryFees.FirstOrDefaultAsync(f => f.State == state, cancellationToken)
            ?? throw new NotFoundException("DeliveryFee", state);

        fee.ChangeAmount(request.Fee);
    }
}
