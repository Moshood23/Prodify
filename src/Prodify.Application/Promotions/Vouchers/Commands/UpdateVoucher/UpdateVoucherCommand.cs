using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Promotions.Vouchers.Commands.CreateVoucher;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.Vouchers.Commands.UpdateVoucher;

// Change a voucher or switch it on/off. The code itself can't change, because orders refer to it.
public class UpdateVoucherCommand : IRequest
{
    public Guid Id { get; set; }
    public string Description { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateVoucherCommandValidator : AbstractValidator<UpdateVoucherCommand>
{
    public UpdateVoucherCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DiscountType).Must(VoucherInput.IsDiscountType).WithMessage("Choose percentage or naira.");
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.Value).LessThanOrEqualTo(Voucher.MaxPercent)
            .When(x => VoucherInput.ParseDiscountType(x.DiscountType) == VoucherDiscountType.Percent)
            .WithMessage($"A percentage can be at most {Voucher.MaxPercent}.");
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0);
    }
}

public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Voucher", request.Id);

        voucher.Update(request.Description, VoucherInput.ParseDiscountType(request.DiscountType)!.Value, request.Value, request.MinOrderAmount);
        voucher.SetActive(request.IsActive);
    }
}
