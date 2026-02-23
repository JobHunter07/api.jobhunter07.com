namespace VerticalSliceArchitectureTemplate.Features.BookFeature.GetBookById;

using VerticalSliceArchitectureTemplate.Abstractions;
using VerticalSliceArchitectureTemplate.Entities;
using VerticalSliceArchitectureTemplate.Repository;
using VerticalSliceArchitectureTemplate.Features.BookFeature;

public sealed record GetBookByIdRequest(Guid Id);
public sealed record GetBookByIdResponse(Guid Id, string Title, string Author, string ISBN, decimal Price, int PublishedYear);

public sealed class GetBookByIdHandler(
    IRepository<Book> _bookRepo) : IHandler<GetBookByIdRequest, Result<GetBookByIdResponse>>
{
    public async Task<Result<GetBookByIdResponse>> HandleAsync(GetBookByIdRequest command, CancellationToken cancellationToken)
    {
        var book = await _bookRepo.GetByIdAsync(command.Id, cancellationToken);
        
        if (book == null)
        {
            return Result.Failure<GetBookByIdResponse>(BookErrors.NotFound(command.Id));
        }

        var response = new GetBookByIdResponse(book.Id, book.Title, book.Author, book.ISBN, book.Price, book.PublishedYear);
        return Result.Success(response);
    }
}
