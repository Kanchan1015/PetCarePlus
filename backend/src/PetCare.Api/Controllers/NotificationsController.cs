// NotificationsController.cs
// Purpose: API controller for in-app notification management
// Features: Get notifications, mark as read, unread count, real-time updates ready

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.Application.Common.Interfaces;
using PetCare.Application.Notifications.DTOs;
using PetCare.Domain.Notifications;

namespace PetCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    // Sprint Feature: Get user's notifications with pagination
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        var dtos = notifications.Select(MapToDto);
        
        return Ok(dtos);
    }

    // Sprint Feature: Get unread notification count for UI badges
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var count = await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
        
        return Ok(new { unreadCount = count });
    }

    // Sprint Feature: Mark notification as read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notification = await _notificationRepository.GetByIdAsync(id);
        
        if (notification == null)
            return NotFound();

        // Authorization: Users can only mark their own notifications as read
        if (notification.UserId != userId)
            return Forbid();

        if (!notification.IsRead)
        {
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }

        var dto = MapToDto(notification);
        return Ok(dto);
    }

    // Sprint Feature: Mark all notifications as read
    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationRepository.MarkAllAsReadAsync(userId);
        
        return Ok(new { message = "All notifications marked as read" });
    }

    // Sprint Feature: Get notification by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notification = await _notificationRepository.GetByIdAsync(id);
        
        if (notification == null)
            return NotFound();

        // Authorization: Users can only access their own notifications
        if (notification.UserId != userId)
            return Forbid();

        var dto = MapToDto(notification);
        return Ok(dto);
    }

    // Admin Feature: Get all notifications (for admin oversight) - Remove this endpoint since interface doesn't support it
    // [HttpGet("admin/all")]
    // [Authorize(Roles = "ADMIN")]
    // This feature can be added later when pagination is needed

    // Helper method to map domain entity to DTO
    private static NotificationDto MapToDto(Notification notification)
    {
        // Map type to display name
        string typeDisplayName = notification.Type switch
        {
            NotificationType.AppointmentApproved => "Appointment Approved",
            NotificationType.AppointmentCancelled => "Appointment Cancelled",
            NotificationType.AppointmentAssigned => "Vet Assigned",
            NotificationType.AppointmentReminder => "Appointment Reminder",
            NotificationType.SystemMessage => "System Message",
            _ => notification.Type.ToString()
        };

        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            TypeDisplayName = typeDisplayName,
            Title = notification.Title,
            Message = notification.Message,
            Data = notification.Data,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            IsRecent = notification.IsRecent
        };
    }
}
