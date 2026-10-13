using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Messaging.Common;
using Prodify.Domain.Messaging.Entities;

namespace Prodify.Application.Messaging.Queries;

// The signed-in customer's or seller's inbox, latest first.
public class ListMyConversationsQuery : IRequest<List<ConversationSummaryDto>>
{
    public MessageSender As { get; set; }
}

public class ListMyConversationsQueryHandler : IRequestHandler<ListMyConversationsQuery, List<ConversationSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListMyConversationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<List<ConversationSummaryDto>> Handle(ListMyConversationsQuery request, CancellationToken cancellationToken) =>
        _context.ToSummariesAsync(_context.Conversations.OwnedBy(_currentUser, request.As), request.As, cancellationToken);
}

public class GetMyConversationQuery : IRequest<ConversationDto>
{
    public Guid ConversationId { get; set; }
    public MessageSender As { get; set; }
}

public class GetMyConversationQueryHandler : IRequestHandler<GetMyConversationQuery, ConversationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyConversationQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ConversationDto> Handle(GetMyConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = _context.Conversations.OwnedBy(_currentUser, request.As).Where(c => c.Id == request.ConversationId);
        return await _context.GetConversationDtoAsync(conversation, request.As, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);
    }
}

// For the badge next to "Messages".
public class GetUnreadMessageCountQuery : IRequest<int>
{
    public MessageSender As { get; set; }
}

public class GetUnreadMessageCountQueryHandler : IRequestHandler<GetUnreadMessageCountQuery, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUnreadMessageCountQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<int> Handle(GetUnreadMessageCountQuery request, CancellationToken cancellationToken)
    {
        var mine = _context.Conversations.OwnedBy(_currentUser, request.As);
        return request.As == MessageSender.Customer
            ? mine.SumAsync(c => c.CustomerUnread, cancellationToken)
            : mine.SumAsync(c => c.SellerUnread, cancellationToken);
    }
}

// Prodify's staff can read any conversation (e.g. while deciding a return), but not write in it.
// search: an order number, a customer's or store's name, or words from the subject.
public class ListAdminConversationsQuery : IRequest<List<ConversationSummaryDto>>
{
    public string? Search { get; set; }
}

public class ListAdminConversationsQueryHandler : IRequestHandler<ListAdminConversationsQuery, List<ConversationSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAdminConversationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<ConversationSummaryDto>> Handle(ListAdminConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = _context.Conversations.AsQueryable();
        var search = request.Search?.Trim();

        if (!string.IsNullOrEmpty(search))
        {
            conversations = conversations.Where(c =>
                c.Subject.Contains(search)
                || _context.Sellers.Any(s => s.Id == c.SellerId && s.BusinessName.Contains(search))
                || _context.Customers.Any(u => u.Id == c.CustomerId
                    && (u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search))));
        }

        return _context.ToSummariesAsync(conversations, null, cancellationToken);
    }
}

public class GetAdminConversationQuery : IRequest<ConversationDto>
{
    public Guid ConversationId { get; set; }
}

public class GetAdminConversationQueryHandler : IRequestHandler<GetAdminConversationQuery, ConversationDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminConversationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationDto> Handle(GetAdminConversationQuery request, CancellationToken cancellationToken) =>
        await _context.GetConversationDtoAsync(_context.Conversations.Where(c => c.Id == request.ConversationId), null, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), request.ConversationId);
}
