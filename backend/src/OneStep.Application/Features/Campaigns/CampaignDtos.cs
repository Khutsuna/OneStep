using System.ComponentModel.DataAnnotations;
using OneStep.Domain.Entities;
using OneStep.Domain.Enums;

namespace OneStep.Application.Features.Campaigns;

/// <summary>Where a campaign is in its life, worked out from its dates and switch.</summary>
public enum CampaignState
{
    Scheduled, // starts in the future
    Active,    // running now: its discounts are applied
    Ended,     // its end date has passed
    Disabled   // switched off by staff, whatever the dates
}

public record CampaignDto(
    int Id,
    string Name,
    string? Description,
    DiscountType DiscountType,
    decimal DiscountValue,
    bool AppliesToAllProducts,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsEnabled,
    CampaignState State,
    int ProductCount,
    IReadOnlyList<string> CategoryNames);

public class SetCampaignEnabledRequest
{
    [Required]
    public bool? IsEnabled { get; set; }
}

public static class CampaignStates
{
    /// <summary>The same rule PricingService uses: enabled, and StartsAt &lt;= now &lt; EndsAt.</summary>
    public static CampaignState Of(Campaign campaign, DateTime now)
    {
        if (!campaign.IsEnabled) return CampaignState.Disabled;
        if (now < campaign.StartsAt) return CampaignState.Scheduled;
        if (now >= campaign.EndsAt) return CampaignState.Ended;
        return CampaignState.Active;
    }
}
