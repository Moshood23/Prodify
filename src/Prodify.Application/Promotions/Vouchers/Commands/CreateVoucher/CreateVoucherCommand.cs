using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.Vouchers.Commands.CreateVoucher;

public class CreateVoucherCommand : IRequest<Guid>
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    // "Percent" or "Fixed".
    public string DiscountType { get; set; } = null!;
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
}

public class CreateVoucherCommandValidator : AbstractValidator<CreateVoucherCommand>
{
    public CreateVoucherCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches("^[A-Za-z0-9]{3,30}$").WithMessage("Use 3 to 30 letters or numbers, e.g. WELCOME10.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DiscountType).Must(VoucherInput.IsDiscountType).WithMessage("Choose percentage or naira.");
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.Value).LessThanOrEqualTo(Voucher.MaxPercent)
            .When(x => VoucherInput.ParseDiscountType(x.DiscountType) == VoucherDiscountType.Percent)
            .WithMessage($"A percentage can be at most {Voucher.MaxPercent}.");
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0);
    }
}

public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        var code = Voucher.Normalize(request.Code);
        if (await _context.Vouchers.AnyAsync(v => v.Code == code, cancellationToken))
            throw new ConflictException($"There is already a voucher with the code {code}.");

        var voucher = Voucher.Create(code, request.Description, VoucherInput.ParseDiscountType(request.DiscountType)!.Value,
            request.Value, request.MinOrderAmount);
        _context.Add(voucher);
        return voucher.Id;
    }
}

public static class VoucherInput
{
    public static VoucherDiscountType? ParseDiscountType(string? value) =>
        Enum.TryParse<VoucherDiscountType>(value, ignoreCase: true, out var type) ? type : null;

    public static bool IsDiscountType(string? value) => ParseDiscountType(value) is not null;
}
