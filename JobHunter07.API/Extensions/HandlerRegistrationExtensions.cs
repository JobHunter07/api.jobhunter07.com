using System.Reflection;
using JobHunter07.API.Abstractions;
using JobHunter07.API.Pipelines;

namespace JobHunter07.API.Extensions;

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection AddHandlersFromAssembly(
    this IServiceCollection services,
    Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                !t.ContainsGenericParameters)
            .ToList();

        foreach (var implementation in handlerTypes)
        {
            var handlerInterfaces = implementation
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IHandler<,>));

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, implementation);
            }
        }

        services.Decorate(typeof(IHandler<,>), typeof(ValidationDecorator<,>));
        services.Decorate(typeof(IHandler<,>), typeof(LoggingDecorator<,>));
        

        return services;
    }
}