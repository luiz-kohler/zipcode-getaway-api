using System.Text.Json;
using System.Text.RegularExpressions;
using API.Services;
using API.Services.FanOutZipcodeResolver;
using API.Services.FanOutZipcodeResolver.Providers;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IHttpClientAdapter, HttpClientAdapter>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IZipcodeProvider, BrasilApiProvider>();
builder.Services.AddScoped<IZipcodeProvider, ViaCepProvider>();

builder.Services.AddScoped<IZipcodeResolver, ZipcodeResolver>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();


app.MapGet("/addresses/{zipcode}", async (
        string zipcode,
        IZipcodeResolver resolver,
        CancellationToken cancellationToken) =>
    {   
        var zipcodeFormatted = zipcode?
            .Replace(".", string.Empty)?
            .Replace("-", string.Empty)?
            .Trim() ?? string.Empty;
        
        const string zipcodeValidatorRegex = @"^\d{5}-?\d{3}$";
        var isZipcodeValid = Regex.IsMatch(zipcodeFormatted, zipcodeValidatorRegex);
        
        if(!isZipcodeValid) 
            return Results.BadRequest("must inform be a valid zipcode.");

        try
        {
            var response = await resolver.GetZipcodeAddress(zipcodeFormatted, cancellationToken);
            return Results.Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("zipcode not found.");
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    })
    .WithName("GetAddressByZipcode")
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError);


app.UseHttpsRedirection();

app.Run();