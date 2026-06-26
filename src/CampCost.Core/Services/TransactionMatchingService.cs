using CampCost.Core.Entities;
using CampCost.Core.Interfaces;

namespace CampCost.Core.Services;

/// <summary>
/// Given a transaction date and a list of trips, finds the best-matching trip.
///
/// MATCHING RULES (ported from JS syncTransactions()):
///   1. Exact window  — transaction date falls within [trip.StartDate, trip.EndDate]
///   2. Buffer window — transaction date falls within [start - BUFFER_DAYS, end + BUFFER_DAYS]
///      This catches charges that post a day or two before/after the campsite dates
///      (gas fill-up on the drive home, site fee that posts late, etc.)
///   3. No match      — returns null; the expense is stored without a trip assignment
///
/// When multiple trips match, we prefer the one whose midpoint is closest to the transaction date.
/// </summary>
public class TransactionMatchingService
{
    private const int BufferDays = 2;

    /// <summary>
    /// Returns the best matching trip for the given transaction date, or null if none match.
    /// </summary>
    public Trip? FindMatchingTrip(DateTime transactionDate, IEnumerable<Trip> trips)
    {
        var tripList = trips.ToList();
        if (tripList.Count == 0) return null;

        // 1. Look for exact window match first
        var exactMatches = tripList
            .Where(t => transactionDate >= t.StartDate.Date && transactionDate <= t.EndDate.Date)
            .ToList();

        if (exactMatches.Count == 1) return exactMatches[0];
        if (exactMatches.Count > 1)  return ClosestByMidpoint(transactionDate, exactMatches);

        // 2. Fall back to buffered window
        var bufferedMatches = tripList
            .Where(t =>
                transactionDate >= t.StartDate.Date.AddDays(-BufferDays) &&
                transactionDate <= t.EndDate.Date.AddDays(BufferDays))
            .ToList();

        if (bufferedMatches.Count == 0) return null;
        if (bufferedMatches.Count == 1) return bufferedMatches[0];

        return ClosestByMidpoint(transactionDate, bufferedMatches);
    }

    private static Trip ClosestByMidpoint(DateTime date, List<Trip> candidates)
    {
        return candidates.OrderBy(t =>
        {
            var midpoint = t.StartDate.Date + TimeSpan.FromDays(
                (t.EndDate.Date - t.StartDate.Date).TotalDays / 2);
            return Math.Abs((date - midpoint).TotalDays);
        }).First();
    }
}
