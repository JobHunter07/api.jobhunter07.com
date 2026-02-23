using System.Reflection.Metadata;
using VerticalSliceArchitectureTemplate.Abstractions;
using VerticalSliceArchitectureTemplate.Constants;
using VerticalSliceArchitectureTemplate.Extensions;
namespace VerticalSliceArchitectureTemplate.Features.BookFeature.CreateBook;

internal sealed class CreateBookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("books", async (IHandler<CreateBookRequest, Result<CreateBookResponse>> handler, CreateBookRequest command, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.Match(
              onSuccess: () => Results.Ok(result.Value),
              onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Books)
        .Produces<CreateBookResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
