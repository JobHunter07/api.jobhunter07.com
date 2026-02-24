using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using JobHunter07.API.Database;

namespace JobHunter07.API.Extensions
{
    public static class HealthChecksExtensions
    {
        public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>();
            return services;
        }
        public static IEndpointRouteBuilder UseHealthChecks(this IEndpointRouteBuilder app)
        {
            app.MapHealthChecks("health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            return app;
        }
    }
}
