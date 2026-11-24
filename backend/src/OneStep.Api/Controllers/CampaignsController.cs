using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Campaigns;

namespace OneStep.Api.Controllers;

/// <summary>Marketing campaigns (discounts with a start and end date). Staff only.</summary>
[ApiController]
[Route("api/campaigns")]
[Authorize(Policy = Policies.StaffOnly)]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaigns;

    public CampaignsController(ICampaignService campaigns)
    {
        _campaigns = campaigns;
    }

    /// <summary>All campaigns, newest first, with their current state (Scheduled, Active, Ended, Disabled).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CampaignDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<CampaignDto>> GetAll(CancellationToken cancellationToken)
    {
        return await _campaigns.GetAllAsync(cancellationToken);
    }

    /// <summary>Switches a campaign on or off. Prices update immediately.</summary>
    /// <response code="404">No campaign has this id.</response>
    [HttpPut("{id:int}/enabled")]
    [ProducesResponseType<CampaignDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<CampaignDto> SetEnabled(int id, SetCampaignEnabledRequest request, CancellationToken cancellationToken)
    {
        return await _campaigns.SetEnabledAsync(id, request.IsEnabled!.Value, cancellationToken);
    }
}
