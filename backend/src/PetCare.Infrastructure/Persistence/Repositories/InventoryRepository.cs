using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Inventory;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly PetCareDbContext _context;

    public InventoryRepository(PetCareDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItem> AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<InventoryItem> UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.InventoryItems.FindAsync(new object[] { id }, cancellationToken);
        if (item != null)
        {
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryItems.AnyAsync(i => i.Id == id, cancellationToken);
    }
}
