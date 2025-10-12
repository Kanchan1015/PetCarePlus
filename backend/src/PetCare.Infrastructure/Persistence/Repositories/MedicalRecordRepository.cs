using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.MedicalRecords;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Persistence.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly PetCareDbContext _context;

    public MedicalRecordRepository(PetCareDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalRecords
            .Include(m => m.Pet)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MedicalRecord>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalRecords
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.RecordDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MedicalRecord>> GetByPetIdAndTypeAsync(Guid petId, MedicalRecordType recordType, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalRecords
            .Where(m => m.PetId == petId && m.RecordType == recordType)
            .OrderByDescending(m => m.RecordDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<MedicalRecord> AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default)
    {
        _context.MedicalRecords.Add(medicalRecord);
        await _context.SaveChangesAsync(cancellationToken);
        return medicalRecord;
    }

    public async Task<MedicalRecord> UpdateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default)
    {
        medicalRecord.UpdatedAt = DateTime.UtcNow;
        _context.MedicalRecords.Update(medicalRecord);
        await _context.SaveChangesAsync(cancellationToken);
        return medicalRecord;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var medicalRecord = await _context.MedicalRecords.FindAsync(new object[] { id }, cancellationToken);
        if (medicalRecord != null)
        {
            _context.MedicalRecords.Remove(medicalRecord);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalRecords.AnyAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<bool> CanUserAccessMedicalRecordAsync(Guid medicalRecordId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalRecords
            .Join(_context.Pets, m => m.PetId, p => p.Id, (m, p) => new { MedicalRecord = m, Pet = p })
            .AnyAsync(mp => mp.MedicalRecord.Id == medicalRecordId && 
                           (mp.Pet.OwnerUserId == userId || mp.MedicalRecord.VetUserId == userId), cancellationToken);
    }
}