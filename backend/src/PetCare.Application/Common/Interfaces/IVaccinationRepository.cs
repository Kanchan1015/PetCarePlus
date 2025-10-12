using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.Common.Interfaces;

public interface IVaccinationRepository
{
    Task<Vaccination?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vaccination>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vaccination>> GetOverdueVaccinationsAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vaccination>> GetUpcomingVaccinationsAsync(Guid petId, DateTime dateThreshold, CancellationToken cancellationToken = default);
    Task<Vaccination> AddAsync(Vaccination vaccination, CancellationToken cancellationToken = default);
    Task<Vaccination> UpdateAsync(Vaccination vaccination, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CanUserAccessVaccinationAsync(Guid vaccinationId, string userId, CancellationToken cancellationToken = default);
}