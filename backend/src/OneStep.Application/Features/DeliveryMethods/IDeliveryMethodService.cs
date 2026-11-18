namespace OneStep.Application.Features.DeliveryMethods;

public interface IDeliveryMethodService
{
    Task<IReadOnlyList<DeliveryMethodDto>> GetActiveAsync(CancellationToken cancellationToken);
    Task<DeliveryMethodDto> GetByIdAsync(int id, CancellationToken cancellationToken);
}
