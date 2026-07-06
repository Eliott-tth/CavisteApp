using System.Net.Http;
using System.Net.Http.Json;
using CavisteApp.Models;

namespace CavisteApp.Services;

public class WineApiDto
{
    public string? Winery { get; set; }
    public string? Wine { get; set; }
    public string? Location { get; set; }
    public string? Image { get; set; }
    public int Id { get; set; }
}

public class WineApiService
{
    private const string BaseUrl = "https://api.sampleapis.com/wines";
    private readonly HttpClient _httpClient;
    private readonly Random _random = new();

    public WineApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<List<Vin>> RecupererVinsAsync(TypeVin type, int nombre = 20)
    {
        var endpoint = type switch
        {
            TypeVin.Rouge => "reds",
            TypeVin.Blanc => "whites",
            TypeVin.Petillant => "sparkling",
            TypeVin.Rose => "rosé",
            _ => "reds"
        };

        var url = $"{BaseUrl}/{endpoint}";
        var resultat = await _httpClient.GetFromJsonAsync<List<WineApiDto>>(url)
                       ?? new List<WineApiDto>();

        return resultat
            .Take(nombre)
            .Where(w => !string.IsNullOrWhiteSpace(w.Wine))
            .Select(w => new Vin
            {
                Nom = string.IsNullOrWhiteSpace(w.Winery) ? w.Wine! : $"{w.Winery} - {w.Wine}",
                Type = type,
                Prix = GenererPrixIndicatif(),
                Stock = 0,
                SeuilBas = 5,
                Origine = "Import API sampleapis.com",
                ImageUrl = w.Image
            })
            .ToList();
    }

    private decimal GenererPrixIndicatif()
    {
        return Math.Round((decimal)(_random.NextDouble() * (60 - 8) + 8), 2);
    }
}
