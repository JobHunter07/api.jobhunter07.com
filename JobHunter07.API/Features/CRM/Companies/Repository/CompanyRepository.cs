using Microsoft.EntityFrameworkCore;
using JobHunter07.API.Database;
using JobHunter07.API.Entities;

namespace JobHunter07.API.Features.Crm.Companies.Repository;

public class CompanyRepository(ApplicationDbContext _context) : ICompanyRepository
{

    public async Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _context.Set<Company>().AddAsync(company, cancellationToken);
        return company;
    }

    public async Task<Company?> GetByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == companyId, cancellationToken);
    }

    public async Task<(IEnumerable<Company> Items, int TotalCount)> SearchAsync(string? name, string? domain, string? industry, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Companies.AsNoTracking().Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.ToLower().Contains(name.ToLower()));

        if (!string.IsNullOrWhiteSpace(domain))
            query = query.Where(c => c.Domain != null && c.Domain.ToLower().Contains(domain.ToLower()));

        if (!string.IsNullOrWhiteSpace(industry))
            query = query.Where(c => c.Industry != null && c.Industry.ToLower().Contains(industry.ToLower()));

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        _context.Companies.Update(company);
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Company company, CancellationToken cancellationToken = default)
    {
        company.IsActive = false;
        _context.Companies.Update(company);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var q = _context.Companies.AsQueryable();
        var lowered = name.ToLower();
        q = q.Where(c => c.Name.ToLower() == lowered);
        if (excludeId.HasValue)
            q = q.Where(c => c.CompanyId != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByDomainAsync(string domain, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var q = _context.Companies.AsQueryable();
        var lowered = domain.ToLower();
        q = q.Where(c => c.Domain != null && c.Domain.ToLower() == lowered);
        if (excludeId.HasValue)
            q = q.Where(c => c.CompanyId != excludeId.Value);
        return await q.AnyAsync(cancellationToken);
    }
}
