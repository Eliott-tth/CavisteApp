using System.Collections.ObjectModel;
using CavisteApp.Models;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

/// <summary>
/// Gère la liste des vins, le formulaire d'édition et l'import depuis l'API
/// distante. Le CRUD passe par <see cref="VinService"/> (EF Core / ORM).
/// </summary>
public partial class VinListViewModel : ObservableObject
{
    private readonly VinService _vinService = new();
    private readonly WineApiService _wineApiService = new();

    [ObservableProperty]
    private ObservableCollection<Vin> vins = new();

    [ObservableProperty]
    private Vin? vinSelectionne;

    // --- Champs du formulaire (édition / création) ---
    [ObservableProperty] private int vinId;
    [ObservableProperty] private string nom = string.Empty;
    [ObservableProperty] private TypeVin typeSelectionne = TypeVin.Rouge;
    [ObservableProperty] private decimal prix;
    [ObservableProperty] private int stock;
    [ObservableProperty] private int seuilBas = 5;

    [ObservableProperty] private string messageStatut = string.Empty;
    [ObservableProperty] private bool importEnCours;

    public Array TypesDisponibles => Enum.GetValues(typeof(TypeVin));

    public VinListViewModel()
    {
        _ = ChargerAsync();
    }

    partial void OnVinSelectionneChanged(Vin? value)
    {
        if (value is null)
        {
            ReinitialiserFormulaire();
            return;
        }

        VinId = value.Id;
        Nom = value.Nom;
        TypeSelectionne = value.Type;
        Prix = value.Prix;
        Stock = value.Stock;
        SeuilBas = value.SeuilBas;
    }

    [RelayCommand]
    private async Task ChargerAsync()
    {
        var liste = await _vinService.ListerAsync();
        Vins = new ObservableCollection<Vin>(liste);
    }

    [RelayCommand]
    private void Nouveau()
    {
        VinSelectionne = null;
        ReinitialiserFormulaire();
    }

    [RelayCommand]
    private async Task EnregistrerAsync()
    {
        if (string.IsNullOrWhiteSpace(Nom))
        {
            MessageStatut = "Le nom du vin est obligatoire.";
            return;
        }

        if (VinId == 0)
        {
            var nouveau = new Vin
            {
                Nom = Nom,
                Type = TypeSelectionne,
                Prix = Prix,
                Stock = Stock,
                SeuilBas = SeuilBas
            };
            await _vinService.AjouterAsync(nouveau);
            MessageStatut = $"Vin '{Nom}' ajouté.";
        }
        else
        {
            var modifie = new Vin
            {
                Id = VinId,
                Nom = Nom,
                Type = TypeSelectionne,
                Prix = Prix,
                Stock = Stock,
                SeuilBas = SeuilBas
            };
            await _vinService.ModifierAsync(modifie);
            MessageStatut = $"Vin '{Nom}' modifié.";
        }

        await ChargerAsync();
        Nouveau();
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (VinSelectionne is null) return;

        await _vinService.SupprimerAsync(VinSelectionne.Id);
        MessageStatut = $"Vin '{VinSelectionne.Nom}' supprimé.";
        await ChargerAsync();
        Nouveau();
    }

    /// <summary>
    /// Importe une vingtaine de vins de la catégorie sélectionnée depuis
    /// l'API distante et les insère en base (voir WineApiService pour la veille).
    /// </summary>
    [RelayCommand]
    private async Task ImporterDepuisApiAsync()
    {
        try
        {
            ImportEnCours = true;
            MessageStatut = "Import en cours depuis l'API...";

            var vinsImportes = await _wineApiService.RecupererVinsAsync(TypeSelectionne, 20);
            await _vinService.AjouterPlusieursAsync(vinsImportes);

            MessageStatut = $"{vinsImportes.Count} vin(s) importé(s) depuis l'API.";
            await ChargerAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de l'import API : {ex.Message}";
        }
        finally
        {
            ImportEnCours = false;
        }
    }

    private void ReinitialiserFormulaire()
    {
        VinId = 0;
        Nom = string.Empty;
        TypeSelectionne = TypeVin.Rouge;
        Prix = 0;
        Stock = 0;
        SeuilBas = 5;
    }
}
