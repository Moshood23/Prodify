using MediatR;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Queries.GetMySeller;

public class GetMySellerQuery : IRequest<SellerProfileDto>
{
}