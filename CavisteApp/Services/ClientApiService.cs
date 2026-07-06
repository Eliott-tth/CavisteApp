using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CavisteApp.Models;

namespace CavisteApp.Services;

/// <summary>
/// DTOs correspondant au format JSON renvoyé par https://randomuser.me/api/.
/// </summary>
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

    // Le "postcode" est parfois une chaîne, parfois un nombre selon la nationalité
    // demandée : on le lit en JsonElement pour absorber les deux cas sans planter.
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

/// <summary>
/// Consomme l'API publique et gratuite randomuser.me pour générer des fiches
/// clients de démonstration (nom, prénom, email, adresse postale complète).
///
/// Veille effectuée (à citer en soutenance) :
///   - randomuser.me : gratuite, sans authentification, données réalistes et
///     localisées (paramètre "nat"), très utilisée pour peupler des jeux de
///     données de test -> retenue.
///   - api-ninjas.com/randomuser : équivalent mais nécessite une clé API -> écartée.
/// </summary>
public class ClientApiService
{
    private const string BaseUrl = "https://randomuser.me/api/";
    private readonly HttpClient _httpClient;

    public ClientApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Récupère des clients fictifs depuis l'API et les convertit en entités
    /// <see cref="Client"/> prêtes à être insérées en base.
    /// </summary>
    /// <param name="nombre">Nombre de clients à générer.</param>
    /// <param name="nationalite">Code nationalité (ex : "fr") pour des adresses cohérentes.</param>
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
