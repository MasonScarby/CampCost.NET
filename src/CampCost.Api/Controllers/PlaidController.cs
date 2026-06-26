using CampCost.Core.Entities;
using CampCost.Core.Interfaces;
using CampCost.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    // Swaps the short-lived public token (from Link UI) for a permanent access token, saves to DB
    [HttpPost("exchange-token")]
    public async Task<IActionResult> ExchangeToken([FromBody] ExchangeRequest req)
    {
        var (accessToken, itemId) = await _plaid.ExchangePublicTokenAsync(req.PublicToken);

        _db.PlaidConnections.Add(new PlaidConnection
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            AccessToken = accessToken,
            ItemId = itemId,
            InstitutionName = req.InstitutionName ?? "Unknown",
            CreatedAt = DateTime.UtcNow
        });
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
    // Receives push notifications from Plaid and triggers a sync for all users
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var result = await _sync.SyncAllAsync();
        return Ok(result);
    }
}

public record ExchangeRequest(string PublicToken, string? InstitutionName);
