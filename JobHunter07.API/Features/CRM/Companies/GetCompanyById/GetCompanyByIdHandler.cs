using JobHunter07.API.Abstractions;
using JobHunter07.API.Entities;
using JobHunter07.API.Features.Crm.Companies.Repository;

namespace JobHunter07.API.Features.Crm.Companies.GetCompanyById;

public sealed record GetCompanyByIdRequest(Guid CompanyId);
public sealed record GetCompanyByIdResponse(Guid CompanyId, string Name, string? Domain, string? Description, string? Industry, string? WebsiteUrl, string? LinkedInUrl, DateTime CreatedAt, DateTime UpdatedAt, bool IsActive);

public sealed class GetCompanyByIdHandler(ICompanyRepository _companyRepo) : IHandler<GetCompanyByIdRequest, Result<GetCompanyByIdResponse>>
{
    public async Task<Result<GetCompanyByIdResponse>> HandleAsync(GetCompanyByIdRequest request, CancellationToken cancellationToken)
    {
        var company = await _companyRepo.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null || !company.IsActive)
            return CompanyErrors.NotFound(request.CompanyId);

        var resp = new GetCompanyByIdResponse(company.CompanyId, company.Name, company.Domain, company.Description, company.Industry, company.WebsiteUrl, company.LinkedInUrl, company.CreatedAt, company.UpdatedAt, company.IsActive);
        return Result.Success(resp);
    }
}
