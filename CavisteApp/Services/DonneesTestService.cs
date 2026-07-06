using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Models;

namespace CavisteApp.Services;

/// <summary>
/// Génère des jeux de données de test pour peupler rapidement l'application
/// (démonstration, développement). Les clients sont générés séparément via
/// <see cref="ClientApiService"/> (randomuser.me) ; ce service couvre le reste :
/// fournisseurs, randomisation du stock, commandes fournisseur, ventes.
/// </summary>
public class DonneesTestService
{
    private readonly VinService _vinService = new();
    private readonly ClientService _clientService = new();
    private readonly FournisseurService _fournisseurService = new();
    private readonly CommandeFournisseurService _commandeService = new();
    private readonly VenteService _venteService = new();

    private readonly Random _random = new();

    private static readonly string[] PrefixesFournisseur = { "Domaine", "Château", "Maison", "Cave", "Négoce", "Vignobles" };
    private static readonly string[] RegionsFournisseur = { "du Rhône", "de Bourgogne", "Bordelaise", "Alsacienne", "du Languedoc", "de Provence", "de la Loire", "Champenoise" };
    private static readonly string[] Rues = { "Rue des Vignes", "Chemin du Pressoir", "Avenue des Coteaux", "Route des Vendanges", "Rue de la Cave" };
    private static readonly string[] Villes = { "Beaune", "Bordeaux", "Avignon", "Reims", "Colmar", "Montpellier", "Angers" };

    /// <summary>Génère des fournisseurs fictifs (noms/adresses/emails plausibles, sans API externe).</summary>
    public async Task<int> GenererFournisseursAsync(int nombre)
    {
        var cree = 0;
        for (var i = 0; i < nombre; i++)
        {
            var prefixe = PrefixesFournisseur[_random.Next(PrefixesFournisseur.Length)];
            var region = RegionsFournisseur[_random.Next(RegionsFournisseur.Length)];
            var nom = $"{prefixe} {region} {_random.Next(1, 999)}";
            var slug = nom.ToLowerInvariant().Replace(" ", "-").Replace("é", "e").Replace("è", "e");

            var fournisseur = new Fournisseur
            {
                Nom = nom,
                Email = $"contact@{slug}.fr",
                Telephone = $"0{_random.Next(1, 6)}{_random.Next(10000000, 99999999)}",
                Adresse = $"{_random.Next(1, 200)} {Rues[_random.Next(Rues.Length)]}, {Villes[_random.Next(Villes.Length)]}"
            };

            await _fournisseurService.AjouterAsync(fournisseur);
            cree++;
        }
        return cree;
    }

    /// <summary>
    /// Réaffecte un stock aléatoire à chaque vin : ~30% restent sous leur seuil
    /// (pour garder des alertes visibles en démonstration), le reste au-dessus.
    /// </summary>
    public async Task<int> RandomiserStockAsync()
    {
        var vins = await _vinService.ListerAsync();
        foreach (var vin in vins)
        {
            vin.Stock = _random.NextDouble() < 0.3
                ? _random.Next(0, vin.SeuilBas + 1)
                : _random.Next(vin.SeuilBas + 1, 60);

            await _vinService.ModifierAsync(vin);
        }
        return vins.Count;
    }

    /// <summary>Crée des commandes fournisseur aléatoires pour des vins ayant un fournisseur associé.</summary>
    public async Task<int> GenererCommandesAsync(int nombre)
    {
        var vins = (await _vinService.ListerAsync()).Where(v => v.FournisseurId is not null).ToList();
        if (vins.Count == 0) return 0;

        var cree = 0;
        for (var i = 0; i < nombre; i++)
        {
            var vin = vins[_random.Next(vins.Count)];
            var quantite = _random.Next(6, 30);

            await _commandeService.CreerCommandeAsync(vin.FournisseurId!.Value,
                new List<(int VinId, int Quantite)> { (vin.Id, quantite) });
            cree++;
        }
        return cree;
    }

    /// <summary>Crée des ventes aléatoires (client + 1 à 3 vins en stock) sans jamais dépasser le stock disponible.</summary>
    public async Task<int> GenererVentesAsync(int nombre)
    {
        var clients = await _clientService.ListerAsync();
        if (clients.Count == 0) return 0;

        var cree = 0;
        for (var i = 0; i < nombre; i++)
        {
            var vinsDisponibles = (await _vinService.ListerAsync()).Where(v => v.Stock > 0).ToList();
            if (vinsDisponibles.Count == 0) break;

            var client = clients[_random.Next(clients.Count)];
            var nombreLignes = _random.Next(1, Math.Min(3, vinsDisponibles.Count) + 1);
            var vinsChoisis = vinsDisponibles.OrderBy(_ => _random.Next()).Take(nombreLignes).ToList();

            var lignes = vinsChoisis
                .Select(v => (v.Id, Quantite: _random.Next(1, Math.Min(v.Stock, 4) + 1)))
                .ToList();

            try
            {
                await _venteService.CreerVenteAsync(client.Id, lignes);
                cree++;
            }
            catch (InvalidOperationException)
            {
                // Stock devenu insuffisant entre-temps (deux vins choisis se recoupent) : on ignore cette itération.
            }
        }
        return cree;
    }
}
