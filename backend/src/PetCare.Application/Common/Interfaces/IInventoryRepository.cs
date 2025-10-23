using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetCare.Domain.Inventory;

namespace PetCare.Application.Common.Interfaces
{
    public interface IInventoryRepository
    {
        Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<InventoryItem> AddAsync(InventoryItem item, CancellationToken cancellationToken = default);
        Task<InventoryItem> UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
