using System.Net;

namespace API.Services.FanOutZipcodeResolver;

public interface IZipcodeResolver
{
    Task<ZipcodeProviderResponse?> GetZipcodeAddress(string zipcode, CancellationToken cancellationToken);
}

public class ZipcodeResolver(IEnumerable<IZipcodeProvider> providers) : IZipcodeResolver
{
    private readonly IEnumerable<IZipcodeProvider> _providers = providers;
    private readonly SemaphoreSlim _semaphore = new(5, 5);
    
    public async Task<ZipcodeProviderResponse?> GetZipcodeAddress(string zipcode, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        try {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var tasks = _providers.Select(provider =>
                Task.Run(async () => await provider.GetZipcodeAddress(zipcode, linkedCts.Token), linkedCts.Token)).ToList();
        
            while (tasks.Count != 0)
            {
                var completed = await Task.WhenAny(tasks);

                try
                {
                    var result = await completed;
                    if (result != null)
                    {
                        await linkedCts.CancelAsync();
                        return result;
                    }
                }
                catch (KeyNotFoundException)
                {
                    throw;
                }
                finally
                {
                    tasks.Remove(completed);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
        
        throw new Exception($"No zipcode provider found for {zipcode}");
    }
}