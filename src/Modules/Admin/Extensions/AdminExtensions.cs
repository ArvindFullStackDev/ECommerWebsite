using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Extensions;

public static class AdminExtensions
{
    public static IServiceCollection AddAdminModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
