using Prodify.Domain.Common;

namespace Prodify.Domain.Promotions.Entities;

public enum FlashSaleStatus
{
    // Waiting for Prodify to look at it.
    Pending,
    // Goes live at StartsAt (or straight away if that has passed) and ends at EndsAt.
    Approved,
    Rejected,
    // Withdrawn by the seller.
    Cancelled,
    // Ended early by Prodify.
    Stopped
}

// A seller puts a limited number of units of one product variant on sale for a few hours or days.
// Prodify approves it first. The seller pays for it: they are paid on the sale price.
public class FlashSale : AuditableEntity
{
    public const int MaxNoteLength = 500;
    public const int MaxUnits = 10_000;
    public const int MinHours = 1;
    public const int MaxDays = 7;
    // Scheduled at most this far ahead.
    public const int MaxDaysAhead = 60;
    // A real deal: at least this much off.
    public const decimal MinPercentOff = 5m;

    public Guid SellerId { get; private set; }
    public Guid ProductVariantId { get; private set; }

    public decimal SalePrice { get; private set; }

    // The variant's price when the sale was set up. Shoppers are shown the lower of this and
    // today's price as the "was" price, so raising the price before a sale doesn't fake a bigger discount.
    public decimal RegularPrice { get; private set; }

    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }

    // Units on offer at the sale price, and how many orders have taken so far.
    public int Quantity { get; private set; }
    public int SoldQuantity { get; private set; }

    public int MaxPerCustomer { get; private set; }

    public FlashSaleStatus Status { get; private set; }

    // Prodify's reason for rejecting or stopping it.
    public string? ReviewNote { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    private FlashSale()
    {
    }

    private FlashSale(Guid id) : base(id)
    {
    }

    public static FlashSale Create(
        Guid sellerId,
        Guid productVariantId,
        decimal regularPrice,
        decimal salePrice,
        int quantity,
        int maxPerCustomer,
        DateTime startsAt,
        DateTime endsAt,
        DateTime now,
        int maxPerCustomerLimit)
    {
        if (sellerId == Guid.Empty)
            throw new ArgumentException("A flash sale needs a seller.", nameof(sellerId));

        if (productVariantId == Guid.Empty)
            throw new ArgumentException("Choose the product to put on sale.", nameof(productVariantId));

        if (salePrice <= 0)
            throw new ArgumentException("The sale price must be more than 0.", nameof(salePrice));

        if (salePrice > MaxSalePriceFor(regularPrice))
            throw new ArgumentException(
                $"The sale price must be at least {MinPercentOff}% below the current price, so at most {MaxSalePriceFor(regularPrice):0.##}.",
                nameof(salePrice));

        if (quantity < 1 || quantity > MaxUnits)
            throw new ArgumentException($"Offer between 1 and {MaxUnits} units.", nameof(quantity));

        if (maxPerCustomer < 1 || maxPerCustomer > maxPerCustomerLimit)
            throw new ArgumentException($"Each customer can buy between 1 and {maxPerCustomerLimit} units.", nameof(maxPerCustomer));

        if (maxPerCustomer > quantity)
            throw new ArgumentException("The limit per customer can't be more than the units on offer.", nameof(maxPerCustomer));

        // A few minutes of slack, for the time it takes to fill in the form.
        if (startsAt < now.AddMinutes(-10))
            throw new ArgumentException("The sale can't start in the past.", nameof(startsAt));

        if (startsAt > now.AddDays(MaxDaysAhead))
            throw new ArgumentException($"Schedule the sale at most {MaxDaysAhead} days ahead.", nameof(startsAt));

        var length = endsAt - startsAt;
        if (length < TimeSpan.FromHours(MinHours) || length > TimeSpan.FromDays(MaxDays))
            throw new ArgumentException($"A flash sale lasts between {MinHours} hour and {MaxDays} days.", nameof(endsAt));

        return new FlashSale(Guid.NewGuid())
        {
            SellerId = sellerId,
            ProductVariantId = productVariantId,
            RegularPrice = regularPrice,
            SalePrice = salePrice,
            Quantity = quantity,
            MaxPerCustomer = maxPerCustomer,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = FlashSaleStatus.Pending
        };
    }

    // The highest sale price that is still a real discount on price.
    public static decimal MaxSalePriceFor(decimal price) =>
        Math.Floor(price * (100m - MinPercentOff) / 100m);

    public int UnitsLeft => Math.Max(0, Quantity - SoldQuantity);

    // Approved and inside its time window. It may still be sold out.
    public bool IsRunningAt(DateTime now) =>
        Status == FlashSaleStatus.Approved && StartsAt <= now && now < EndsAt;

    // Still to happen or happening: blocks another sale on the same variant at the same time.
    public bool IsActive => Status is FlashSaleStatus.Pending or FlashSaleStatus.Approved;

    public void Approve(DateTime now)
    {
        if (Status != FlashSaleStatus.Pending)
            throw new InvalidOperationException("Only a flash sale waiting for review can be approved.");

        if (EndsAt <= now)
            throw new InvalidOperationException("This flash sale's end time has passed. The seller needs to set up a new one.");

        Status = FlashSaleStatus.Approved;
        ReviewNote = null;
        ReviewedAt = now;
    }

    public void Reject(string note, DateTime now)
    {
        if (Status != FlashSaleStatus.Pending)
            throw new InvalidOperationException("Only a flash sale waiting for review can be rejected.");

        ReviewNote = CleanNote(note);
        Status = FlashSaleStatus.Rejected;
        ReviewedAt = now;
    }

    // Prodify ends an approved sale early, e.g. after a complaint.
    public void Stop(string note, DateTime now)
    {
        if (Status != FlashSaleStatus.Approved || EndsAt <= now)
            throw new InvalidOperationException("Only an approved flash sale that hasn't ended can be stopped.");

        ReviewNote = CleanNote(note);
        Status = FlashSaleStatus.Stopped;
        ReviewedAt = now;
    }

    // The seller withdraws it: before review, or after approval as long as it hasn't ended.
    public void Cancel(DateTime now)
    {
        if (!IsActive || EndsAt <= now)
            throw new InvalidOperationException("This flash sale can no longer be cancelled.");

        Status = FlashSaleStatus.Cancelled;
    }

    // An order took units at the sale price.
    public void Take(int units, DateTime now)
    {
        if (units <= 0)
            throw new ArgumentOutOfRangeException(nameof(units), "Take at least one unit.");

        if (!IsRunningAt(now))
            throw new InvalidOperationException("This flash sale isn't running.");

        if (units > UnitsLeft)
            throw new InvalidOperationException("Not enough flash sale units left.");

        SoldQuantity += units;
    }

    // A cancelled order gives its units back, so someone else can have them.
    public void GiveBack(int units)
    {
        if (units <= 0)
            throw new ArgumentOutOfRangeException(nameof(units), "Give back at least one unit.");

        SoldQuantity = Math.Max(0, SoldQuantity - units);
    }

    private static string CleanNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Give the seller a reason.", nameof(note));

        var trimmed = note.Trim();
        if (trimmed.Length > MaxNoteLength)
            throw new ArgumentException($"Keep the reason under {MaxNoteLength} characters.", nameof(note));

        return trimmed;
    }
}

// Units of a flash sale taken by one order, so they can be given back if the order is cancelled
// and so each customer's limit can be checked.
public class FlashSaleClaim : AuditableEntity
{
    public Guid FlashSaleId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }

    // The order (or this seller's part of it) was cancelled and the units went back to the sale.
    public bool IsReleased { get; private set; }

    private FlashSaleClaim()
    {
    }

    private FlashSaleClaim(Guid id) : base(id)
    {
    }

    public static FlashSaleClaim Create(FlashSale sale, Guid orderId, Guid customerId, int quantity, DateTime now)
    {
        sale.Take(quantity, now);

        return new FlashSaleClaim(Guid.NewGuid())
        {
            FlashSaleId = sale.Id,
            OrderId = orderId,
            CustomerId = customerId,
            ProductVariantId = sale.ProductVariantId,
            Quantity = quantity
        };
    }

    public void Release(FlashSale sale)
    {
        if (IsReleased)
            return;

        if (sale.Id != FlashSaleId)
            throw new ArgumentException("That is a different flash sale.", nameof(sale));

        sale.GiveBack(Quantity);
        IsReleased = true;
    }
}
