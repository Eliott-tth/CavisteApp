using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CavisteApp.Models;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

public partial class UtilisateurListViewModel : ObservableObject
{
    private readonly UtilisateurService _utilisateurService = new();

    [ObservableProperty] private ObservableCollection<Utilisateur> utilisateurs = new();
    [ObservableProperty] private string messageStatut = string.Empty;

    public UtilisateurListViewModel()
    {
        _ = ChargerAsync();
    }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        var liste = await _utilisateurService.ListerAsync();
        Utilisateurs = new ObservableCollection<Utilisateur>(liste);
    }

    [RelayCommand]
    private async Task BasculerRoleAsync(Utilisateur? utilisateur)
    {
        if (utilisateur is null) return;

        var nouveauRole = utilisateur.Role == RoleUtilisateur.Administrateur
            ? RoleUtilisateur.Visiteur
            : RoleUtilisateur.Administrateur;

        await _utilisateurService.ModifierRoleAsync(utilisateur.Id, nouveauRole);
        MessageStatut = $"Rôle de {utilisateur.Email} changé en {nouveauRole}.";
        await ChargerAsync();
    }

    [RelayCommand]
    private async Task SupprimerAsync(Utilisateur? utilisateur)
    {
        if (utilisateur is null) return;

        await _utilisateurService.SupprimerAsync(utilisateur.Id);
        MessageStatut = $"Utilisateur {utilisateur.Email} supprimé.";
        await ChargerAsync();
    }
}
