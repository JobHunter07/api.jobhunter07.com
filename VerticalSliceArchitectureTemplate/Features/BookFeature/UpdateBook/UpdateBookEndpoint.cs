using VerticalSliceArchitectureTemplate.Abstractions;
using VerticalSliceArchitectureTemplate.Constants;
using VerticalSliceArchitectureTemplate.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.BookFeature.UpdateBook;

internal sealed class UpdateBookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("books/{id:guid}", async (Guid id, UpdateBookRequest request, IHandler<UpdateBookRequest, Result<UpdateBookResponse>> handler, CancellationToken cancellationToken) =>
        {
            var updateRequest = request with { Id = id };
            var result = await handler.HandleAsync(updateRequest, cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Books)
        .Produces<UpdateBookResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
