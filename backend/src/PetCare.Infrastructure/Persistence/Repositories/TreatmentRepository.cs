using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.MedicalRecords;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Persistence.Repositories;

public class TreatmentRepository : ITreatmentRepository
{
    private readonly PetCareDbContext _context;

    public TreatmentRepository(PetCareDbContext context)
    {
        _context = context;
    }

    public async Task<Treatment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Treatments
            .Include(t => t.Pet)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Treatment>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _context.Treatments
            .Where(t => t.PetId == petId)
            .OrderByDescending(t => t.TreatmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Treatment>> GetTreatmentHistoryAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _context.Treatments
            .Where(t => t.PetId == petId)
            .OrderByDescending(t => t.TreatmentDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Treatment> AddAsync(Treatment treatment, CancellationToken cancellationToken = default)
    {
        _context.Treatments.Add(treatment);
        await _context.SaveChangesAsync(cancellationToken);
        return treatment;
    }

    public async Task<Treatment> UpdateAsync(Treatment treatment, CancellationToken cancellationToken = default)
    {
        treatment.UpdatedAt = DateTime.UtcNow;
        _context.Treatments.Update(treatment);
        await _context.SaveChangesAsync(cancellationToken);
        return treatment;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var treatment = await _context.Treatments.FindAsync(new object[] { id }, cancellationToken);
        if (treatment != null)
        {
            _context.Treatments.Remove(treatment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Treatments.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> CanUserAccessTreatmentAsync(Guid treatmentId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Treatments
            .Join(_context.Pets, t => t.PetId, p => p.Id, (t, p) => new { Treatment = t, Pet = p })
            .AnyAsync(tp => tp.Treatment.Id == treatmentId && 
                           (tp.Pet.OwnerUserId == userId || tp.Treatment.VetUserId == userId), cancellationToken);
    }
}