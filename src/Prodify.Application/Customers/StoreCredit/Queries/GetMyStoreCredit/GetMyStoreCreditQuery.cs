using MediatR;

namespace Prodify.Application.Customers.StoreCredit.Queries.GetMyStoreCredit;

// The signed-in customer's store credit: what they can spend, and the latest movements.
public class GetMyStoreCreditQuery : IRequest<StoreCreditDto>
{
}

public class StoreCreditDto
{
    public decimal Balance { get; set; }
    public List<StoreCreditEntryDto> Entries { get; set; } = new();
}

public class StoreCreditEntryDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Kind { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
