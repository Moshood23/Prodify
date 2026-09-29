namespace Prodify.Application.Common.Interfaces;

// Links to pages of the website, for emails.
public interface IAppUrls
{
    // e.g. Page("/orders/123") -> "https://prodify.ng/orders/123"
    string Page(string path);
}
