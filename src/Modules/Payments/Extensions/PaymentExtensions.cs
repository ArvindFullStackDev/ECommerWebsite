using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Payments.Interfaces;
using Payments.Services;

namespace Payments.Extensions;

public static class PaymentExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<IPaymentService, CashOnDeliveryService>();
        services.AddScoped<StripePaymentService>();
        return services;
    }
}
