using Microsoft.Extensions.DependencyInjection;
using JobHunter07.API.Features.Crm.Companies.Repository;

namespace JobHunter07.API.Features.Crm.Companies.Extensions;

public static class CompanyFeatureExtensions
{
    public static IServiceCollection AddCompanyFeatureServices(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        return services;
    }
}
