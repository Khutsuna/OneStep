using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Attributes;

namespace OneStep.Api.Controllers;

/// <summary>Kinds of product characteristics (Color, Storage, RAM...).</summary>
[ApiController]
[Route("api/attributes")]
public class AttributesController : ControllerBase
{
    private readonly IAttributeService _attributeService;

    public AttributesController(IAttributeService attributeService)
    {
        _attributeService = attributeService;
    }

    /// <summary>Lists all attributes.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<AttributeDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<AttributeDto>> GetAll(CancellationToken cancellationToken)
    {
        return await _attributeService.GetAllAsync(cancellationToken);
    }

    /// <summary>Creates an attribute. Staff only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.StaffOnly)]
    [ProducesResponseType<AttributeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AttributeDto>> Create(AttributeRequest request, CancellationToken cancellationToken)
    {
        var attribute = await _attributeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), attribute);
    }
}
