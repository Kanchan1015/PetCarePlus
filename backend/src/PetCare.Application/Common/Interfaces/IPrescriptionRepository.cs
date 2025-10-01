using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.Common.Interfaces;

public interface IPrescriptionRepository
{
    Task<Prescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prescription>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prescription>> GetActivePrescriptionsAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<Prescription> AddAsync(Prescription prescription, CancellationToken cancellationToken = default);
    Task<Prescription> UpdateAsync(Prescription prescription, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CanUserAccessPrescriptionAsync(Guid prescriptionId, string userId, CancellationToken cancellationToken = default);
}