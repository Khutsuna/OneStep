using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Features.DeliveryMethods;

namespace OneStep.Api.Controllers;

/// <summary>Delivery options shown at checkout.</summary>
[ApiController]
[Route("api/delivery-methods")]
public class DeliveryMethodsController : ControllerBase
{
    private readonly IDeliveryMethodService _service;

    public DeliveryMethodsController(IDeliveryMethodService service)
    {
        _service = service;
    }

    /// <summary>Lists the active delivery methods, cheapest first.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DeliveryMethodDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<DeliveryMethodDto>> GetAll(CancellationToken cancellationToken)
    {
        return await _service.GetActiveAsync(cancellationToken);
    }

    /// <summary>Gets one delivery method by its id.</summary>
    /// <response code="404">No delivery method has this id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<DeliveryMethodDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<DeliveryMethodDto> GetById(int id, CancellationToken cancellationToken)
    {
        return await _service.GetByIdAsync(id, cancellationToken);
    }
}
