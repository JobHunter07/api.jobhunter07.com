using JobHunter07.API.Abstractions;
using JobHunter07.API.Constants;
using JobHunter07.API.Extensions;

namespace JobHunter07.API.Features.Crm.Companies.UpdateCompany;

internal sealed class UpdateCompanyEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("crm/companies/{id:guid}", async (IHandler<UpdateCompanyRequest, Result<UpdateCompanyResponse>> handler, Guid id, UpdateCompanyRequest request, CancellationToken cancellationToken) =>
        {
            // Ensure id from route is used
            var cmd = new UpdateCompanyRequest(id, request.Name, request.Domain, request.Description, request.Industry, request.WebsiteUrl, request.LinkedInUrl);
            var result = await handler.HandleAsync(cmd, cancellationToken);
                return result.Match(
                    onSuccess: resp => Results.Ok(resp),
                    onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Companies)
        .Produces<UpdateCompanyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
