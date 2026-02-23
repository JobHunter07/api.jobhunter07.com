namespace VerticalSliceArchitectureTemplate.Features.BookFeature.UpdateBook;

using VerticalSliceArchitectureTemplate.Abstractions;
using VerticalSliceArchitectureTemplate.Entities;
using VerticalSliceArchitectureTemplate.Repository;
using VerticalSliceArchitectureTemplate.Features.BookFeature;

public sealed record UpdateBookRequest(Guid Id, string? Title, string? Author, string? ISBN, decimal? Price, int? PublishedYear);
public sealed record UpdateBookResponse(Guid Id, string Title, string Author, string ISBN, decimal Price, int PublishedYear);

public sealed class UpdateBookHandler(
    IRepository<Book> _bookRepo,
    IUnitOfWork _unitOfWork) : IHandler<UpdateBookRequest, Result<UpdateBookResponse>>
{
    public async Task<Result<UpdateBookResponse>> HandleAsync(UpdateBookRequest command, CancellationToken cancellationToken)
    {
        var book = await _bookRepo.GetByIdAsync(command.Id, cancellationToken);
        
        if (book == null)
        {
            return Result.Failure<UpdateBookResponse>(BookErrors.NotFound(command.Id));
        }

        book.Title = command.Title ?? book.Title;
        book.Author = command.Author ?? book.Author;
        book.ISBN = command.ISBN ?? book.ISBN;
        book.Price = command.Price ?? book.Price;
        book.PublishedYear = command.PublishedYear ?? book.PublishedYear;

        await _bookRepo.UpdateAsync(book, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var response = new UpdateBookResponse(book.Id, book.Title, book.Author, book.ISBN, book.Price, book.PublishedYear);
        return Result.Success(response);
    }
}
