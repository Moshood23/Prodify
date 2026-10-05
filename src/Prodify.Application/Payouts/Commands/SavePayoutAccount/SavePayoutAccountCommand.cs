using MediatR;

namespace Prodify.Application.Payouts.Commands.SavePayoutAccount;

// Adds or changes the bank account the seller's payouts go to.
public class SavePayoutAccountCommand : IRequest
{
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
}
