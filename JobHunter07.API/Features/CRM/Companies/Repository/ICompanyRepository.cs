using JobHunter07.API.Entities;

namespace JobHunter07.API.Features.Crm.Companies.Repository;

public interface ICompanyRepository
{
    Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default);
    Task<Company?> GetByIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Company> Items, int TotalCount)> SearchAsync(
        string? name, string? domain, string? industry,
        int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Company company, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Company company, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDomainAsync(string domain, Guid? excludeId, CancellationToken cancellationToken = default);
}
