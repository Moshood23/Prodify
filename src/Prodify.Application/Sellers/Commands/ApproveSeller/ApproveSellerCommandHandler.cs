using MediatR;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;
using Prodify.Domain.Notifications.Entities;

namespace Prodify.Application.Sellers.Commands.ApproveSeller;

public class ApproveSellerCommandHandler : IRequestHandler<ApproveSellerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public ApproveSellerCommandHandler(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(ApproveSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Approve();

        _emails.Enqueue(seller.Email, seller.BusinessName, SellerEmails.Approved(seller, _urls));
        _context.Add(Notification.Create(seller.Id, NotificationType.AccountUpdate, "Your store is approved",
            "You can now add products and start selling.", "/seller/products"));
    }
}
