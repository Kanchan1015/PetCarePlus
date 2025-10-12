// NotificationType.cs
// Purpose: Enum for different types of notifications in the system
// Types: Appointment-related notifications for status changes and vet assignments

namespace PetCare.Domain.Notifications;

public enum NotificationType
{
    General = 0,
    AppointmentApproved = 1,      // Owner receives when admin approves their request
    AppointmentCancelled = 2,     // Owner receives when admin cancels their request
    AppointmentAssigned = 3,      // Vet receives when admin assigns them to an appointment
    AppointmentReminder = 4,      // Future: Reminder notifications
    SystemMessage = 5             // Future: System-wide messages
}
