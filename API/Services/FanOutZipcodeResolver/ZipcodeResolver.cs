namespace API.Services.FanOutZipcodeResolver;

public interface IZipcodeResolver
{
    Task<ZipcodeProviderResponse?> GetZipcodeAddress(string zipcode, CancellationToken cancellationToken);
}

public class ZipcodeResolver(IEnumerable<IZipcodeProvider> providers, ILogger<ZipcodeResolver> logger) : IZipcodeResolver
{
    private readonly IEnumerable<IZipcodeProvider> _providers = providers;
    private readonly ILogger<ZipcodeResolver> _logger = logger;
    
    public async Task<ZipcodeProviderResponse?> GetZipcodeAddress(string zipcode, CancellationToken cancellationToken)
    {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                tasks.Remove(completed);
            }
        }
        
        throw new Exception();
    }
}