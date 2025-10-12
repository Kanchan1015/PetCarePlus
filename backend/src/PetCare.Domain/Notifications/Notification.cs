// Notification.cs
// Purpose: Domain entity for in-app notifications triggered by appointment status changes
// Features: Type-based notifications, read/unread state, JSON data payload for context
// Foreign Key: UserId (string) to AspNetUsers

using System;

namespace PetCare.Domain.Notifications;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // User who receives the notification
    public string UserId { get; set; } = default!; // FK to AspNetUsers

    // Notification content
    public NotificationType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    
    // JSON data for additional context (appointment details, etc.)
    public string? Data { get; set; }

    // Read state tracking
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Helper methods
    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public bool IsRecent => CreatedAt > DateTime.UtcNow.AddDays(-7); // Last 7 days
}
