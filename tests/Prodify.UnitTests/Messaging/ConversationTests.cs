using Prodify.Domain.Messaging.Entities;

namespace Prodify.UnitTests.Messaging;

public class ConversationTests
{
    private static readonly DateTime Now = new(2026, 10, 12, 9, 0, 0, DateTimeKind.Utc);

    private static Conversation NewConversation() =>
        Conversation.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "  Golden Penny Spaghetti  ");

    [Fact]
    public void Post_CountsUnreadForTheOtherSide()
    {
        var conversation = NewConversation();

        var message = conversation.Post(MessageSender.Customer, "  Is it in stock?  ", Now);
        conversation.Post(MessageSender.Customer, "Hello?", Now.AddMinutes(1));

        Assert.Equal("Golden Penny Spaghetti", conversation.Subject);
        Assert.Equal("Is it in stock?", message.Body);
        Assert.Equal(conversation.Id, message.ConversationId);
        Assert.Equal(2, conversation.SellerUnread);
        Assert.Equal(0, conversation.CustomerUnread);
        Assert.Equal("Hello?", conversation.LastMessagePreview);
        Assert.Equal(MessageSender.Customer, conversation.LastSender);
        Assert.Equal(Now.AddMinutes(1), conversation.LastMessageAt);

        conversation.MarkReadBy(MessageSender.Seller);
        conversation.Post(MessageSender.Seller, "Yes, plenty.", Now.AddMinutes(2));

        Assert.Equal(0, conversation.SellerUnread);
        Assert.Equal(1, conversation.CustomerUnread);
    }

    [Fact]
    public void Post_LongMessage_PreviewIsShortened()
    {
        var conversation = NewConversation();
        conversation.Post(MessageSender.Customer, new string('a', 500), Now);

        Assert.Equal(Conversation.PreviewLength + 3, conversation.LastMessagePreview.Length);
        Assert.EndsWith("...", conversation.LastMessagePreview);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Post_Empty_Throws(string body)
    {
        Assert.Throws<ArgumentException>(() => NewConversation().Post(MessageSender.Customer, body, Now));
    }

    [Fact]
    public void Post_TooLong_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NewConversation().Post(MessageSender.Customer, new string('a', ConversationMessage.MaxLength + 1), Now));
    }

    [Fact]
    public void Post_ContactDetails_AreHidden()
    {
        var message = NewConversation().Post(MessageSender.Seller, "Call me on 0803 123 4567 or mail sales@shop.ng", Now);

        Assert.Equal("Call me on [hidden] or mail [hidden]", message.Body);
        Assert.True(message.HadContactDetails);
    }

    [Theory]
    [InlineData("08031234567")]
    [InlineData("+234 803 123 4567")]
    [InlineData("234-803-123-4567")]
    [InlineData("(0803) 123-4567")]
    [InlineData("0803.123.4567")]
    [InlineData("tolu.ade@gmail.com")]
    [InlineData("tolu at gmail dot com")]
    [InlineData("tolu (at) gmail (dot) com")]
    public void Hide_FindsContactDetails(string contact)
    {
        var (text, hidden) = ContactDetails.Hide($"Reach me: {contact} thanks");

        Assert.True(hidden, text);
        Assert.Equal("Reach me: [hidden] thanks", text);
    }

    [Theory]
    [InlineData("Order ORD-20261011-1A2B3C4D arrived late")]
    [InlineData("Order ORD-20261011-12345678 arrived late")]
    [InlineData("It costs 189000 naira, I want 2")]
    [InlineData("Is the 2025 model 128GB?")]
    [InlineData("Size 42, colour black")]
    public void Hide_LeavesOrdinaryTextAlone(string text)
    {
        var (result, hidden) = ContactDetails.Hide(text);

        Assert.False(hidden);
        Assert.Equal(text, result);
    }
}
