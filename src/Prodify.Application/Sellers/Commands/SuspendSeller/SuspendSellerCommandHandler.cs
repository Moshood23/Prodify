using MediatR;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Commands.SuspendSeller;

public class SuspendSellerCommandHandler : IRequestHandler<SuspendSellerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public SuspendSellerCommandHandler(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(SuspendSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Suspend(request.Reason);

        _emails.Enqueue(seller.Email, seller.BusinessName, SellerEmails.Suspended(seller, request.Reason.Trim()));
    }
}
