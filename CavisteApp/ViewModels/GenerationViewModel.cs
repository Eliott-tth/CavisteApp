using System;
using System.Threading.Tasks;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

public partial class GenerationViewModel : ObservableObject
{
    private readonly DonneesTestService _donneesTestService = new();
    private readonly ClientApiService _clientApiService = new();
    private readonly ClientService _clientService = new();

    [ObservableProperty] private int nombreClients = 10;
    [ObservableProperty] private int nombreFournisseurs = 5;
    [ObservableProperty] private int nombreCommandes = 5;
    [ObservableProperty] private int nombreVentes = 10;

    [ObservableProperty] private string messageStatut = string.Empty;
    [ObservableProperty] private bool generationEnCours;

    [RelayCommand]
    private async Task RandomiserStockAsync()
    {
        await ExecuterAsync(async () =>
        {
            var nombre = await _donneesTestService.RandomiserStockAsync();
            MessageStatut = $"Stock recalculé aléatoirement pour {nombre} vin(s).";
        });
    }

    [RelayCommand]
    private async Task GenererFournisseursAsync()
    {
        await ExecuterAsync(async () =>
        {
            var nombre = await _donneesTestService.GenererFournisseursAsync(NombreFournisseurs);
            MessageStatut = $"{nombre} fournisseur(s) généré(s).";
        });
    }

    [RelayCommand]
    private async Task GenererClientsAsync()
    {
        await ExecuterAsync(async () =>
        {
            var clients = await _clientApiService.GenererClientsAsync(NombreClients);
            foreach (var client in clients)
                await _clientService.AjouterAsync(client);
            MessageStatut = $"{clients.Count} client(s) généré(s) depuis l'API.";
        });
    }

    [RelayCommand]
    private async Task GenererCommandesAsync()
    {
        await ExecuterAsync(async () =>
        {
            var nombre = await _donneesTestService.GenererCommandesAsync(NombreCommandes);
            MessageStatut = nombre > 0
                ? $"{nombre} commande(s) fournisseur générée(s)."
                : "Aucune commande générée : associe un fournisseur à au moins un vin dans l'onglet Stock.";
        });
    }

    [RelayCommand]
    private async Task GenererVentesAsync()
    {
        await ExecuterAsync(async () =>
        {
            var nombre = await _donneesTestService.GenererVentesAsync(NombreVentes);
            MessageStatut = nombre > 0
                ? $"{nombre} vente(s) générée(s)."
                : "Aucune vente générée : vérifie qu'il y a des clients et du stock disponible.";
        });
    }

    [RelayCommand]
    private async Task GenererToutAsync()
    {
        await ExecuterAsync(async () =>
        {
            var stock = await _donneesTestService.RandomiserStockAsync();
            var fournisseurs = await _donneesTestService.GenererFournisseursAsync(NombreFournisseurs);

            var clients = await _clientApiService.GenererClientsAsync(NombreClients);
            foreach (var client in clients)
                await _clientService.AjouterAsync(client);

            var commandes = await _donneesTestService.GenererCommandesAsync(NombreCommandes);
            var ventes = await _donneesTestService.GenererVentesAsync(NombreVentes);

            MessageStatut = $"Génération complète : {stock} stock(s) recalculé(s), " +
                             $"{fournisseurs} fournisseur(s), {clients.Count} client(s), " +
                             $"{commandes} commande(s), {ventes} vente(s).";
        });
    }

    private async Task ExecuterAsync(Func<Task> action)
    {
        try
        {
            GenerationEnCours = true;
            MessageStatut = "Génération en cours...";
            await action();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la génération : {ex.InnerException?.Message ?? ex.Message}";
        }
        finally
        {
            GenerationEnCours = false;
        }
    }
}
