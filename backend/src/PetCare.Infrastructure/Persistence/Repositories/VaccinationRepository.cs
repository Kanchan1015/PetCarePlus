using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.MedicalRecords;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Persistence.Repositories;

public class VaccinationRepository : IVaccinationRepository
{
    private readonly PetCareDbContext _context;

    public VaccinationRepository(PetCareDbContext context)
    {
        _context = context;
    }

    public async Task<Vaccination?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vaccinations
            .Include(v => v.Pet)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Vaccination>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        return await _context.Vaccinations
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.VaccinationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vaccination>> GetOverdueVaccinationsAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.Vaccinations
            .Where(v => v.PetId == petId && 
                       v.NextDueDate.HasValue && 
                       v.NextDueDate.Value < today)
            .OrderBy(v => v.NextDueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vaccination>> GetUpcomingVaccinationsAsync(Guid petId, DateTime dateThreshold, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.Vaccinations
            .Where(v => v.PetId == petId && 
                       v.NextDueDate.HasValue && 
                       v.NextDueDate.Value >= today &&
                       v.NextDueDate.Value <= dateThreshold)
            .OrderBy(v => v.NextDueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Vaccination> AddAsync(Vaccination vaccination, CancellationToken cancellationToken = default)
    {
        _context.Vaccinations.Add(vaccination);
        await _context.SaveChangesAsync(cancellationToken);
        return vaccination;
    }

    public async Task<Vaccination> UpdateAsync(Vaccination vaccination, CancellationToken cancellationToken = default)
    {
        vaccination.UpdatedAt = DateTime.UtcNow;
        
        // Update status based on due date
        if (vaccination.NextDueDate.HasValue)
        {
            var today = DateTime.Today;
            if (vaccination.NextDueDate.Value < today)
            {
                vaccination.Status = VaccinationStatus.Overdue;
            }
            else if (vaccination.NextDueDate.Value <= today.AddDays(30))
            {
                vaccination.Status = VaccinationStatus.Upcoming;
            }
            else
            {
                vaccination.Status = VaccinationStatus.Current;
            }
        }
        
        _context.Vaccinations.Update(vaccination);
        await _context.SaveChangesAsync(cancellationToken);
        return vaccination;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vaccination = await _context.Vaccinations.FindAsync(new object[] { id }, cancellationToken);
        if (vaccination != null)
        {
            _context.Vaccinations.Remove(vaccination);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vaccinations.AnyAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<bool> CanUserAccessVaccinationAsync(Guid vaccinationId, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Vaccinations
            .Join(_context.Pets, v => v.PetId, p => p.Id, (v, p) => new { Vaccination = v, Pet = p })
            .AnyAsync(vp => vp.Vaccination.Id == vaccinationId && 
                           (vp.Pet.OwnerUserId == userId || vp.Vaccination.VetUserId == userId), cancellationToken);
    }
}