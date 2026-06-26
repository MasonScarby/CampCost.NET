namespace CampCost.Core.Entities;

/// <summary>
/// Maps to the `trip_stops` table. One row per planned stop on the route.
/// Arrival/departure dates are nullable so users can add stops without dates yet.
/// Ownership is via the parent trip (join RLS pattern B).
/// </summary>
public class TripStop
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateOnly? ArrivalDate { get; set; }
    public DateOnly? DepartureDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Trip? Trip { get; set; }
}
