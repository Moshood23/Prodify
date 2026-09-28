namespace Prodify.Infrastructure.Storage;

public class FileStorageSettings
{
    // Folder for uploaded files, relative to the API project (or absolute).
    public string LocalPath { get; set; } = "uploads";

    // URL path the files are served from, e.g. /uploads/products/abc.jpg.
    public string RequestPath { get; set; } = "/uploads";

    // Start of the public URL, e.g. https://api.prodify.ng. When empty the
    // address of the current request is used (fine for local development).
    public string? PublicBaseUrl { get; set; }
}
