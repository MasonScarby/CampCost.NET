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

    public async Task<SyncResult> SyncForUserAsync(Guid userId)
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
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var startDate = connection.LastSyncAt.HasValue
                ? DateOnly.FromDateTime(connection.LastSyncAt.Value.AddDays(-2))
                : endDate.AddDays(-90);

            var transactions = await _plaid.GetTransactionsAsync(
                connection.AccessToken, startDate, endDate);
            fetched += transactions.Count;

            // Only match against planning/active trips — same as original JS filter
            var trips = await _db.Trips
                .Where(t => t.UserId == connection.UserId &&
                            (t.Status == "planning" || t.Status == "active"))
                .ToListAsync();

            var existingIds = (await _db.Expenses
                .Where(e => e.UserId == connection.UserId && e.PlaidTransactionId != null)
                .Select(e => e.PlaidTransactionId)
                .ToListAsync()).ToHashSet();

            foreach (var tx in transactions)
            {
                // Skip credits (money in) — Plaid: negative amount = credit
                if (tx.Amount < 0) { skipped++; continue; }

                // Deduplicate by plaid_transaction_id
                if (existingIds.Contains(tx.TransactionId)) { skipped++; continue; }

                var matchedTrip = _matcher.FindMatchingTrip(tx.Date, trips);

                // No trip to assign to — skip (original JS: if (!tripMatch) continue)
                if (matchedTrip is null) { skipped++; continue; }

                _db.Expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    UserId = connection.UserId,
                    TripId = matchedTrip.Id,
                    PlaidTransactionId = tx.TransactionId,
                    MerchantName = tx.MerchantName,
                    Amount = tx.Amount,
                    Category = _categorizer.Categorize(tx.Name, tx.MerchantName),
                    ExpenseDate = tx.Date,
                    Source = "plaid",
                    Reviewed = false,
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
