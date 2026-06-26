using System.Text.RegularExpressions;

namespace CampCost.Core.Services;

/// <summary>
/// Converts a merchant name string into a human-readable expense category.
/// Pure function — no I/O, no dependencies. Directly ports the JS categorize() regex logic.
///
/// HOW IT WORKS:
/// Each rule is a (regex pattern, category label) pair. We test them in order and return
/// the first match. If nothing matches we return "Other". The regex uses case-insensitive
/// matching (RegexOptions.IgnoreCase).
/// </summary>
public class TransactionCategorizationService
{
    private static readonly (Regex Pattern, string Category)[] Rules =
    {
        (new Regex(@"camp(ground|site|ing)|rv\s?park|koa|hipcamp|the dyrt", RegexOptions.IgnoreCase), "Campsite"),
        (new Regex(@"walmart|target|costco|sam['\s]?s club|grocery|safeway|kroger|whole foods|trader joe|aldi|publix|sprouts|market|food lion|meijer|heb", RegexOptions.IgnoreCase), "Groceries"),
        (new Regex(@"shell|chevron|exxon|bp|mobil|arco|circle k|wawa|pilot|love['\s]?s|casey|speedway|valero|marathon|sunoco|fuel|gas station|kwik trip", RegexOptions.IgnoreCase), "Gas"),
        (new Regex(@"mcdonald|burger king|wendy['\s]?s|taco bell|chick.fil|subway|chipotle|domino|pizza|kfc|dunkin|starbucks|restaurant|diner|grill|cafe|bar |tavern|brewery|eatery|kitchen|bistro|sushi|steakhouse|ihop|waffle house|cracker barrel|olive garden|applebee|chili['\s]?s", RegexOptions.IgnoreCase), "Dining"),
        (new Regex(@"rei|bass pro|cabela['\s]?s|dick['\s]?s sporting|academy sports|patagonia|columbia|north face|osprey|kelty|marmot|yeti|outdoor|camp chef|jetboil", RegexOptions.IgnoreCase), "Gear"),
        (new Regex(@"amazon|ebay|etsy|shopify|bestbuy|best buy|apple|home depot|lowe['\s]?s|menard", RegexOptions.IgnoreCase), "Shopping"),
        (new Regex(@"netflix|spotify|hulu|disney|hbo|youtube|apple tv|xbox|playstation|steam|gaming|twitch", RegexOptions.IgnoreCase), "Entertainment"),
        (new Regex(@"hotel|motel|airbnb|vrbo|hilton|marriott|hyatt|holiday inn|best western|days inn|super 8|comfort inn|embassy suites", RegexOptions.IgnoreCase), "Lodging"),
        (new Regex(@"uber|lyft|taxi|transit|mta|bart|metro|amtrak|greyhound|spirit|frontier|southwest|delta|united|american airlines|jetblue|alaska air|airport|parking", RegexOptions.IgnoreCase), "Travel"),
        (new Regex(@"cvs|walgreen|rite aid|pharmacy|urgent care|hospital|clinic|doctor|dentist|optometrist|health|medical|insurance", RegexOptions.IgnoreCase), "Health"),
        (new Regex(@"at&t|verizon|t.mobile|sprint|comcast|xfinity|spectrum|internet|phone|wireless", RegexOptions.IgnoreCase), "Utilities"),
        (new Regex(@"atm|withdrawal|transfer|zelle|venmo|paypal|cashapp|cash app", RegexOptions.IgnoreCase), "Transfer"),
    };

    /// <summary>
    /// Returns the category for a given merchant name.
    /// Never throws — returns "Other" for any unrecognized input.
    /// </summary>
    public string Categorize(string merchantName)
    {
        if (string.IsNullOrWhiteSpace(merchantName))
            return "Other";

        foreach (var (pattern, category) in Rules)
        {
            if (pattern.IsMatch(merchantName))
                return category;
        }

        return "Other";
    }
}
