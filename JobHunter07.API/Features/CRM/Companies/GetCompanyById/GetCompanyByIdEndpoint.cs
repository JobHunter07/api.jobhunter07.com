using JobHunter07.API.Abstractions;
using JobHunter07.API.Constants;

namespace JobHunter07.API.Features.Crm.Companies.GetCompanyById;

internal sealed class GetCompanyByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("crm/companies/{id:guid}", async (IHandler<GetCompanyByIdRequest, Result<GetCompanyByIdResponse>> handler, Guid id, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetCompanyByIdRequest(id), cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.NotFound(error));
        })
        .WithTags(ApiTags.Companies)
        .Produces<GetCompanyByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
