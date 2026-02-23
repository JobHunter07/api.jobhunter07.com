namespace VerticalSliceArchitectureTemplate.Features.BookFeature.GetAllBooks;

using VerticalSliceArchitectureTemplate.Abstractions;
using VerticalSliceArchitectureTemplate.Entities;
using VerticalSliceArchitectureTemplate.Repository;

public sealed record GetAllBooksRequest;
public sealed record GetAllBooksResponse(IEnumerable<BookDto> Books);
public sealed record BookDto(Guid Id, string Title, string Author, string ISBN, decimal Price, int PublishedYear);

public sealed class GetAllBooksHandler(
    IRepository<Book> _bookRepo) : IHandler<GetAllBooksRequest, Result<GetAllBooksResponse>>
{
    public async Task<Result<GetAllBooksResponse>> HandleAsync(GetAllBooksRequest command, CancellationToken cancellationToken)
    {
        var books = await _bookRepo.GetAllAsync(cancellationToken);
        var bookDtos = books.Select(b => new BookDto(b.Id, b.Title, b.Author, b.ISBN, b.Price, b.PublishedYear)).ToList();
        
        return Result.Success(new GetAllBooksResponse(bookDtos));
    }
}
