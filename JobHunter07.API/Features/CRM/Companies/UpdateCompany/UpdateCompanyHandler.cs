using JobHunter07.API.Abstractions;
using JobHunter07.API.Features.Crm.Companies.Repository;

namespace JobHunter07.API.Features.Crm.Companies.UpdateCompany;

public sealed record UpdateCompanyRequest(Guid CompanyId, string? Name, string? Domain, string? Description, string? Industry, string? WebsiteUrl, string? LinkedInUrl);
public sealed record UpdateCompanyResponse(Guid CompanyId, string Name, string? Domain, string? Description, string? Industry, string? WebsiteUrl, string? LinkedInUrl, DateTime CreatedAt, DateTime UpdatedAt, bool IsActive);

public sealed class UpdateCompanyHandler(ICompanyRepository _companyRepo, IUnitOfWork _unitOfWork) : IHandler<UpdateCompanyRequest, Result<UpdateCompanyResponse>>
{
    public async Task<Result<UpdateCompanyResponse>> HandleAsync(UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await _companyRepo.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null || !company.IsActive)
            return CompanyErrors.NotFound(request.CompanyId);

        if (!string.IsNullOrWhiteSpace(request.Name) && await _companyRepo.ExistsByNameAsync(request.Name, request.CompanyId, cancellationToken))
            return CompanyErrors.NameConflict(request.Name);

        if (!string.IsNullOrWhiteSpace(request.Domain) && await _companyRepo.ExistsByDomainAsync(request.Domain, request.CompanyId, cancellationToken))
            return CompanyErrors.DomainConflict(request.Domain);

        company.Name = request.Name ?? company.Name;
        company.Domain = request.Domain ?? company.Domain;
        company.Description = request.Description ?? company.Description;
        company.Industry = request.Industry ?? company.Industry;
        company.WebsiteUrl = request.WebsiteUrl ?? company.WebsiteUrl;
        company.LinkedInUrl = request.LinkedInUrl ?? company.LinkedInUrl;
        company.UpdatedAt = DateTime.UtcNow;

        await _companyRepo.UpdateAsync(company, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var resp = new UpdateCompanyResponse(company.CompanyId, company.Name, company.Domain, company.Description, company.Industry, company.WebsiteUrl, company.LinkedInUrl, company.CreatedAt, company.UpdatedAt, company.IsActive);
        return Result.Success(resp);
    }
}
