namespace OneStep.Application.Features.Reviews;

/// <summary>
/// Who may write a review. The same rule decides whether the product page shows the form
/// AND whether the API accepts the review, so the two can never disagree. Unit tested.
/// </summary>
public static class ReviewRules
{
    public const string NotReceivedMessage = "You can review a product once it has been delivered to you.";
    public const string AlreadyReviewedMessage = "You have already reviewed this product.";

    /// <summary>Returns null when the customer may write a review, otherwise the reason they can't.</summary>
    public static string? WhyCannotReview(bool hasReceivedProduct, bool hasAlreadyReviewed)
    {
        if (hasAlreadyReviewed) return AlreadyReviewedMessage;
        if (!hasReceivedProduct) return NotReceivedMessage;
        return null;
    }

    /// <summary>"Nino Beridze" -> "Nino B."</summary>
    public static string DisplayName(string firstName, string lastName)
    {
        var first = firstName.Trim();
        var last = lastName.Trim();
        return last.Length == 0 ? first : $"{first} {char.ToUpperInvariant(last[0])}.";
    }
}
