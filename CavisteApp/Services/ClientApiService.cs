using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CavisteApp.Models;

namespace CavisteApp.Services;

public class RandomUserApiResponse
{
    public List<RandomUserDto>? Results { get; set; }
}

public class RandomUserDto
{
    public RandomUserName? Name { get; set; }
    public string? Email { get; set; }
    public RandomUserLocation? Location { get; set; }
}

public class RandomUserName
{
    public string? First { get; set; }
    public string? Last { get; set; }
}

public class RandomUserLocation
{
    public RandomUserStreet? Street { get; set; }
    public string? City { get; set; }

    public JsonElement Postcode { get; set; }

    public string PostcodeAffiche => Postcode.ValueKind switch
    {
        JsonValueKind.String => Postcode.GetString() ?? string.Empty,
        JsonValueKind.Number => Postcode.GetRawText(),
        _ => string.Empty
    };
}

public class RandomUserStreet
{
    public int Number { get; set; }
    public string? Name { get; set; }
}

public class ClientApiService
{
    private const string BaseUrl = "https://randomuser.me/api/";
    private readonly HttpClient _httpClient;

    public ClientApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<List<Client>> GenererClientsAsync(int nombre = 10, string nationalite = "fr")
    {
        var url = $"{BaseUrl}?results={nombre}&nat={nationalite}&inc=name,email,location";
        var reponse = await _httpClient.GetFromJsonAsync<RandomUserApiResponse>(url);

        return (reponse?.Results ?? new List<RandomUserDto>())
            .Where(u => u.Name is not null && !string.IsNullOrWhiteSpace(u.Email))
            .Select(u => new Client
            {
                Nom = Capitaliser(u.Name!.Last ?? string.Empty),
                Prenom = Capitaliser(u.Name.First ?? string.Empty),
                Email = u.Email!,
                AdressePostale = FormaterAdresse(u.Location)
            })
            .ToList();
    }

    private static string FormaterAdresse(RandomUserLocation? location)
    {
        if (location is null) return string.Empty;
        var numero = location.Street?.Number.ToString() ?? string.Empty;
        var rue = location.Street?.Name ?? string.Empty;
        return $"{numero} {rue}, {location.PostcodeAffiche} {location.City}".Trim();
    }

    private static string Capitaliser(string valeur) =>
        string.IsNullOrEmpty(valeur) ? valeur : char.ToUpper(valeur[0]) + valeur[1..];
}
