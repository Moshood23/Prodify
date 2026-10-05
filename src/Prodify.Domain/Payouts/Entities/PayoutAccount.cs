using System.Text.RegularExpressions;
using Prodify.Domain.Common;

namespace Prodify.Domain.Payouts.Entities;

// The Nigerian bank account a seller's payouts go to. One per seller.
public class PayoutAccount : AuditableEntity
{
    public Guid SellerId { get; private set; }
    public string BankName { get; private set; } = null!;

    // 10-digit NUBAN account number.
    public string AccountNumber { get; private set; } = null!;
    public string AccountName { get; private set; } = null!;

    private PayoutAccount()
    {
    }

    private PayoutAccount(Guid id, Guid sellerId) : base(id)
    {
        SellerId = sellerId;
    }

    public static PayoutAccount Create(Guid sellerId, string bankName, string accountNumber, string accountName)
    {
        var account = new PayoutAccount(Guid.NewGuid(), sellerId);
        account.Update(bankName, accountNumber, accountName);
        return account;
    }

    public void Update(string bankName, string accountNumber, string accountName)
    {
        if (string.IsNullOrWhiteSpace(bankName))
            throw new ArgumentException("Bank name cannot be empty.", nameof(bankName));
        if (string.IsNullOrWhiteSpace(accountName))
            throw new ArgumentException("Account name cannot be empty.", nameof(accountName));

        accountNumber = accountNumber?.Trim() ?? "";
        if (!Regex.IsMatch(accountNumber, @"^\d{10}$"))
            throw new ArgumentException("Account number must be 10 digits.", nameof(accountNumber));

        BankName = bankName.Trim();
        AccountNumber = accountNumber;
        AccountName = accountName.Trim();
    }
}
