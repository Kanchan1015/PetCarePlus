// INotificationRepository.cs
// Purpose: Repository interface for notification data access with read state management

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCare.Domain.Notifications;

namespace PetCare.Application.Common.Interfaces;

public interface INotificationRepository
{
    // Basic CRUD operations
    Task<Notification> CreateAsync(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<Notification> UpdateAsync(Notification notification);
    Task DeleteAsync(Guid id);

    // User-specific queries
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
    Task<int> GetUnreadCountByUserIdAsync(string userId);

    // Read state management
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(string userId);

    // Cleanup operations
    Task DeleteOldNotificationsAsync(DateTime cutoffDate);
}
