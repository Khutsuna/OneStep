namespace OneStep.Application.Features.Campaigns;

public interface ICampaignService
{
    Task<IReadOnlyList<CampaignDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CampaignDto> SetEnabledAsync(int id, bool isEnabled, CancellationToken cancellationToken);
}
