namespace OneStep.Application.Features.DeliveryMethods;

/// <summary>
/// What the API returns for a delivery method. A DTO ("data transfer object")
/// is a separate class from the entity, so the API only exposes what it should.
/// </summary>
public record DeliveryMethodDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    decimal? FreeOverAmount,
    byte MinDays,
    byte MaxDays);
