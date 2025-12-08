namespace API.Services.FanOutZipcodeResolver;

public interface IZipcodeProvider
{
    Task<ZipcodeProviderResponse?> GetZipcodeAddress(string zipcode, CancellationToken cancellationToken);
}

public record ZipcodeProviderResponse(
    string Street, 
    string Neighborhood, 
    string City,
    string State);