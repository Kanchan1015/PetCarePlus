using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Pets;

namespace PetCare.Infrastructure.Persistence.Repositories;

public class PetRepository : IPetRepository
{
    private readonly PetCareDbContext _context;

    public PetRepository(PetCareDbContext context)
    {
        _context = context;
    }

    public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Pet>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Pet>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .Where(p => p.OwnerUserId == ownerId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pet> AddAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync(cancellationToken);
        return pet;
    }

    public async Task<Pet> UpdateAsync(Pet pet, CancellationToken cancellationToken = default)
    {
        _context.Pets.Update(pet);
        await _context.SaveChangesAsync(cancellationToken);
        return pet;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pet = await _context.Pets.FindAsync(new object[] { id }, cancellationToken);
        if (pet != null)
        {
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> IsOwnerAsync(Guid petId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .AnyAsync(p => p.Id == petId && p.OwnerUserId == userId, cancellationToken);
    }

    public async Task<int> GetPetCountByOwnerAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Pets
            .CountAsync(p => p.OwnerUserId == ownerId && p.IsActive, cancellationToken);
    }
}