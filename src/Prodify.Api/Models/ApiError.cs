namespace Prodify.Api.Models;

public class ApiError
{
    public string Message { get; set; } = null!;
    public IDictionary<string, string[]>? ValidationErrors { get; set; }
}