namespace OneStep.Application.Features.Addresses;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AddressDto> CreateAsync(AddressRequest request, CancellationToken cancellationToken);
    Task<AddressDto> UpdateAsync(int id, AddressRequest request, CancellationToken cancellationToken);
    Task SetDefaultAsync(int id, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
