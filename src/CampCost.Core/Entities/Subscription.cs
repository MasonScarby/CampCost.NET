namespace CampCost.Core.Entities;

/// <summary>
/// Maps to the `subscriptions` table. One row per user.
/// Plan: 'free' | 'pro'. Status: 'active' | 'cancelled' | 'past_due'.
/// </summary>
public class Subscription
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Plan { get; set; } = "free";
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}
