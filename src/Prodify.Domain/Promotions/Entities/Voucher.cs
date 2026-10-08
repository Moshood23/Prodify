using System.Text.RegularExpressions;
using Prodify.Domain.Common;

namespace Prodify.Domain.Promotions.Entities;

public enum VoucherDiscountType
{
    // Value is a percentage of the items, e.g. 10 = 10% off.
    Percent,
    // Value is an amount in naira, e.g. 2000 = 2,000 naira off.
    Fixed
}

// A code customers enter at checkout, e.g. WELCOME10. Made by admins; Prodify pays the
// discount, so sellers are paid as if there were none. Discounts the items, not delivery.
public class Voucher : AuditableEntity
{
    public const int MaxCodeLength = 30;
    public const decimal MaxPercent = 90m;

    public string Code { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public VoucherDiscountType DiscountType { get; private set; }
    public decimal Value { get; private set; }

    // The items must add up to at least this (0 = any order).
    public decimal MinOrderAmount { get; private set; }

    // Switched off codes are refused at checkout.
    public bool IsActive { get; private set; }

    // How many orders have used it.
    public int TimesUsed { get; private set; }

    private Voucher()
    {
    }

    private Voucher(Guid id) : base(id)
    {
    }

    public static Voucher Create(string code, string description, VoucherDiscountType discountType, decimal value, decimal minOrderAmount)
    {
        var normalized = Normalize(code);
        if (!Regex.IsMatch(normalized, $"^[A-Z0-9]{{3,{MaxCodeLength}}}$"))
            throw new ArgumentException($"A voucher code is 3 to {MaxCodeLength} letters or numbers, e.g. WELCOME10.", nameof(code));

        var voucher = new Voucher(Guid.NewGuid()) { Code = normalized, IsActive = true };
        voucher.Update(description, discountType, value, minOrderAmount);
        return voucher;
    }

    public void Update(string description, VoucherDiscountType discountType, decimal value, decimal minOrderAmount)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Describe the voucher, e.g. \"10% off your first order\".", nameof(description));

        if (discountType == VoucherDiscountType.Percent && (value <= 0 || value > MaxPercent))
            throw new ArgumentException($"A percentage voucher must be between 1 and {MaxPercent}%.", nameof(value));

        if (discountType == VoucherDiscountType.Fixed && value <= 0)
            throw new ArgumentException("A voucher must take off more than 0 naira.", nameof(value));

        if (minOrderAmount < 0)
            throw new ArgumentException("The minimum order can't be negative.", nameof(minOrderAmount));

        Description = description.Trim();
        DiscountType = discountType;
        Value = value;
        MinOrderAmount = minOrderAmount;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    // The naira taken off items worth itemsTotal. Never more than the items themselves.
    public decimal DiscountFor(decimal itemsTotal)
    {
        var discount = DiscountType == VoucherDiscountType.Percent
            ? Math.Round(itemsTotal * Value / 100m, 2, MidpointRounding.AwayFromZero)
            : Value;

        return Math.Min(discount, itemsTotal);
    }

    public void RecordUse() => TimesUsed++;

    public static string Normalize(string code) => (code ?? "").Trim().ToUpperInvariant();
}
