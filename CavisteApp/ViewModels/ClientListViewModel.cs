using System.Collections.ObjectModel;
using CavisteApp.Models;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

/// <summary>
/// Gère la liste des clients et le formulaire d'édition. Le CRUD passe par
/// <see cref="ClientService"/> (SQL brut / ADO.NET).
/// </summary>
public partial class ClientListViewModel : ObservableObject
{
    private readonly ClientService _clientService = new();

    [ObservableProperty]
    private ObservableCollection<Client> clients = new();

    [ObservableProperty]
    private Client? clientSelectionne;

    [ObservableProperty] private int clientId;
    [ObservableProperty] private string nom = string.Empty;
    [ObservableProperty] private string prenom = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string adressePostale = string.Empty;

    [ObservableProperty] private string messageStatut = string.Empty;

    public ClientListViewModel()
    {
        _ = ChargerAsync();
    }

    partial void OnClientSelectionneChanged(Client? value)
    {
        if (value is null)
        {
            ReinitialiserFormulaire();
            return;
        }

        ClientId = value.Id;
        Nom = value.Nom;
        Prenom = value.Prenom;
        Email = value.Email;
        AdressePostale = value.AdressePostale;
    }

    [RelayCommand]
    private async Task ChargerAsync()
    {
        var liste = await _clientService.ListerAsync();
        Clients = new ObservableCollection<Client>(liste);
    }

    [RelayCommand]
    private void Nouveau()
    {
        ClientSelectionne = null;
        ReinitialiserFormulaire();
    }

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Prenom) || string.IsNullOrWhiteSpace(Email))
        {
            MessageStatut = "Nom, prénom et email sont obligatoires.";
            return;
        }

        if (ClientId == 0)
        {
            var nouveau = new Client
            {
                Nom = Nom,
                Prenom = Prenom,
                Email = Email,
                AdressePostale = AdressePostale
            };
            await _clientService.AjouterAsync(nouveau);
            MessageStatut = $"Client '{nouveau.NomComplet}' ajouté.";
        }
        else
        {
            var modifie = new Client
            {
                Id = ClientId,
                Nom = Nom,
                Prenom = Prenom,
                Email = Email,
                AdressePostale = AdressePostale
            };
            await _clientService.ModifierAsync(modifie);
            MessageStatut = $"Client '{modifie.NomComplet}' modifié.";
        }

        await ChargerAsync();
        Nouveau();
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (ClientSelectionne is null) return;

        await _clientService.SupprimerAsync(ClientSelectionne.Id);
        MessageStatut = $"Client '{ClientSelectionne.NomComplet}' supprimé.";
        await ChargerAsync();
        Nouveau();
    }

    private void ReinitialiserFormulaire()
    {
        ClientId = 0;
        Nom = string.Empty;
        Prenom = string.Empty;
        Email = string.Empty;
        AdressePostale = string.Empty;
    }
}
