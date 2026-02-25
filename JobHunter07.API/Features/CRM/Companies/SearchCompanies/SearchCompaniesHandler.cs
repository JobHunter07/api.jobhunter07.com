using JobHunter07.API.Abstractions;
using JobHunter07.API.Features.Crm.Companies.Repository;

namespace JobHunter07.API.Features.Crm.Companies.SearchCompanies;

public sealed record SearchCompaniesRequest(string? Name, string? Domain, string? Industry, int Page = 1, int PageSize = 20);
public sealed record CompanyResponse(Guid CompanyId, string Name, string? Domain, string? Description, string? Industry, string? WebsiteUrl, string? LinkedInUrl, DateTime CreatedAt, DateTime UpdatedAt, bool IsActive);
public sealed record SearchCompaniesResponse(IEnumerable<CompanyResponse> Companies, int Page, int PageSize, int TotalCount);

public sealed class SearchCompaniesHandler(ICompanyRepository _companyRepo) : IHandler<SearchCompaniesRequest, Result<SearchCompaniesResponse>>
{
    public async Task<Result<SearchCompaniesResponse>> HandleAsync(SearchCompaniesRequest request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        var (items, total) = await _companyRepo.SearchAsync(request.Name, request.Domain, request.Industry, page, pageSize, cancellationToken);

        var respItems = items.Select(c => new CompanyResponse(c.CompanyId, c.Name, c.Domain, c.Description, c.Industry, c.WebsiteUrl, c.LinkedInUrl, c.CreatedAt, c.UpdatedAt, c.IsActive));
        var resp = new SearchCompaniesResponse(respItems, page, pageSize, total);
        return Result.Success(resp);
    }
}
