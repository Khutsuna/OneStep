using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Brands;

public class BrandService : IBrandService
{
    private readonly IAppDbContext _db;

    public BrandService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var brands = await _db.Brands
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, b.Name, b.Slug, b.LogoPath, ProductCount = b.Products.Count })
            .ToListAsync(cancellationToken);

        return brands
            .Select(b => new BrandDto(b.Id, b.Name, b.Slug, MediaUrl.From(b.LogoPath), b.ProductCount))
            .ToList();
    }

    public async Task<BrandDto> CreateAsync(BrandRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(name, brandId: null, cancellationToken);

        var brand = new Brand
        {
            Name = name,
            Slug = await SlugHelper.CreateUniqueAsync(name,
                slug => _db.Brands.AnyAsync(b => b.Slug == slug, cancellationToken))
        };

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync(cancellationToken);
        return new BrandDto(brand.Id, brand.Name, brand.Slug, null, 0);
    }

    public async Task<BrandDto> UpdateAsync(int id, BrandRequest request, CancellationToken cancellationToken)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Brand {id} was not found.");

        var name = request.Name.Trim();
        await EnsureNameIsFreeAsync(name, id, cancellationToken);

        if (brand.Name != name)
        {
            brand.Name = name;
            brand.Slug = await SlugHelper.CreateUniqueAsync(name,
                slug => _db.Brands.AnyAsync(b => b.Slug == slug && b.Id != id, cancellationToken));
            await _db.SaveChangesAsync(cancellationToken);
        }

        var productCount = await _db.Products.CountAsync(p => p.BrandId == id, cancellationToken);
        return new BrandDto(brand.Id, brand.Name, brand.Slug, MediaUrl.From(brand.LogoPath), productCount);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Brand {id} was not found.");

        if (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.BrandId == id, cancellationToken))
        {
            throw new ConflictException("This brand still has products. Change their brand first.");
        }

        _db.Brands.Remove(brand);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameIsFreeAsync(string name, int? brandId, CancellationToken cancellationToken)
    {
        if (await _db.Brands.AnyAsync(b => b.Name == name && b.Id != brandId, cancellationToken))
        {
            throw new ConflictException($"A brand named '{name}' already exists.");
        }
    }
}
