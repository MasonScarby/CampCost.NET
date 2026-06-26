using CampCost.Core.Entities;
using CampCost.Core.Interfaces;
using CampCost.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace CampCost.Api.Controllers;

[ApiController]
[Route("api/plaid")]
[Authorize]
public class PlaidController : ControllerBase
{
    private readonly IPlaidService _plaid;
    private readonly ITransactionSyncService _sync;
    private readonly CampCostDbContext _db;

    public PlaidController(IPlaidService plaid, ITransactionSyncService sync, CampCostDbContext db)
    {
        _plaid = plaid;
        _sync = sync;
        _db = db;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("No user ID in token");

    // POST /api/plaid/link-token
    // Creates a Plaid Link token so the frontend can open the bank-connection UI
    [HttpPost("link-token")]
    public async Task<IActionResult> CreateLinkToken()
    {
        var token = await _plaid.CreateLinkTokenAsync(UserId);
        return Ok(new { link_token = token });
    }

    // POST /api/plaid/exchange-token
    // Swaps the short-lived public token for a permanent access token.
    // Upserts on item_id — re-linking the same bank updates the access token.
    [HttpPost("exchange-token")]
    public async Task<IActionResult> ExchangeToken([FromBody] ExchangeRequest req)
    {
        var (accessToken, itemId) = await _plaid.ExchangePublicTokenAsync(req.PublicToken);

        var existing = await _db.PlaidConnections
            .FirstOrDefaultAsync(c => c.ItemId == itemId);

        if (existing is not null)
        {
            existing.AccessToken = accessToken;
            if (req.InstitutionName is not null)
                existing.InstitutionName = req.InstitutionName;
        }
        else
        {
            _db.PlaidConnections.Add(new PlaidConnection
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                AccessToken = accessToken,
                ItemId = itemId,
                InstitutionName = req.InstitutionName ?? "Unknown",
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();

        return Ok(new { success = true, item_id = itemId });
    }

    // POST /api/plaid/sync
    // Manually pulls new transactions for the current user and assigns them to trips
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var result = await _sync.SyncForUserAsync(UserId);
        return Ok(result);
    }

    // POST /api/plaid/webhook
    // Receives push notifications from Plaid. Only acts on TRANSACTIONS webhooks.
    // Looks up the affected user by item_id and syncs only them.
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] WebhookPayload? payload)
    {
        if (payload?.WebhookType != "TRANSACTIONS")
            return Ok(new { received = true });

        if (payload.ItemId is not null)
        {
            var connection = await _db.PlaidConnections
                .FirstOrDefaultAsync(c => c.ItemId == payload.ItemId);

            if (connection is not null)
            {
                await _sync.SyncForUserAsync(connection.UserId);
                return Ok(new { received = true });
            }
        }

        await _sync.SyncAllAsync();
        return Ok(new { received = true });
    }
}

public record ExchangeRequest(string PublicToken, string? InstitutionName);

public record WebhookPayload(
    [property: JsonPropertyName("webhook_type")] string? WebhookType,
    [property: JsonPropertyName("item_id")] string? ItemId
);
