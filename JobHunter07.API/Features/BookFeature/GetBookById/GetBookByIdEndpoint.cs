using JobHunter07.API.Abstractions;
using JobHunter07.API.Constants;
using JobHunter07.API.Extensions;

namespace JobHunter07.API.Features.BookFeature.GetBookById;

internal sealed class GetBookByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("books/{id:guid}", async (Guid id, IHandler<GetBookByIdRequest, Result<GetBookByIdResponse>> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetBookByIdRequest(id), cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.NotFound(error));
        })
        .WithTags(ApiTags.Books)
        .Produces<GetBookByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
