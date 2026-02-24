using Azure.Core;


namespace JobHunter07.API.Abstractions
{
    public interface IHandler<in TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest command, CancellationToken cancellationToken);
    }

    
}
