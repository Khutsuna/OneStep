using OneStep.Application.Features.Campaigns;
using OneStep.Domain.Entities;

namespace OneStep.UnitTests.Campaigns;

public class CampaignStateTests
{
    private static readonly DateTime Now = new(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);

    private static Campaign MakeCampaign(int startsInDays, int endsInDays, bool enabled = true) => new()
    {
        Name = "Test",
        StartsAt = Now.AddDays(startsInDays),
        EndsAt = Now.AddDays(endsInDays),
        IsEnabled = enabled
    };

    [Fact]
    public void A_campaign_that_has_not_started_is_scheduled()
    {
        Assert.Equal(CampaignState.Scheduled, CampaignStates.Of(MakeCampaign(1, 5), Now));
    }

    [Fact]
    public void A_campaign_between_its_dates_is_active()
    {
        Assert.Equal(CampaignState.Active, CampaignStates.Of(MakeCampaign(-1, 5), Now));
    }

    [Fact]
    public void A_campaign_past_its_end_date_has_ended()
    {
        Assert.Equal(CampaignState.Ended, CampaignStates.Of(MakeCampaign(-10, -1), Now));
    }

    [Fact]
    public void A_switched_off_campaign_is_disabled_even_inside_its_dates()
    {
        Assert.Equal(CampaignState.Disabled, CampaignStates.Of(MakeCampaign(-1, 5, enabled: false), Now));
    }
}
