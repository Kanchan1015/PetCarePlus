using PetCare.Domain.Pets;

namespace PetCare.Application.Common.Interfaces;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pet>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pet>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<Pet> AddAsync(Pet pet, CancellationToken cancellationToken = default);
    Task<Pet> UpdateAsync(Pet pet, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsOwnerAsync(Guid petId, string userId, CancellationToken cancellationToken = default);
    Task<int> GetPetCountByOwnerAsync(string ownerId, CancellationToken cancellationToken = default);
}