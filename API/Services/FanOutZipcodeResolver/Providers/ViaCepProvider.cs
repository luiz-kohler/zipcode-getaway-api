using System.Text.Json.Serialization;

namespace API.Services.FanOutZipcodeResolver.Providers;

public class ViaCepProvider(IHttpClientAdapter httpClientAdapter, ILogger<ViaCepProvider> logger) : IZipcodeProvider
{
    private readonly IHttpClientAdapter _httpClient = httpClientAdapter;
    private readonly ILogger<ViaCepProvider> _logger = logger;
    
    public async Task<ZipcodeProviderResponse?> GetZipcodeAddress(string zipcode, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"### ViaCEP provider started for {zipcode}");

        var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{zipcode}/json", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var model = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken);
        
        if (model.Erro != null)
            throw new KeyNotFoundException();

        _logger.LogInformation($"### ViaCEP provider used for zipcode {zipcode}");

        return new ZipcodeProviderResponse(
            model.Logradouro,
            model.Bairro,
            model.Localidade,
            model.Uf
        );
    }
}

public class ViaCepResponse
{
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? Uf { get; set; }
    public string? Erro { get; set; }
}