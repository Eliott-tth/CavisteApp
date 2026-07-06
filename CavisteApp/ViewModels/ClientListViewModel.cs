using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly ClientApiService _clientApiService = new();

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
    [ObservableProperty] private bool importEnCours;

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

        try
        {
            int idSauvegarde;

            if (ClientId == 0)
            {
                var nouveau = new Client
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    AdressePostale = AdressePostale
                };
                idSauvegarde = await _clientService.AjouterAsync(nouveau);
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
                idSauvegarde = ClientId;
                MessageStatut = $"Client '{modifie.NomComplet}' modifié.";
            }

            await ChargerAsync();
            ClientSelectionne = Clients.FirstOrDefault(c => c.Id == idSauvegarde);
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de l'enregistrement : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (ClientSelectionne is null) return;

        try
        {
            await _clientService.SupprimerAsync(ClientSelectionne.Id);
            MessageStatut = $"Client '{ClientSelectionne.NomComplet}' supprimé.";
            await ChargerAsync();
            Nouveau();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la suppression : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    private void ReinitialiserFormulaire()
    {
        ClientId = 0;
        Nom = string.Empty;
        Prenom = string.Empty;
        Email = string.Empty;
        AdressePostale = string.Empty;
    }

    /// <summary>
    /// Génère 10 clients fictifs depuis l'API randomuser.me et les insère en
    /// base (voir ClientApiService pour la veille). Ignore les emails déjà présents.
    /// </summary>
    [RelayCommand]
    private async Task ImporterDepuisApiAsync()
    {
        try
        {
            ImportEnCours = true;
            MessageStatut = "Génération de clients en cours...";

            var clientsGeneres = await _clientApiService.GenererClientsAsync(10);

            var emailsExistants = Clients.Select(c => c.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var clientsNouveaux = clientsGeneres.Where(c => !emailsExistants.Contains(c.Email)).ToList();

            foreach (var client in clientsNouveaux)
                await _clientService.AjouterAsync(client);

            var ignores = clientsGeneres.Count - clientsNouveaux.Count;
            MessageStatut = ignores > 0
                ? $"{clientsNouveaux.Count} client(s) généré(s), {ignores} déjà présent(s) ignoré(s)."
                : $"{clientsNouveaux.Count} client(s) généré(s) depuis l'API.";

            await ChargerAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la génération : {ex.InnerException?.Message ?? ex.Message}";
        }
        finally
        {
            ImportEnCours = false;
        }
    }
}
