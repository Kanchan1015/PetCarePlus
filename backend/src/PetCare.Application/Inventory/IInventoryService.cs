using PetCare.Application.Inventory;

public interface IInventoryService
{
    Task<IEnumerable<InventoryItemDto>> GetAllAsync();
    Task<InventoryItemDto?> GetByIdAsync(Guid id);
    Task<InventoryItemDto> CreateAsync(CreateInventoryDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateInventoryDto dto);
    Task<bool> DeleteAsync(Guid id);
}
