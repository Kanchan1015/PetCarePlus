using PetCare.Domain.MedicalRecords;

namespace PetCare.Application.Common.Interfaces;

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalRecord>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalRecord>> GetByPetIdAndTypeAsync(Guid petId, MedicalRecordType recordType, CancellationToken cancellationToken = default);
    Task<MedicalRecord> AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default);
    Task<MedicalRecord> UpdateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CanUserAccessMedicalRecordAsync(Guid medicalRecordId, string userId, CancellationToken cancellationToken = default);
}