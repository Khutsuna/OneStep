using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneStep.Application.Common;
using OneStep.Application.Features.Addresses;

namespace OneStep.Api.Controllers;

/// <summary>The logged-in customer's address book.</summary>
[ApiController]
[Route("api/account/addresses")]
[Authorize(Policy = Policies.CustomerOnly)]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>Lists my addresses, default first.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AddressDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<AddressDto>> GetAll(CancellationToken cancellationToken)
    {
        return await _addressService.GetAllAsync(cancellationToken);
    }

    /// <summary>Adds an address. The first one becomes the default.</summary>
    [HttpPost]
    [ProducesResponseType<AddressDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddressDto>> Create(AddressRequest request, CancellationToken cancellationToken)
    {
        var address = await _addressService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), address);
    }

    /// <summary>Changes an address.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<AddressDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<AddressDto> Update(int id, AddressRequest request, CancellationToken cancellationToken)
    {
        return await _addressService.UpdateAsync(id, request, cancellationToken);
    }

    /// <summary>Makes an address the default one.</summary>
    [HttpPut("{id:int}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefault(int id, CancellationToken cancellationToken)
    {
        await _addressService.SetDefaultAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes an address. Past orders keep their own copy.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _addressService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
