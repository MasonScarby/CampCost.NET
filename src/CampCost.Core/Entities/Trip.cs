namespace CampCost.Core.Entities;

/// <summary>
/// Maps to the `trips` table in Supabase.
/// A Trip is a dated camping event. Expenses within its date range (+ buffer) are linked here.
/// </summary>
public class Trip
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? BudgetAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
