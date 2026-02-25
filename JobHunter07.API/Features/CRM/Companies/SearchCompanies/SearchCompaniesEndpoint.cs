using JobHunter07.API.Abstractions;
using JobHunter07.API.Constants;

namespace JobHunter07.API.Features.Crm.Companies.SearchCompanies;

internal sealed class SearchCompaniesEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("crm/companies", async (IHandler<SearchCompaniesRequest, Result<SearchCompaniesResponse>> handler, string? name, string? domain, string? industry, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        {
            var request = new SearchCompaniesRequest(name, domain, industry, page, pageSize);
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Companies)
        .Produces<SearchCompaniesResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
