using JobHunter07.API.Abstractions;
using JobHunter07.API.Constants;
using JobHunter07.API.Extensions;

namespace JobHunter07.API.Features.BookFeature.DeleteBook;

internal sealed class DeleteBookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("books/{id:guid}", async (Guid id, IHandler<DeleteBookRequest, Result<DeleteBookResponse>> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteBookRequest(id), cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.NotFound(error));
        })
        .WithTags(ApiTags.Books)
        .Produces<DeleteBookResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
