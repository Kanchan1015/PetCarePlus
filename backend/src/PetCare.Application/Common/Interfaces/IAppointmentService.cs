// IAppointmentService.cs
// Purpose: Application service interface following Single Responsibility Principle
// Separates business logic from controller concerns

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCare.Application.Appointments.DTOs;
using PetCare.Domain.Appointments;

namespace PetCare.Application.Common.Interfaces;

public interface IAppointmentService
{
    // Owner operations
    Task<AppointmentDto> CreateAppointmentAsync(string ownerId, CreateAppointmentRequest request);
    Task<IEnumerable<AppointmentDto>> GetOwnerAppointmentsAsync(string ownerId);
    
    // Admin operations  
    Task<AppointmentDto> ApproveAppointmentAsync(Guid appointmentId, string adminId, string? vetId = null, string? notes = null);
    Task<AppointmentDto> CancelAppointmentAsync(Guid appointmentId, string adminId, string reason);
    Task<IEnumerable<AppointmentDto>> GetPendingAppointmentsAsync();
    Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
    
    // Vet operations
    Task<IEnumerable<AppointmentDto>> GetVetAppointmentsAsync(string vetId);
    
    // Common operations
    Task<AppointmentDto?> GetAppointmentAsync(Guid appointmentId);
}
