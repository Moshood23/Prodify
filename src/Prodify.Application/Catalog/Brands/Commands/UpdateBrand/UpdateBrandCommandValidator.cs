using FluentValidation;
using Prodify.Application.Catalog.Common;

namespace Prodify.Application.Catalog.Brands.Commands.UpdateBrand;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .Must(CatalogUrls.IsHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("Logo must be a link starting with http:// or https://");
    }
}