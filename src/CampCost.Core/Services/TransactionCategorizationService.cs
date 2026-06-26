using System.Text.RegularExpressions;

namespace CampCost.Core.Services;

/// <summary>
/// Maps merchant names to the expense_category enum values used by the Supabase DB.
/// Category strings MUST exactly match the DB enum: fuel, campground, food_groceries,
/// gear, repairs, propane_utilities, activities, misc.
/// Ported directly from the original JS categorize() function.
/// </summary>
public class TransactionCategorizationService
{
    private static readonly (Regex Pattern, string Category)[] Rules =
    {
        (new Regex(@"shell|bp |chevron|exxon|mobil|pilot|love['\s]?s|loves|flying j|speedway|sunoco|circle k|casey|kwik trip|fuel|gasoline|gas station", RegexOptions.IgnoreCase), "fuel"),
        (new Regex(@"koa|hipcamp|reserveamerica|campground|rv\s?park|rv resort|state park|campsite|boondock|harvest host", RegexOptions.IgnoreCase), "campground"),
        (new Regex(@"walmart|kroger|safeway|aldi|publix|heb|meijer|whole foods|trader joe|grocery|supermarket|food lion|sprouts", RegexOptions.IgnoreCase), "food_groceries"),
        (new Regex(@"rei|cabela['\s]?s|bass pro|academy sports|backcountry|moosejaw|gear", RegexOptions.IgnoreCase), "gear"),
        (new Regex(@"autozone|o['\s]?reilly|napa auto|advance auto|mechanic|muffler|tire|jiffy lube|repair|firestone|pep boys", RegexOptions.IgnoreCase), "repairs"),
        (new Regex(@"propane|amerigas|ferrellgas|blue rhino|utilities|dump station", RegexOptions.IgnoreCase), "propane_utilities"),
        (new Regex(@"national park|state park|entrance fee|permit|recreation", RegexOptions.IgnoreCase), "activities"),
    };

    /// <summary>
    /// Returns the DB enum category string for a merchant name.
    /// Falls back to "misc" (the DB default) if nothing matches.
    /// </summary>
    public string Categorize(string? name, string? merchantName = null)
    {
        var text = $"{name} {merchantName}".Trim();
        if (string.IsNullOrWhiteSpace(text)) return "misc";

        foreach (var (pattern, category) in Rules)
        {
            if (pattern.IsMatch(text)) return category;
        }

        return "misc";
    }
}
