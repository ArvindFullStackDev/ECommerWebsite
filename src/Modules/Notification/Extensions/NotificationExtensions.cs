using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Notification.Hubs;
using Notification.Interfaces;
using Notification.Services;

namespace Notification.Extensions;

public static class NotificationExtensions
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSignalR();
        return services;
    }
}
