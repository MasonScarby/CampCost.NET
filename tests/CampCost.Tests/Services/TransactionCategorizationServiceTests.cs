using CampCost.Core.Services;
using FluentAssertions;

namespace CampCost.Tests.Services;

public class TransactionCategorizationServiceTests
{
    private readonly TransactionCategorizationService _sut = new();

    // Test name-only categorization (single-arg form)
    [Theory]
    [InlineData("KOA Campgrounds", "campground")]
    [InlineData("HipCamp Reservation", "campground")]
    [InlineData("ReserveAmerica Campsite", "campground")]
    [InlineData("Walmart Supercenter", "food_groceries")]
    [InlineData("Whole Foods Market", "food_groceries")]
    [InlineData("Kroger Grocery", "food_groceries")]
    [InlineData("Shell Gas Station", "fuel")]
    [InlineData("Pilot Flying J", "fuel")]
    [InlineData("Chevron", "fuel")]
    [InlineData("REI Co-op", "gear")]
    [InlineData("Bass Pro Shops", "gear")]
    [InlineData("Cabela's Outfitters", "gear")]
    [InlineData("AutoZone", "repairs")]
    [InlineData("Jiffy Lube", "repairs")]
    [InlineData("Propane Exchange", "propane_utilities")]
    [InlineData("AmeriGas Propane", "propane_utilities")]
    [InlineData("National Park Entrance Fee", "activities")]
    [InlineData("Recreation Permit", "activities")]
    [InlineData("McDonald's", "misc")]
    [InlineData("Netflix", "misc")]
    [InlineData("Some Random Merchant", "misc")]
    [InlineData("", "misc")]
    public void Categorize_ByName_ReturnsExpectedCategory(string name, string expectedCategory)
    {
        _sut.Categorize(name).Should().Be(expectedCategory);
    }

    [Fact]
    public void Categorize_MerchantNameTriggersRule()
    {
        // Raw name has no signal; merchant name identifies the category
        _sut.Categorize("SQ *001", "KOA Campgrounds").Should().Be("campground");
        _sut.Categorize("POS PURCHASE", "Shell").Should().Be("fuel");
    }

    [Fact]
    public void Categorize_IsCaseInsensitive()
    {
        _sut.Categorize("WALMART").Should().Be("food_groceries");
        _sut.Categorize("walmart").Should().Be("food_groceries");
        _sut.Categorize("WaLmArT").Should().Be("food_groceries");
    }

    [Fact]
    public void Categorize_NullInput_ReturnsMisc()
    {
        _sut.Categorize(null).Should().Be("misc");
    }

    [Fact]
    public void Categorize_BothNull_ReturnsMisc()
    {
        _sut.Categorize(null, null).Should().Be("misc");
    }

    [Fact]
    public void Categorize_FirstMatchingRuleWins()
    {
        // "state park" appears in both campground and activities patterns;
        // campground rule is listed first so it wins.
        _sut.Categorize("Oregon State Park Entrance").Should().Be("campground");
    }
}
