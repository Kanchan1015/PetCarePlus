// NotificationDto.cs
// Purpose: Response DTO for notification data with type display names

using System;
using PetCare.Domain.Notifications;

namespace PetCare.Application.Notifications.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public NotificationType Type { get; set; }
    public string TypeDisplayName { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? Data { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRecent { get; set; }
}
