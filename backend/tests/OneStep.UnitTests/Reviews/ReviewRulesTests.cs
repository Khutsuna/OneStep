using OneStep.Application.Features.Reviews;

namespace OneStep.UnitTests.Reviews;

public class ReviewRulesTests
{
    [Fact]
    public void A_customer_who_received_the_product_may_review_it()
    {
        Assert.Null(ReviewRules.WhyCannotReview(hasReceivedProduct: true, hasAlreadyReviewed: false));
    }

    [Fact]
    public void A_customer_who_has_not_received_the_product_may_not_review_it()
    {
        Assert.Equal(ReviewRules.NotReceivedMessage,
            ReviewRules.WhyCannotReview(hasReceivedProduct: false, hasAlreadyReviewed: false));
    }

    [Fact]
    public void A_customer_can_review_a_product_only_once()
    {
        Assert.Equal(ReviewRules.AlreadyReviewedMessage,
            ReviewRules.WhyCannotReview(hasReceivedProduct: true, hasAlreadyReviewed: true));
    }

    [Theory]
    [InlineData("Nino", "Beridze", "Nino B.")]
    [InlineData(" luka ", "gelashvili", "luka G.")]
    [InlineData("Ana", "", "Ana")]
    public void Authors_are_shown_with_their_last_initial_only(string first, string last, string expected)
    {
        Assert.Equal(expected, ReviewRules.DisplayName(first, last));
    }
}
