using FluentValidation;

namespace Prodify.Application.Catalog.Products.Commands.AddProductImage;

public class AddProductImageCommandValidator : AbstractValidator<AddProductImageCommand>
{
    public AddProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(1000)
            .Must(BeAnHttpUrl)
            .WithMessage("Image URL must be a valid http or https address.");

        RuleFor(x => x.AltText)
            .MaximumLength(250);
    }

    private static bool BeAnHttpUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}