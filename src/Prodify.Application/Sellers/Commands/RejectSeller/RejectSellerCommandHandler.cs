using MediatR;
using Prodify.Application.Common.Emails;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;
using Prodify.Domain.Notifications.Entities;

namespace Prodify.Application.Sellers.Commands.RejectSeller;

public class RejectSellerCommandHandler : IRequestHandler<RejectSellerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailQueue _emails;
    private readonly IAppUrls _urls;

    public RejectSellerCommandHandler(IApplicationDbContext context, IEmailQueue emails, IAppUrls urls)
    {
        _context = context;
        _emails = emails;
        _urls = urls;
    }

    public async Task Handle(RejectSellerCommand request, CancellationToken cancellationToken)
    {
        var seller = await _context.GetSellerOrThrowAsync(request.SellerId, cancellationToken);
        seller.Reject(request.Reason);

        _emails.Enqueue(seller.Email, seller.BusinessName, SellerEmails.Rejected(seller, request.Reason.Trim(), _urls));
        _context.Add(Notification.Create(seller.Id, NotificationType.AccountUpdate, "Your seller application was not approved",
            $"You can update your details and apply again. Reason: {request.Reason.Trim()}", "/seller"));
    }
}
