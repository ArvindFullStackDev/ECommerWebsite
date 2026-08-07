using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Reporting.Extensions;

public static class ReportingExtensions
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
