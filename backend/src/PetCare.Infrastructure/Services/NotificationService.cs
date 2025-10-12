// NotificationService.cs
// Purpose: Service implementation for creating appointment-related notifications
// Features: Type-specific notifications, JSON data serialization, automatic triggering

using System;
using System.Text.Json;
using System.Threading.Tasks;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Appointments;
using PetCare.Domain.Notifications;

namespace PetCare.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    // Sprint Feature: Owner notification when appointment is approved
    public async Task NotifyAppointmentApprovedAsync(Appointment appointment, string? vetName = null)
    {
        var title = "Appointment Approved ✅";
        var message = $"Your appointment request for {appointment.Pet.Name} on {appointment.RequestedDateTime:MMM dd, yyyy 'at' h:mm tt} has been approved.";
        
        if (!string.IsNullOrEmpty(vetName))
        {
            message += $" Assigned veterinarian: {vetName}";
        }

        var data = new
        {
            appointmentId = appointment.Id,
            petId = appointment.PetId,
            petName = appointment.Pet.Name,
            requestedDateTime = appointment.RequestedDateTime,
            vetUserId = appointment.VetUserId,
            vetName = vetName
        };

        var notification = new Notification
        {
            UserId = appointment.OwnerUserId,
            Type = NotificationType.AppointmentApproved,
            Title = title,
            Message = message,
            Data = JsonSerializer.Serialize(data)
        };

        await _notificationRepository.CreateAsync(notification);
    }

    // Sprint Feature: Owner notification when appointment is cancelled
    public async Task NotifyAppointmentCancelledAsync(Appointment appointment, string reason)
    {
        var title = "Appointment Cancelled ❌";
        var message = $"Your appointment request for {appointment.Pet.Name} on {appointment.RequestedDateTime:MMM dd, yyyy 'at' h:mm tt} has been cancelled.";
        
        if (!string.IsNullOrEmpty(reason))
        {
            message += $" Reason: {reason}";
        }

        var data = new
        {
            appointmentId = appointment.Id,
            petId = appointment.PetId,
            petName = appointment.Pet.Name,
            requestedDateTime = appointment.RequestedDateTime,
            reason = reason
        };

        var notification = new Notification
        {
            UserId = appointment.OwnerUserId,
            Type = NotificationType.AppointmentCancelled,
            Title = title,
            Message = message,
            Data = JsonSerializer.Serialize(data)
        };

        await _notificationRepository.CreateAsync(notification);
    }

        // Sprint Feature: Notify vet when assigned to appointment
    public async Task NotifyVetAssignedAsync(Appointment appointment, string vetUserId)
    {
        var data = JsonSerializer.Serialize(new
        {
            AppointmentId = appointment.Id,
            PetName = appointment.Pet?.Name ?? "Unknown Pet",
            OwnerName = "Pet Owner", // Could be passed as parameter if needed
            RequestedDateTime = appointment.RequestedDateTime,
            ReasonForVisit = appointment.ReasonForVisit
        });

        var notification = new Notification
        {
            UserId = vetUserId,
            Type = NotificationType.AppointmentAssigned,
            Title = "New Appointment Assigned",
            Message = $"You have been assigned to an appointment for {appointment.Pet?.Name ?? "a pet"} on {appointment.RequestedDateTime:MMM dd, yyyy 'at' h:mm tt}.",
            Data = data
        };

        await _notificationRepository.CreateAsync(notification);
    }

    // General notification creation for future use
    public async Task CreateNotificationAsync(string userId, string title, string message, object? data = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = NotificationType.General,
            Title = title,
            Message = message,
            Data = data != null ? JsonSerializer.Serialize(data) : null
        };

        await _notificationRepository.CreateAsync(notification);
    }
}
