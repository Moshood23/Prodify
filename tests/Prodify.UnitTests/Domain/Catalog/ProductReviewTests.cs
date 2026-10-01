using Prodify.Application.Catalog.Reviews.Common;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.UnitTests.Domain.Catalog;

public class ProductReviewTests
{
    [Fact]
    public void Create_TrimsTextAndDropsEmptyParts()
    {
        var review = ProductReview.Create(Guid.NewGuid(), Guid.NewGuid(), "Tolu A.", 4, "  Great  ", "   ");

        Assert.Equal(4, review.Rating);
        Assert.Equal("Great", review.Title);
        Assert.Null(review.Comment);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Rating_MustBeOneToFive(int rating)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProductReview.Create(Guid.NewGuid(), Guid.NewGuid(), "Tolu A.", rating, null, null));
    }

    [Fact]
    public void Hide_NeedsAReason_AndUnhideClearsIt()
    {
        var review = ProductReview.Create(Guid.NewGuid(), Guid.NewGuid(), "Tolu A.", 1, null, "Rude words");

        Assert.Throws<ArgumentException>(() => review.Hide(" "));

        review.Hide("Offensive language");
        Assert.True(review.IsHidden);

        review.Unhide();
        Assert.False(review.IsHidden);
        Assert.Null(review.HiddenReason);
    }

    [Theory]
    [InlineData("Tolu", "Ade", "Tolu A.")]
    [InlineData("Chioma", "okafor", "Chioma O.")]
    [InlineData("Emeka", "", "Emeka")]
    public void ShortName_KeepsFirstNameAndInitial(string first, string last, string expected)
    {
        Assert.Equal(expected, ReviewRules.ShortName(first, last));
    }
}
