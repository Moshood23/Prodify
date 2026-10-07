namespace Prodify.Application.Common.Interfaces;

// A command that saves its own changes and must keep them even when it fails,
// so TransactionBehavior does not wrap it in a transaction that would roll them back.
// Example: a wrong password still has to count towards the account lockout.
public interface INoTransaction
{
}
