using MediatR;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;
using Prodify.Domain.Notifications.Entities;

namespace Prodify.Application.Sellers.Commands.ReinstateSeller;

public class ReinstateSellerCommandHandler : IRequestHandler<ReinstateSellerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public ReinstateSellerCommandHandler(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(ReinstateSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Reinstate();

        _emails.Enqueue(seller.Email, seller.BusinessName, SellerEmails.Reinstated(seller, _urls));
        _context.Add(Notification.Create(seller.Id, NotificationType.AccountUpdate, "Your store is active again",
            "Your products are back in the shop.", "/seller"));
    }
}
