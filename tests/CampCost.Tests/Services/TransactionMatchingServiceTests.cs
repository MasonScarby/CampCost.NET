using CampCost.Core.Entities;
using CampCost.Core.Services;
using FluentAssertions;

namespace CampCost.Tests.Services;

public class TransactionMatchingServiceTests
{
    private readonly TransactionMatchingService _sut = new();

    private static Trip MakeTrip(string name, DateTime start, DateTime end) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        StartDate = start,
        EndDate = end,
        UserId = "user1"
    };

    [Fact]
    public void ExactDateMatch_ReturnsTrip()
    {
        var trip = MakeTrip("Summer Camp", new DateTime(2024, 7, 1), new DateTime(2024, 7, 7));
        var result = _sut.FindMatchingTrip(new DateTime(2024, 7, 4), new[] { trip });
        result.Should().Be(trip);
    }

    [Fact]
    public void TransactionOnStartDate_ReturnsTrip()
    {
        var trip = MakeTrip("Trip", new DateTime(2024, 6, 10), new DateTime(2024, 6, 15));
        _sut.FindMatchingTrip(new DateTime(2024, 6, 10), new[] { trip }).Should().Be(trip);
    }

    [Fact]
    public void TransactionOnEndDate_ReturnsTrip()
    {
        var trip = MakeTrip("Trip", new DateTime(2024, 6, 10), new DateTime(2024, 6, 15));
        _sut.FindMatchingTrip(new DateTime(2024, 6, 15), new[] { trip }).Should().Be(trip);
    }

    [Fact]
    public void TransactionWithinBuffer_ReturnsTrip()
    {
        // 2 days before start — within the BUFFER_DAYS=2 window
        var trip = MakeTrip("Trip", new DateTime(2024, 8, 5), new DateTime(2024, 8, 10));
        _sut.FindMatchingTrip(new DateTime(2024, 8, 3), new[] { trip }).Should().Be(trip);
    }

    [Fact]
    public void TransactionAfterBufferEnd_ReturnsNull()
    {
        // 3 days after end — outside BUFFER_DAYS=2
        var trip = MakeTrip("Trip", new DateTime(2024, 8, 5), new DateTime(2024, 8, 10));
        _sut.FindMatchingTrip(new DateTime(2024, 8, 13), new[] { trip }).Should().BeNull();
    }

    [Fact]
    public void NoTrips_ReturnsNull()
    {
        _sut.FindMatchingTrip(DateTime.Now, Array.Empty<Trip>()).Should().BeNull();
    }

    [Fact]
    public void MultipleMatches_ReturnsTripWithClosestMidpoint()
    {
        var close = MakeTrip("Close Trip", new DateTime(2024, 9, 1), new DateTime(2024, 9, 5));
        var far   = MakeTrip("Far Trip",   new DateTime(2024, 9, 1), new DateTime(2024, 9, 30));
        // Transaction on Sept 2 — "Close Trip" midpoint is Sept 3, "Far Trip" midpoint is Sept 15
        var result = _sut.FindMatchingTrip(new DateTime(2024, 9, 2), new[] { close, far });
        result.Should().Be(close);
    }

    [Fact]
    public void TransactionFarFromAllTrips_ReturnsNull()
    {
        var trip = MakeTrip("Trip", new DateTime(2024, 1, 1), new DateTime(2024, 1, 5));
        _sut.FindMatchingTrip(new DateTime(2024, 6, 1), new[] { trip }).Should().BeNull();
    }
}
