using JobHunter07.API.Abstractions.Errors;
namespace JobHunter07.API.Features.BookFeature;

public static class BookErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Books.NotFound", $"The Book with Id '{id}' was not found");
}