using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();


app.MapGet("/addresses/{zipcode}", async (string zipcode) =>
    {   
        var zipcodeFormatted = zipcode?
            .Replace(".", string.Empty)?
            .Replace("-", string.Empty)?
            .Trim() ?? string.Empty;
        
        const string zipcodeValidatorRegex = @"^\d{5}-?\d{3}$";
        var isZipcodeValid = Regex.IsMatch(zipcodeFormatted, zipcodeValidatorRegex);
        
        if(!isZipcodeValid) 
            return Results.BadRequest("must inform be a valid zipcode.");
        
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"https://viacep.com.br/ws/{zipcodeFormatted}/json");
        
        if (!response.IsSuccessStatusCode)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        var body = await response.Content.ReadAsStringAsync();
        var address = JsonSerializer.Deserialize<ViaCepResponse>(body);
        
        if(address?.Cep is null)
            return Results.NotFound("Zipcode not found");
        
        return Results.Ok(address);
    })
    .WithName("GetAddressByCep")
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError);


app.UseHttpsRedirection();

app.Run();

public class ViaCepResponse
{
    [JsonPropertyName("cep")]
    public string Cep { get; set; }

    [JsonPropertyName("logradouro")]
    public string Logradouro { get; set; }

    [JsonPropertyName("complemento")]
    public string Complemento { get; set; }

    [JsonPropertyName("unidade")]
    public string Unidade { get; set; }

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; }

    [JsonPropertyName("localidade")]
    public string Localidade { get; set; }

    [JsonPropertyName("uf")]
    public string Uf { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; }

    [JsonPropertyName("regiao")]
    public string Regiao { get; set; }

    [JsonPropertyName("ibge")]
    public string Ibge { get; set; }

    [JsonPropertyName("gia")]
    public string Gia { get; set; }

    [JsonPropertyName("ddd")]
    public string Ddd { get; set; }

    [JsonPropertyName("siafi")]
    public string Siafi { get; set; }
}
