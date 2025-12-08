using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Services.FanOutZipcodeResolver.Providers;

public class BrasilApiProvider(IHttpClientAdapter httpClientAdapter, ILogger<BrasilApiProvider> logger) : IZipcodeProvider
{
    private readonly IHttpClientAdapter _httpClient = httpClientAdapter;
    private readonly ILogger<BrasilApiProvider> _logger = logger;
    
    public async Task<ZipcodeProviderResponse?> GetZipcodeAddress(string zipcode, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"BrasilAPI provider started for {zipcode}");

        var response = await _httpClient.GetAsync($"https://brasilapi.com.br/api/cep/v1/{zipcode}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException();
        
        var model = await response.Content.ReadFromJsonAsync<BrasilApiResponse>(cancellationToken);

        _logger.LogInformation($"BrasilAPI provider used for zipcode {zipcode}");

        return new ZipcodeProviderResponse(
            model.Street,
            model.Neighborhood,
            model.City,
            model.State
        );    
    }
}

public class BrasilApiResponse
{
    public string? Cep { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Neighborhood { get; set; }
    public string? Street { get; set; }
}
