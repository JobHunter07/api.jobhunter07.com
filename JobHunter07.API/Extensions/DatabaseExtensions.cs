using Microsoft.EntityFrameworkCore;
using JobHunter07.API.Database;

namespace JobHunter07.API.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddSQLDatabaseConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("connection"));
            });

            return services;
        }
    }
}
