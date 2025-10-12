using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.Common.Interfaces;

public interface ITreatmentRepository
{
    Task<Treatment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Treatment>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Treatment>> GetTreatmentHistoryAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<Treatment> AddAsync(Treatment treatment, CancellationToken cancellationToken = default);
    Task<Treatment> UpdateAsync(Treatment treatment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CanUserAccessTreatmentAsync(Guid treatmentId, string userId, CancellationToken cancellationToken = default);
}