// INotificationService.cs
// Purpose: Service interface for creating and managing notifications triggered by appointment events
// Methods: Create notifications for different appointment events, send to specific users

using System.Threading.Tasks;
using PetCare.Domain.Appointments;

namespace PetCare.Application.Common.Interfaces;

public interface INotificationService
{
    // Sprint Feature: Create notifications for appointment status changes
    Task NotifyAppointmentApprovedAsync(Appointment appointment, string? vetName = null);
    Task NotifyAppointmentCancelledAsync(Appointment appointment, string reason);
    Task NotifyVetAssignedAsync(Appointment appointment, string vetUserId);
    
    // General notification creation
    Task CreateNotificationAsync(string userId, string title, string message, object? data = null);
}
