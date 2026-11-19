namespace OneStep.Application.Features.Attributes;

public interface IAttributeService
{
    Task<IReadOnlyList<AttributeDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AttributeDto> CreateAsync(AttributeRequest request, CancellationToken cancellationToken);
}
