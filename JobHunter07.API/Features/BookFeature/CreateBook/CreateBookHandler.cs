using JobHunter07.API.Abstractions;
using JobHunter07.API.Entities;
using JobHunter07.API.Repository;

namespace JobHunter07.API.Features.BookFeature.CreateBook;

public sealed record CreateBookRequest(string Title, string Author, string ISBN, decimal Price, int PublishedYear);
public sealed record CreateBookResponse(Guid Id, string Title, string Author, string ISBN, decimal Price, int PublishedYear);

public sealed class CreateBookHandler(
    IRepository<Book> _callLogRepo,
    IUnitOfWork _unitOfWork) : IHandler<CreateBookRequest, Result<CreateBookResponse>>
{
    public async Task<Result<CreateBookResponse>> HandleAsync(CreateBookRequest command, CancellationToken cancellationToken)
    {
        var book = new Book
        {
            Id = Guid.CreateVersion7(),
            Title = command.Title,
            Author = command.Author,
            ISBN = command.ISBN,
            Price = command.Price,
            PublishedYear = command.PublishedYear
        };

        await _callLogRepo.AddAsync(book, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new CreateBookResponse(book.Id, book.Title, book.Author, book.ISBN, book.Price, book.PublishedYear));
    }
}
