using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Attributes;

/// <summary>The kinds of characteristics products can have, e.g. Color, Storage, RAM.</summary>
public class AttributeService : IAttributeService
{
    private readonly IAppDbContext _db;

    public AttributeService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AttributeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _db.ProductAttributes
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new AttributeDto(a.Id, a.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<AttributeDto> CreateAsync(AttributeRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await _db.ProductAttributes.AnyAsync(a => a.Name == name, cancellationToken))
        {
            throw new ConflictException($"An attribute named '{name}' already exists.");
        }

        var attribute = new ProductAttribute { Name = name };
        _db.ProductAttributes.Add(attribute);
        await _db.SaveChangesAsync(cancellationToken);
        return new AttributeDto(attribute.Id, attribute.Name);
    }
}
