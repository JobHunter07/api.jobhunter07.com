using JobHunter07.API.Abstractions;
using JobHunter07.API.Constants;
using JobHunter07.API.Extensions;

namespace JobHunter07.API.Features.Crm.Companies.CreateCompany;

internal sealed class CreateCompanyEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("crm/companies", async (IHandler<CreateCompanyRequest, Result<CreateCompanyResponse>> handler, CreateCompanyRequest command, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Companies)
        .Produces<CreateCompanyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);
    }
}
