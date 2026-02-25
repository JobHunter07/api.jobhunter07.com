using JobHunter07.API.Abstractions;
using JobHunter07.API.Features.Crm.Companies.Repository;

namespace JobHunter07.API.Features.Crm.Companies.DeleteCompany;

public sealed record DeleteCompanyRequest(Guid CompanyId);
public sealed record DeleteCompanyResponse(Guid CompanyId);

public sealed class DeleteCompanyHandler(ICompanyRepository _companyRepo, IUnitOfWork _unitOfWork) : IHandler<DeleteCompanyRequest, Result<DeleteCompanyResponse>>
{
    public async Task<Result<DeleteCompanyResponse>> HandleAsync(DeleteCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await _companyRepo.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null || !company.IsActive)
            return CompanyErrors.NotFound(request.CompanyId);

        await _companyRepo.SoftDeleteAsync(company, cancellationToken);
        company.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new DeleteCompanyResponse(request.CompanyId));
    }
}
