namespace CampCost.Core.Interfaces;

/// <summary>
/// Defines the contract for all Plaid API operations.
/// Infrastructure implements this; Core and Api depend only on this interface.
/// </summary>
public interface IPlaidService
{
    /// <summary>Creates a Plaid Link token for the given user to start the bank-link flow.</summary>
    Task<string> CreateLinkTokenAsync(string userId);

    /// <summary>Exchanges the short-lived public token (from Link UI) for a permanent access token.</summary>
    Task<(string AccessToken, string ItemId)> ExchangePublicTokenAsync(string publicToken);

    /// <summary>Pulls all transactions for an access token within the given date window.</summary>
    Task<IReadOnlyList<PlaidTransaction>> GetTransactionsAsync(
        string accessToken, DateTime startDate, DateTime endDate);
}

/// <summary>A lightweight DTO for Plaid transaction data — no Plaid SDK types leak into Core.</summary>
public record PlaidTransaction(
    string PlaidTransactionId,
    string MerchantName,
    decimal Amount,
    DateTime Date
);
