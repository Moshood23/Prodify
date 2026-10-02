using FluentValidation;

namespace Prodify.Application.Wishlist.Queries.GetWishlist;

public class GetWishlistQueryValidator : AbstractValidator<GetWishlistQuery>
{
    public GetWishlistQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 60);
    }
}
