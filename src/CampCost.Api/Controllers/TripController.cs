using CampCost.Core.Services;
using CampCost.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampCost.Api.Controllers;

[ApiController]
[Route("api/trips")]
[Authorize]
public class TripController : ControllerBase
{
    private readonly CampCostDbContext _db;
    private readonly BudgetCalculationService _budget;

    public TripController(CampCostDbContext db, BudgetCalculationService budget)
    {
        _db = db;
        _budget = budget;
    }

    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("No user ID in token"));

    // GET /api/trips
    // Returns all planning/active trips for the current user, ordered by start date
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _db.Trips
            .Where(t => t.UserId == UserId)
            .OrderBy(t => t.StartDate)
            .ToListAsync();

        return Ok(trips);
    }

    // GET /api/trips/{id}/budget
    // Returns budget summary for one trip:
    //   planned_budget, spent, money_left, daily_burn_rate, days_left_on_road
    // days_left_on_road uses the spending burn rate model (remaining / daily_burn).
    // Returns null when there is no spending yet (burn rate cannot be estimated).
    [HttpGet("{id:guid}/budget")]
    public async Task<IActionResult> GetBudget(Guid id)
    {
        var trip = await _db.Trips
            .Include(t => t.Expenses)
            .Include(t => t.BudgetCategories)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);

        if (trip is null) return NotFound();

        var summary = _budget.Calculate(trip, trip.Expenses, trip.BudgetCategories);
        return Ok(summary);
    }

    // GET /api/trips/{id}/budget/by-category
    // Returns planned vs actual spending broken down by expense category
    [HttpGet("{id:guid}/budget/by-category")]
    public async Task<IActionResult> GetBudgetByCategory(Guid id)
    {
        var trip = await _db.Trips
            .Include(t => t.Expenses)
            .Include(t => t.BudgetCategories)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);

        if (trip is null) return NotFound();

        var plannedByCategory = trip.BudgetCategories
            .ToDictionary(bc => bc.Category, bc => bc.PlannedAmount);

        var spentByCategory = trip.Expenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        // Union all known categories (planned or spent)
        var allCategories = plannedByCategory.Keys
            .Union(spentByCategory.Keys)
            .OrderBy(c => c);

        var breakdown = allCategories.Select(cat => new
        {
            category = cat,
            planned = plannedByCategory.GetValueOrDefault(cat, 0m),
            spent = spentByCategory.GetValueOrDefault(cat, 0m),
            remaining = plannedByCategory.GetValueOrDefault(cat, 0m)
                        - spentByCategory.GetValueOrDefault(cat, 0m)
        });

        return Ok(breakdown);
    }
}
