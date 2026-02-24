using JobHunter07.API.Abstractions;
using JobHunter07.API.Abstractions.Errors;

namespace JobHunter07.API.Extensions;

public static class ResultExtensions
{
    // For non-generic Result
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error);
    }

    // 🔥 THIS is what your endpoint needs
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }
}
