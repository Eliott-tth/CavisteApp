using System.Net.Http;
using System.Net.Http.Json;
using CavisteApp.Models;

namespace CavisteApp.Services;

/// <summary>
/// DTO correspondant au format JSON renvoyé par https://api.sampleapis.com/wines/{categorie}.
/// L'API ne fournit pas de prix : voir <see cref="WineApiService.GenererPrixIndicatif"/>.
/// </summary>
public class WineApiDto
{
    public string? Winery { get; set; }
    public string? Wine { get; set; }
    public string? Location { get; set; }
    public string? Image { get; set; }
    public int Id { get; set; }
}

/// <summary>
/// Consomme l'API publique et gratuite sampleapis.com/wines pour enrichir la
/// base de données du caviste (cf. exigence "Intégration d'une API pour
/// récupérer une liste de vins" du cahier des charges).
///
/// Veille effectuée (à citer en soutenance) :
///   - TheCocktailDB (cité dans les consignes) : porte sur les cocktails, pas les vins -> écarté.
///   - OpenWine API : introuvable / non maintenue -> écartée.
///   - Wine-Searcher API : complète mais nécessite une clé payante -> écartée pour un projet étudiant.
///   - sampleapis.com/wines : gratuite, sans authentification, 4 catégories (reds,
///     whites, sparkling, rosé/dessert), mise à jour régulière, utilisée dans
///     plusieurs tutoriels (Medium, GitHub) -> retenue.
/// Limite assumée : l'API ne renvoie pas de prix ; un prix indicatif est donc
/// généré à l'import et reste modifiable manuellement par le caviste.
/// </summary>
public class WineApiService
{
    private const string BaseUrl = "https://api.sampleapis.com/wines";
    private readonly HttpClient _httpClient;
    private readonly Random _random = new();

    public WineApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Récupère une catégorie de vins depuis l'API et les convertit en entités
    /// <see cref="Vin"/> prêtes à être insérées en base.
    /// </summary>
    /// <param name="type">Catégorie de vin à importer.</param>
    /// <param name="nombre">Nombre maximum de vins à ramener (l'API en renvoie plusieurs centaines).</param>
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

    /// <summary>
    /// Génère un prix de vente indicatif (l'API source ne fournissant pas cette
    /// donnée). Le caviste peut ensuite l'ajuster manuellement via l'écran de
    /// gestion du stock.
    /// </summary>
    private decimal GenererPrixIndicatif()
    {
        return Math.Round((decimal)(_random.NextDouble() * (60 - 8) + 8), 2);
    }
}
