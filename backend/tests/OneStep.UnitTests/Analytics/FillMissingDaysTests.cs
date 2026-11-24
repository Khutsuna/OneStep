using OneStep.Application.Features.Analytics;

namespace OneStep.UnitTests.Analytics;

public class FillMissingDaysTests
{
    private static readonly DateOnly FirstDay = new(2026, 11, 1);

    [Fact]
    public void Every_day_of_the_period_is_returned_in_order()
    {
        var result = AnalyticsService.FillMissingDays([], FirstDay, 7);

        Assert.Equal(7, result.Count);
        Assert.Equal(FirstDay, result[0].Day);
        Assert.Equal(new DateOnly(2026, 11, 7), result[^1].Day);
    }

    [Fact]
    public void Days_without_sales_are_zero_and_days_with_sales_keep_their_numbers()
    {
        var rows = new[]
        {
            new DailySalesRow { Day = new DateTime(2026, 11, 3), OrderCount = 2, Revenue = 1500.50m },
        };

        var result = AnalyticsService.FillMissingDays(rows, FirstDay, 5);

        Assert.Equal(0m, result[1].Revenue);                 // 2 Nov: no sales
        Assert.Equal(0, result[1].OrderCount);
        Assert.Equal(1500.50m, result[2].Revenue);           // 3 Nov
        Assert.Equal(2, result[2].OrderCount);
    }
}
