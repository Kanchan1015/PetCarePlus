// IAppointmentRepository.cs
// Purpose: Repository interface for appointment data access with role-specific methods
// Methods: CRUD operations, owner-specific queries, admin queries, vet queries

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCare.Domain.Appointments;

namespace PetCare.Application.Common.Interfaces;

public interface IAppointmentRepository
{
    // Basic CRUD operations
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task DeleteAsync(Guid id);

    // Owner-specific queries
    Task<IEnumerable<Appointment>> GetByOwnerUserIdAsync(string ownerUserId);
    Task<IEnumerable<Appointment>> GetByOwnerUserIdAndStatusAsync(string ownerUserId, AppointmentStatus status);

    // Admin queries for management
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status);
    Task<IEnumerable<Appointment>> GetPendingAsync();

    // Vet-specific queries
    Task<IEnumerable<Appointment>> GetByVetUserIdAsync(string vetUserId);

    // Scheduling conflict detection
    Task<bool> HasConflictAsync(DateTime requestedDateTime, string? vetUserId = null, Guid? excludeAppointmentId = null);

    // Statistics for dashboard
    Task<int> GetCountByStatusAsync(AppointmentStatus status);
    Task<int> GetTodayAppointmentsCountAsync();
}
