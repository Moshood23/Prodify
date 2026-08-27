namespace Prodify.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? CustomerId { get; }
    Guid? SellerId { get; }
    bool IsAuthenticated { get; }
}