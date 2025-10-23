using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Inventory;

namespace PetCare.Application.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InventoryItemDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(i => new InventoryItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Quantity = i.Quantity,
                Category = i.Category,
                Supplier = i.Supplier,
                ExpiryDate = i.ExpiryDate,
                Description = i.Description,
                PhotoUrl = i.PhotoUrl // <-- Ensure PhotoUrl is mapped
            });
        }

        public async Task<InventoryItemDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return null;
            return new InventoryItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                Category = item.Category,
                Supplier = item.Supplier,
                ExpiryDate = item.ExpiryDate,
                Description = item.Description,
                PhotoUrl = item.PhotoUrl // <-- Ensure PhotoUrl is mapped
            };
        }

        public async Task<InventoryItemDto> CreateAsync(CreateInventoryDto dto)
        {
            // Check for duplicate by name (case-insensitive, trimmed)
            var normalizedName = dto.Name.Trim().ToLower();
            var exists = await _repository.GetAllAsync();
            if (exists.Any(i => i.Name.Trim().ToLower() == normalizedName))
            {
                throw new InvalidOperationException("An item with this name already exists.");
            }
            var item = new InventoryItem
            {
                Name = dto.Name,
                Quantity = dto.Quantity,
                Category = dto.Category,
                Supplier = dto.Supplier,
                ExpiryDate = dto.ExpiryDate,
                Description = dto.Description,
                PhotoUrl = dto.PhotoUrl // <-- Ensure PhotoUrl is set
            };
            var created = await _repository.AddAsync(item);
            return new InventoryItemDto
            {
                Id = created.Id,
                Name = created.Name,
                Quantity = created.Quantity,
                Category = created.Category,
                Supplier = created.Supplier,
                ExpiryDate = created.ExpiryDate,
                Description = created.Description,
                PhotoUrl = created.PhotoUrl // <-- Ensure PhotoUrl is mapped
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateInventoryDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return false;
            // Check for duplicate by name (case-insensitive, trimmed), excluding current item
            var normalizedName = dto.Name.Trim().ToLower();
            var exists = await _repository.GetAllAsync();
            if (exists.Any(i => i.Name.Trim().ToLower() == normalizedName && i.Id != id))
            {
                throw new InvalidOperationException("An item with this name already exists.");
            }
            item.Name = dto.Name;
            item.Quantity = dto.Quantity;
            item.Category = dto.Category;
            item.Supplier = dto.Supplier;
            item.ExpiryDate = dto.ExpiryDate;
            item.Description = dto.Description;
            item.PhotoUrl = dto.PhotoUrl; // <-- Ensure PhotoUrl is updated
            await _repository.UpdateAsync(item);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists) return false;
            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
