using CampCost.Core.Services;
using FluentAssertions;

namespace CampCost.Tests.Services;

public class TransactionCategorizationServiceTests
{
    private readonly TransactionCategorizationService _sut = new();

    [Theory]
    [InlineData("KOA Campgrounds", "Campsite")]
    [InlineData("HipCamp Reservation", "Campsite")]
    [InlineData("Walmart Supercenter", "Groceries")]
    [InlineData("Whole Foods Market", "Groceries")]
    [InlineData("Shell Gas Station", "Gas")]
    [InlineData("Pilot Flying J", "Gas")]
    [InlineData("McDonald's", "Dining")]
    [InlineData("Chipotle Mexican Grill", "Dining")]
    [InlineData("REI Co-op", "Gear")]
    [InlineData("Bass Pro Shops", "Gear")]
    [InlineData("Amazon.com", "Shopping")]
    [InlineData("Netflix", "Entertainment")]
    [InlineData("Airbnb", "Lodging")]
    [InlineData("Uber", "Travel")]
    [InlineData("CVS Pharmacy", "Health")]
    [InlineData("AT&T Wireless", "Utilities")]
    [InlineData("Venmo Payment", "Transfer")]
    [InlineData("Some Random Merchant", "Other")]
    [InlineData("", "Other")]
    public void Categorize_ReturnsExpectedCategory(string merchantName, string expectedCategory)
    {
        _sut.Categorize(merchantName).Should().Be(expectedCategory);
    }

    [Fact]
    public void Categorize_IsCaseInsensitive()
    {
        _sut.Categorize("WALMART").Should().Be("Groceries");
        _sut.Categorize("walmart").Should().Be("Groceries");
        _sut.Categorize("WaLmArT").Should().Be("Groceries");
    }

    [Fact]
    public void Categorize_NullInput_ReturnsOther()
    {
        _sut.Categorize(null!).Should().Be("Other");
    }
}
