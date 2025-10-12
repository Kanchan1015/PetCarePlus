using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.MedicalRecords;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Persistence.Repositories;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly PetCareDbContext _context;

    public PrescriptionRepository(PetCareDbContext context)
    {
        _context = context;
    }

    public async Task<Prescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .Include(p => p.Pet)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Prescription>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .Where(p => p.PetId == petId)
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prescription>> GetActivePrescriptionsAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .Where(p => p.PetId == petId && p.Status == PrescriptionStatus.Active)
            .OrderByDescending(p => p.PrescribedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Prescription> AddAsync(Prescription prescription, CancellationToken cancellationToken = default)
    {
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync(cancellationToken);
        return prescription;
    }

    public async Task<Prescription> UpdateAsync(Prescription prescription, CancellationToken cancellationToken = default)
    {
        prescription.UpdatedAt = DateTime.UtcNow;
        
        // Update status based on end date
        if (prescription.EndDate.HasValue && prescription.EndDate.Value < DateTime.Today && prescription.Status == PrescriptionStatus.Active)
        {
            prescription.Status = PrescriptionStatus.Completed;
        }
        
        _context.Prescriptions.Update(prescription);
        await _context.SaveChangesAsync(cancellationToken);
        return prescription;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var prescription = await _context.Prescriptions.FindAsync(new object[] { id }, cancellationToken);
        if (prescription != null)
        {
            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> CanUserAccessPrescriptionAsync(Guid prescriptionId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Prescriptions
            .Join(_context.Pets, p => p.PetId, pet => pet.Id, (p, pet) => new { Prescription = p, Pet = pet })
            .AnyAsync(pp => pp.Prescription.Id == prescriptionId && 
                           (pp.Pet.OwnerUserId == userId || pp.Prescription.VetUserId == userId), cancellationToken);
    }
}