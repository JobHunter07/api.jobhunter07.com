using JobHunter07.API.Abstractions;
using JobHunter07.API.Constants;
using JobHunter07.API.Extensions;

namespace JobHunter07.API.Features.Crm.Companies.DeleteCompany;

internal sealed class DeleteCompanyEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("crm/companies/{id:guid}", async (IHandler<DeleteCompanyRequest, Result<DeleteCompanyResponse>> handler, Guid id, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteCompanyRequest(id), cancellationToken);
                return result.Match(
                    onSuccess: resp => Results.Ok(new DeleteCompanyResponse(id)),
                    onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Companies)
        .Produces<DeleteCompanyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
