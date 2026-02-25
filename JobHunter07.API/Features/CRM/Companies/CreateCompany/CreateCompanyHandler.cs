using JobHunter07.API.Abstractions;
using JobHunter07.API.Entities;
using JobHunter07.API.Features.Crm.Companies.Repository;

namespace JobHunter07.API.Features.Crm.Companies.CreateCompany;

public sealed record CreateCompanyRequest(string Name, string? Domain, string? Description, string? Industry, string? WebsiteUrl, string? LinkedInUrl);
public sealed record CreateCompanyResponse(Guid CompanyId, string Name, string? Domain, string? Description, string? Industry, string? WebsiteUrl, string? LinkedInUrl, DateTime CreatedAt, DateTime UpdatedAt, bool IsActive);

public sealed class CreateCompanyHandler(
    ICompanyRepository _companyRepo,
    IUnitOfWork _unitOfWork) : IHandler<CreateCompanyRequest, Result<CreateCompanyResponse>>
{
    public async Task<Result<CreateCompanyResponse>> HandleAsync(CreateCompanyRequest command, CancellationToken cancellationToken)
    {
        if (await _companyRepo.ExistsByNameAsync(command.Name, null, cancellationToken))
            return CompanyErrors.NameConflict(command.Name);

        if (!string.IsNullOrWhiteSpace(command.Domain) && await _companyRepo.ExistsByDomainAsync(command.Domain!, null, cancellationToken))
            return CompanyErrors.DomainConflict(command.Domain!);

        var now = DateTime.UtcNow;
        var company = new Company
        {
            CompanyId = Guid.CreateVersion7(),
            Name = command.Name,
            Domain = command.Domain,
            Description = command.Description,
            Industry = command.Industry,
            WebsiteUrl = command.WebsiteUrl,
            LinkedInUrl = command.LinkedInUrl,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        };

        await _companyRepo.AddAsync(company, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var resp = new CreateCompanyResponse(company.CompanyId, company.Name, company.Domain, company.Description, company.Industry, company.WebsiteUrl, company.LinkedInUrl, company.CreatedAt, company.UpdatedAt, company.IsActive);
        return Result.Success(resp);
    }
}
