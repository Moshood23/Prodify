namespace Prodify.Api.Models;

public class UploadImageForm
{
    public IFormFile File { get; set; } = null!;
    public string? AltText { get; set; }
}