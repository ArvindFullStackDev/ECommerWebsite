using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Notification.DTOs;
using Notification.Interfaces;

namespace Notification.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    public NotificationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task SendNotificationAsync(string userId, string title, string message, string type, string? referenceId = null)
    {
        var notification = new Domain.Entities.Notification
        {
            UserId = userId, Title = title, Message = message,
            Type = Enum.TryParse<NotificationType>(type, out var nt) ? nt : NotificationType.Info,
            ReferenceId = referenceId
        };
        await _unitOfWork.Repository<Domain.Entities.Notification>().AddAsync(notification);
        await _unitOfWork.CompleteAsync();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var notif = await _unitOfWork.Repository<Domain.Entities.Notification>().GetByIdAsync(id);
        if (notif != null) { notif.IsRead = true; await _unitOfWork.CompleteAsync(); }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _unitOfWork.Repository<Domain.Entities.Notification>().GetQueryable()
            .Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in notifications) n.IsRead = true;
        await _unitOfWork.CompleteAsync();
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
    {
        return await _unitOfWork.Repository<Domain.Entities.Notification>().GetQueryable()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id, UserId = n.UserId, Title = n.Title, Message = n.Message,
                Type = n.Type.ToString(), IsRead = n.IsRead, ReferenceId = n.ReferenceId, CreatedAt = n.CreatedAt
            }).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _unitOfWork.Repository<Domain.Entities.Notification>().GetQueryable()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
