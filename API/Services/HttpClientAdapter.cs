namespace API.Services;

public interface IHttpClientAdapter
{
    Task<HttpResponseMessage> GetAsync(string requestUri , CancellationToken cancellationToken);
}

public class HttpClientAdapter(HttpClient client) : IHttpClientAdapter
{
    private readonly HttpClient _client = client;

    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken)
        => await _client.GetAsync(requestUri, cancellationToken);
}