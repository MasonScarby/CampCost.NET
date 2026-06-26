using CampCost.Core.Entities;
using CampCost.Core.Interfaces;
using CampCost.Core.Services;
using CampCost.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CampCost.Infrastructure.Services;

public class TransactionSyncService : ITransactionSyncService
{
    private readonly CampCostDbContext _db;
    private readonly IPlaidService _plaid;
    private readonly TransactionCategorizationService _categorizer;
    private readonly TransactionMatchingService _matcher;

    public TransactionSyncService(
        CampCostDbContext db,
        IPlaidService plaid,
        TransactionCategorizationService categorizer,
        TransactionMatchingService matcher)
    {
        _db = db;
        _plaid = plaid;
        _categorizer = categorizer;
        _matcher = matcher;
    }

    public async Task<SyncResult> SyncForUserAsync(string userId)
    {
        var connections = await _db.PlaidConnections
            .Where(c => c.UserId == userId)
            .ToListAsync();
        return await SyncConnections(connections);
    }

    public async Task<SyncResult> SyncAllAsync()
    {
        var connections = await _db.PlaidConnections.ToListAsync();
        return await SyncConnections(connections);
    }

    private async Task<SyncResult> SyncConnections(List<PlaidConnection> connections)
    {
        int fetched = 0, upserted = 0, skipped = 0;

        foreach (var connection in connections)
        {
            var end = DateTime.UtcNow;
            var start = connection.LastSyncAt?.AddDays(-2) ?? end.AddDays(-90);

            var transactions = await _plaid.GetTransactionsAsync(connection.AccessToken, start, end);
            fetched += transactions.Count;

            var trips = await _db.Trips
                .Where(t => t.UserId == connection.UserId)
                .ToListAsync();

            var existingIds = (await _db.Expenses
                .Where(e => e.UserId == connection.UserId)
                .Select(e => e.PlaidTransactionId)
                .ToListAsync()).ToHashSet();

            foreach (var tx in transactions)
            {
                if (existingIds.Contains(tx.PlaidTransactionId)) { skipped++; continue; }

                var matchedTrip = _matcher.FindMatchingTrip(tx.Date, trips);

                _db.Expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = connection.UserId,
                    TripId = matchedTrip?.Id,
                    PlaidTransactionId = tx.PlaidTransactionId,
                    MerchantName = tx.MerchantName,
                    Amount = tx.Amount,
                    Category = _categorizer.Categorize(tx.MerchantName),
                    TransactionDate = tx.Date,
                    CreatedAt = DateTime.UtcNow
                });
                upserted++;
            }

            connection.LastSyncAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return new SyncResult(connections.Count, fetched, upserted, skipped);
    }
}
