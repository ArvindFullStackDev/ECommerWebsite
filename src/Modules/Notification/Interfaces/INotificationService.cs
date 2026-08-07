using Notification.DTOs;

namespace Notification.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, string type, string? referenceId = null);
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync(string userId);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}
