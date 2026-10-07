using Prodify.Infrastructure.Storage;

namespace Prodify.IntegrationTests.Storage;

public class CloudinaryFileStorageTests
{
    [Fact]
    public void Reads_the_public_id_from_a_photo_url()
    {
        var url = "https://res.cloudinary.com/prodify/image/upload/f_auto,q_auto/v1712345678/prodify/products/abc123.jpg";

        Assert.Equal("prodify/products/abc123", CloudinaryFileStorage.GetPublicId(url, "prodify"));
    }

    [Theory]
    [InlineData("https://images.unsplash.com/photo-1.jpg")]
    [InlineData("https://res.cloudinary.com/someone-else/image/upload/v1/prodify/products/abc.jpg")]
    [InlineData("https://res.cloudinary.com/prodify/image/upload/prodify/products/abc.jpg")]
    [InlineData("not a url")]
    public void Ignores_urls_it_did_not_create(string url)
    {
        Assert.Null(CloudinaryFileStorage.GetPublicId(url, "prodify"));
    }
}
