using System.Collections.ObjectModel;
using System.Linq;
using CavisteApp.Models;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

public partial class FournisseurListViewModel : ObservableObject
{
    private readonly FournisseurService _fournisseurService = new();

    [ObservableProperty] private ObservableCollection<Fournisseur> fournisseurs = new();
    [ObservableProperty] private Fournisseur? fournisseurSelectionne;

    [ObservableProperty] private int fournisseurId;
    [ObservableProperty] private string nom = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string? telephone;
    [ObservableProperty] private string? adresse;

    [ObservableProperty] private string messageStatut = string.Empty;

    public FournisseurListViewModel()
    {
        _ = ChargerAsync();
    }

    partial void OnFournisseurSelectionneChanged(Fournisseur? value)
    {
        if (value is null)
        {
            ReinitialiserFormulaire();
            return;
        }

        FournisseurId = value.Id;
        Nom = value.Nom;
        Email = value.Email;
        Telephone = value.Telephone;
        Adresse = value.Adresse;
    }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        var liste = await _fournisseurService.ListerAsync();
        Fournisseurs = new ObservableCollection<Fournisseur>(liste);
    }

    [RelayCommand]
    private void Nouveau()
    {
        FournisseurSelectionne = null;
        ReinitialiserFormulaire();
    }

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Email))
        {
            MessageStatut = "Nom et email sont obligatoires.";
            return;
        }

        try
        {
            int idSauvegarde;

            if (FournisseurId == 0)
            {
                var nouveau = new Fournisseur { Nom = Nom, Email = Email, Telephone = Telephone, Adresse = Adresse };
                var ajoute = await _fournisseurService.AjouterAsync(nouveau);
                idSauvegarde = ajoute.Id;
                MessageStatut = $"Fournisseur '{Nom}' ajouté.";
            }
            else
            {
                var modifie = new Fournisseur { Id = FournisseurId, Nom = Nom, Email = Email, Telephone = Telephone, Adresse = Adresse };
                await _fournisseurService.ModifierAsync(modifie);
                idSauvegarde = FournisseurId;
                MessageStatut = $"Fournisseur '{Nom}' modifié.";
            }

            await ChargerAsync();
            FournisseurSelectionne = Fournisseurs.FirstOrDefault(f => f.Id == idSauvegarde);
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de l'enregistrement : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (FournisseurSelectionne is null) return;

        try
        {
            await _fournisseurService.SupprimerAsync(FournisseurSelectionne.Id);
            MessageStatut = $"Fournisseur '{FournisseurSelectionne.Nom}' supprimé.";
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
        FournisseurId = 0;
        Nom = string.Empty;
        Email = string.Empty;
        Telephone = null;
        Adresse = null;
    }
}