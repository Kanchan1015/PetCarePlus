// INotificationFactory.cs  
// Purpose: Factory pattern interface following Open/Closed Principle
// Allows adding new notification types without modifying existing code

using PetCare.Domain.Appointments;
using PetCare.Domain.Notifications;

namespace PetCare.Application.Common.Interfaces;

public interface INotificationFactory
{
    Notification CreateAppointmentApprovedNotification(Appointment appointment, string ownerId, string? vetName = null);
    Notification CreateAppointmentCancelledNotification(Appointment appointment, string ownerId, string reason);
    Notification CreateVetAssignedNotification(Appointment appointment, string vetId);
    Notification CreateCustomNotification(string userId, string title, string message, NotificationType type, object? data = null);
}
