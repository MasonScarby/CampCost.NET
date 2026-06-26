namespace CampCost.Core.Entities;

/// <summary>
/// Maps to the `expenses` table in Supabase.
/// Created/upserted by the sync process when Plaid transactions are pulled.
/// plaid_transaction_id is the deduplication key — we never insert the same Plaid tx twice.
/// </summary>
public class Expense
{
    public Guid Id { get; set; }
    public Guid? TripId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PlaidTransactionId { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Trip? Trip { get; set; }
}
