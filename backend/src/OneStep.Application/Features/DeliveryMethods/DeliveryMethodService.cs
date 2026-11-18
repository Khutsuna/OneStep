using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;

namespace OneStep.Application.Features.DeliveryMethods;

/// <summary>
/// The pattern every feature follows: the controller calls a service,
/// the service queries the database with LINQ and returns DTOs.
/// </summary>
public class DeliveryMethodService : IDeliveryMethodService
{
    private readonly IAppDbContext _db;

    public DeliveryMethodService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DeliveryMethodDto>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await _db.DeliveryMethods
            .AsNoTracking()                       // read-only query: EF doesn't need to track changes
            .Where(d => d.IsActive)
            .OrderBy(d => d.Price)
            .Select(d => new DeliveryMethodDto(
                d.Id, d.Name, d.Description, d.Price, d.FreeOverAmount, d.MinDays, d.MaxDays))
            .ToListAsync(cancellationToken);
    }

    public async Task<DeliveryMethodDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var deliveryMethod = await _db.DeliveryMethods
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DeliveryMethodDto(
                d.Id, d.Name, d.Description, d.Price, d.FreeOverAmount, d.MinDays, d.MaxDays))
            .FirstOrDefaultAsync(cancellationToken);

        // No result -> throw; the global exception handler turns this into a 404 response.
        return deliveryMethod ?? throw new NotFoundException($"Delivery method {id} was not found.");
    }
}
