// AppointmentRepository.cs
// Purpose: EF implementation of appointment repository with optimized queries and includes
// Features: Includes Pet and User data, efficient filtering, conflict detection

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Appointments;
using PetCare.Infrastructure.Persistence;

namespace PetCare.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly PetCareDbContext _context;

    public AppointmentRepository(PetCareDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.UtcNow;
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task DeleteAsync(Guid id)
    {
        var appointment = await GetByIdAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }

    // Owner-specific queries with Pet information
    public async Task<IEnumerable<Appointment>> GetByOwnerUserIdAsync(string ownerUserId)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Where(a => a.OwnerUserId == ownerUserId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByOwnerUserIdAndStatusAsync(string ownerUserId, AppointmentStatus status)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Where(a => a.OwnerUserId == ownerUserId && a.Status == status)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    // Admin queries for management interface
    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Where(a => a.Status == status)
            .OrderBy(a => a.RequestedDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetPendingAsync()
    {
        return await GetByStatusAsync(AppointmentStatus.Pending);
    }

    // Vet-specific queries
    public async Task<IEnumerable<Appointment>> GetByVetUserIdAsync(string vetUserId)
    {
        return await _context.Appointments
            .Include(a => a.Pet)
            .Where(a => a.VetUserId == vetUserId)
            .OrderBy(a => a.RequestedDateTime)
            .ToListAsync();
    }

    // Sprint Feature: Conflict detection for appointment scheduling
    public async Task<bool> HasConflictAsync(DateTime requestedDateTime, string? vetUserId = null, Guid? excludeAppointmentId = null)
    {
        var query = _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Approved);

        // Check for conflicts within 30-minute window
        var startTime = requestedDateTime.AddMinutes(-30);
        var endTime = requestedDateTime.AddMinutes(30);
        
        query = query.Where(a => a.ActualDateTime >= startTime && a.ActualDateTime <= endTime);

        if (!string.IsNullOrEmpty(vetUserId))
        {
            query = query.Where(a => a.VetUserId == vetUserId);
        }

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync();
    }

    // Dashboard statistics
    public async Task<int> GetCountByStatusAsync(AppointmentStatus status)
    {
        return await _context.Appointments
            .CountAsync(a => a.Status == status);
    }

    public async Task<int> GetTodayAppointmentsCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        return await _context.Appointments
            .CountAsync(a => a.RequestedDateTime >= today && a.RequestedDateTime < tomorrow);
    }
}
