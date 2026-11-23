using Microsoft.EntityFrameworkCore;
using OneStep.Application.Abstractions;
using OneStep.Application.Common.Exceptions;
using OneStep.Domain.Entities;

namespace OneStep.Application.Features.Addresses;

/// <summary>
/// The customer's address book. Exactly one address is the default (the database enforces
/// "at most one" with a filtered unique index; this service makes sure there is always one).
/// </summary>
public class AddressService : IAddressService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AddressService(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private int UserId => _currentUser.UserId ?? throw new UnauthorizedException("Please log in.");

    public async Task<IReadOnlyList<AddressDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var addresses = await _db.Addresses.AsNoTracking()
            .Where(a => a.UserId == UserId)
            .OrderByDescending(a => a.IsDefault).ThenBy(a => a.Label)
            .ToListAsync(cancellationToken);
        return addresses.Select(ToDto).ToList();
    }

    public async Task<AddressDto> CreateAsync(AddressRequest request, CancellationToken cancellationToken)
    {
        var hasAddresses = await _db.Addresses.AnyAsync(a => a.UserId == UserId, cancellationToken);

        var address = new Address { UserId = UserId };
        Apply(address, request);
        address.IsDefault = false;          // set below, safely
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync(cancellationToken);

        // The first address always becomes the default.
        if (!hasAddresses || request.IsDefault)
        {
            await SetDefaultAsync(address.Id, cancellationToken);
            address.IsDefault = true;
        }

        return ToDto(address);
    }

    public async Task<AddressDto> UpdateAsync(int id, AddressRequest request, CancellationToken cancellationToken)
    {
        var address = await FindOwnAsync(id, cancellationToken);
        var wasDefault = address.IsDefault;
        Apply(address, request);
        address.IsDefault = wasDefault;
        await _db.SaveChangesAsync(cancellationToken);

        if (request.IsDefault && !wasDefault)
        {
            await SetDefaultAsync(id, cancellationToken);
            address.IsDefault = true;
        }

        return ToDto(address);
    }

    public async Task SetDefaultAsync(int id, CancellationToken cancellationToken)
    {
        var addresses = await _db.Addresses.Where(a => a.UserId == UserId).ToListAsync(cancellationToken);
        var target = addresses.FirstOrDefault(a => a.Id == id)
            ?? throw new NotFoundException("This address isn't in your address book.");

        // Two saves: clear the old default first, then set the new one,
        // so the "one default per user" index is never broken in between.
        foreach (var address in addresses.Where(a => a.IsDefault && a.Id != id))
        {
            address.IsDefault = false;
        }
        await _db.SaveChangesAsync(cancellationToken);

        target.IsDefault = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var address = await FindOwnAsync(id, cancellationToken);
        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync(cancellationToken);

        // Orders keep their own copy of the address, so deleting it here never changes an order.
        if (address.IsDefault)
        {
            var next = await _db.Addresses.Where(a => a.UserId == UserId).OrderBy(a => a.Id).FirstOrDefaultAsync(cancellationToken);
            if (next is not null)
            {
                next.IsDefault = true;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task<Address> FindOwnAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId, cancellationToken)
            ?? throw new NotFoundException("This address isn't in your address book.");
    }

    private static void Apply(Address address, AddressRequest request)
    {
        address.Label = request.Label.Trim();
        address.RecipientName = request.RecipientName.Trim();
        address.Phone = request.Phone.Trim();
        address.Line1 = request.Line1.Trim();
        address.Line2 = string.IsNullOrWhiteSpace(request.Line2) ? null : request.Line2.Trim();
        address.City = request.City.Trim();
        address.Region = string.IsNullOrWhiteSpace(request.Region) ? null : request.Region.Trim();
        address.PostalCode = string.IsNullOrWhiteSpace(request.PostalCode) ? null : request.PostalCode.Trim();
        address.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
    }

    private static AddressDto ToDto(Address a)
    {
        return new AddressDto(a.Id, a.Label, a.RecipientName, a.Phone, a.Line1, a.Line2,
            a.City, a.Region, a.PostalCode, a.CountryCode, a.IsDefault);
    }
}
