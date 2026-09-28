namespace Prodify.Application.Catalog.Products.Common;

// Limits for product photos, shared by "add photo link" and "upload photo".
public static class ProductImageRules
{
    public const int MaxImagesPerProduct = 10;
    public const long MaxUploadBytes = 5 * 1024 * 1024;
    public const string UploadFolder = "products";

    // Works out the real file type from the first bytes of the file, so a
    // renamed .exe can't pass as a photo. Returns null for anything that
    // isn't a JPG, PNG or WebP image.
    public static string? DetectExtension(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ".jpg";

        if (header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return ".png";

        if (header.Length >= 12
            && header[..4].SequenceEqual("RIFF"u8)
            && header[8..12].SequenceEqual("WEBP"u8))
            return ".webp";

        return null;
    }
}