using FluentValidation;

namespace Prodify.Application.Catalog.Products.Commands.UpdateProductAttributes;

public class UpdateProductAttributesCommandValidator : AbstractValidator<UpdateProductAttributesCommand>
{
    public const int MaxAttributes = 30;

    public UpdateProductAttributesCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Attributes)
            .NotNull()
            .Must(a => a.Count <= MaxAttributes)
            .WithMessage($"A product can have at most {MaxAttributes} specifications.")
            .Must(a => a.Select(x => x.Name?.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == a.Count)
            .WithMessage("Each specification name can only be used once.");

        RuleForEach(x => x.Attributes).ChildRules(attribute =>
        {
            attribute.RuleFor(a => a.Name)
                .NotEmpty()
                .MaximumLength(100);

            attribute.RuleFor(a => a.Value)
                .NotEmpty()
                .MaximumLength(500);
        });
    }
}