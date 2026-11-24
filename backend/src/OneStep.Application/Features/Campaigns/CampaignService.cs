using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Campaigns;

/// <summary>Marketing campaigns for the admin panel: list them, and switch them on or off.</summary>
public class CampaignService : ICampaignService
{
    private readonly IAppDbContext _db;

    public CampaignService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CampaignDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var campaigns = await _db.Campaigns
            .AsNoTracking()
            .Include(c => c.Products)
            .Include(c => c.Categories).ThenInclude(cc => cc.Category)
            .OrderByDescending(c => c.StartsAt)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        return campaigns.Select(c => ToDto(c, now)).ToList();
    }

    public async Task<CampaignDto> SetEnabledAsync(int id, bool isEnabled, CancellationToken cancellationToken)
    {
        var campaign = await _db.Campaigns
            .Include(c => c.Products)
            .Include(c => c.Categories).ThenInclude(cc => cc.Category)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Campaign {id} was not found.");

        campaign.IsEnabled = isEnabled;
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(campaign, DateTime.UtcNow);
    }

    private static CampaignDto ToDto(Campaign c, DateTime now) => new(
        c.Id,
        c.Name,
        c.Description,
        c.DiscountType,
        c.DiscountValue,
        c.AppliesToAllProducts,
        c.StartsAt,
        c.EndsAt,
        c.IsEnabled,
        CampaignStates.Of(c, now),
        c.Products.Count,
        c.Categories.Select(cc => cc.Category.Name).OrderBy(n => n).ToList());
}
