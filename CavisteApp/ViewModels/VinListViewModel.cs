using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly FournisseurService _fournisseurService = new();

    [ObservableProperty]
    private ObservableCollection<Vin> vins = new();

    [ObservableProperty]
    private Vin? vinSelectionne;

    [ObservableProperty]
    private ObservableCollection<Fournisseur> fournisseursDisponibles = new();

    // --- Champs du formulaire (édition / création) ---
    [ObservableProperty] private int vinId;
    [ObservableProperty] private string nom = string.Empty;
    [ObservableProperty] private TypeVin typeSelectionne = TypeVin.Rouge;
    [ObservableProperty] private decimal prix;
    [ObservableProperty] private int stock;
    [ObservableProperty] private int seuilBas = 5;
    [ObservableProperty] private Fournisseur? fournisseurSelectionne;

    [ObservableProperty] private string messageStatut = string.Empty;
    [ObservableProperty] private bool importEnCours;

    public Array TypesDisponibles => Enum.GetValues(typeof(TypeVin));

    public VinListViewModel()
    {
        _ = InitialiserAsync();
    }

    private async Task InitialiserAsync()
    {
        await RafraichirFournisseursAsync();
        await ChargerAsync();
    }

    /// <summary>Recharge la liste des fournisseurs (ex : au moment d'ouvrir le ComboBox), utile car cette
    /// vue est instanciée une seule fois au démarrage et peut donc avoir une liste obsolète.</summary>
    public async Task RafraichirFournisseursAsync()
    {
        var fournisseurIdActuel = FournisseurSelectionne?.Id;
        var fournisseurs = await _fournisseurService.ListerAsync();
        FournisseursDisponibles = new ObservableCollection<Fournisseur>(fournisseurs);
        if (fournisseurIdActuel is not null)
            FournisseurSelectionne = FournisseursDisponibles.FirstOrDefault(f => f.Id == fournisseurIdActuel);
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
        FournisseurSelectionne = FournisseursDisponibles.FirstOrDefault(f => f.Id == value.FournisseurId);
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

        try
        {
            if (VinId == 0)
            {
                var nouveau = new Vin
                {
                    Nom = Nom,
                    Type = TypeSelectionne,
                    Prix = Prix,
                    Stock = Stock,
                    SeuilBas = SeuilBas,
                    FournisseurId = FournisseurSelectionne?.Id
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
                    SeuilBas = SeuilBas,
                    FournisseurId = FournisseurSelectionne?.Id
                };
                await _vinService.ModifierAsync(modifie);
                MessageStatut = $"Vin '{Nom}' modifié.";
            }

            await ChargerAsync();
            Nouveau();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de l'enregistrement : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        if (VinSelectionne is null) return;

        try
        {
            await _vinService.SupprimerAsync(VinSelectionne.Id);
            MessageStatut = $"Vin '{VinSelectionne.Nom}' supprimé.";
            await ChargerAsync();
            Nouveau();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la suppression : {ex.InnerException?.Message ?? ex.Message}";
        }
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

            var nomsExistants = Vins.Select(v => v.Nom).ToHashSet();
            var vinsNouveaux = vinsImportes.Where(v => !nomsExistants.Contains(v.Nom)).ToList();

            await _vinService.AjouterPlusieursAsync(vinsNouveaux);

            var ignores = vinsImportes.Count - vinsNouveaux.Count;
            MessageStatut = ignores > 0
                ? $"{vinsNouveaux.Count} vin(s) importé(s), {ignores} déjà présent(s) ignoré(s)."
                : $"{vinsNouveaux.Count} vin(s) importé(s) depuis l'API.";
            await ChargerAsync();
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            MessageStatut = $"Échec de l'import API : {detail}";
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
        FournisseurSelectionne = null;
    }
}
