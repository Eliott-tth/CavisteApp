using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CavisteApp.Models;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

/// <summary>
/// Regroupe la vue "Alertes" (vins sous leur seuil, envoi d'email, création de
/// commande) et le suivi des commandes fournisseur (validation, réception).
/// </summary>
public partial class AlerteCommandeViewModel : ObservableObject
{
    private readonly VinService _vinService = new();
    private readonly FournisseurService _fournisseurService = new();
    private readonly CommandeFournisseurService _commandeService = new();
    private readonly EmailService _emailService = new();

    [ObservableProperty] private ObservableCollection<Vin> vinsEnAlerte = new();
    [ObservableProperty] private ObservableCollection<CommandeFournisseur> commandes = new();
    [ObservableProperty] private ObservableCollection<Fournisseur> fournisseursDisponibles = new();

    [ObservableProperty] private string messageStatut = string.Empty;

    public AlerteCommandeViewModel()
    {
        _ = InitialiserAsync();
    }

    private async Task InitialiserAsync()
    {
        var fournisseurs = await _fournisseurService.ListerAsync();
        FournisseursDisponibles = new ObservableCollection<Fournisseur>(fournisseurs);
        await RafraichirAsync();
    }

    [RelayCommand]
    public async Task RafraichirAsync()
    {
        var vins = await _vinService.ListerVinsEnAlerteAsync();
        VinsEnAlerte = new ObservableCollection<Vin>(vins);

        var commandesExistantes = await _commandeService.ListerAsync();
        Commandes = new ObservableCollection<CommandeFournisseur>(commandesExistantes);
    }

    /// <summary>
    /// Crée une commande fournisseur pour reconstituer le stock du vin
    /// jusqu'au double de son seuil (règle simple et modifiable). Utilise le
    /// fournisseur associé au vin ; échoue proprement si aucun fournisseur
    /// n'est renseigné sur la fiche vin.
    /// </summary>
    [RelayCommand]
    private async Task CommanderAsync(Vin? vin)
    {
        if (vin is null) return;

        if (vin.FournisseurId is null)
        {
            MessageStatut = $"Aucun fournisseur associé à '{vin.Nom}'. Renseigne-le dans l'onglet Stock.";
            return;
        }

        try
        {
            var quantiteACommander = Math.Max(vin.SeuilBas * 2 - vin.Stock, vin.SeuilBas);
            await _commandeService.CreerCommandeAsync(vin.FournisseurId.Value,
                new List<(int VinId, int Quantite)> { (vin.Id, quantiteACommander) });

            MessageStatut = $"Commande créée pour '{vin.Nom}' ({quantiteACommander} bouteille(s)).";
            await RafraichirAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la création de commande : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    /// <summary>Envoie l'email d'alerte pour un vin sous son seuil (voir EmailService).</summary>
    [RelayCommand]
    private async Task EnvoyerAlerteEmailAsync(Vin? vin)
    {
        if (vin is null) return;

        try
        {
            await _emailService.EnvoyerAlerteStockBasAsync(vin);
            MessageStatut = $"Email d'alerte envoyé pour '{vin.Nom}'.";
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de l'envoi de l'email : {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ValiderCommandeAsync(CommandeFournisseur? commande)
    {
        if (commande is null) return;

        try
        {
            await _commandeService.ValiderAsync(commande.Id);
            MessageStatut = $"Commande #{commande.Id} validée.";
            await RafraichirAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la validation : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    /// <summary>
    /// Réceptionne intégralement la commande (quantité reçue = quantité
    /// commandée pour chaque ligne). Incrémente le stock des vins concernés.
    /// </summary>
    [RelayCommand]
    private async Task ReceptionnerCommandeAsync(CommandeFournisseur? commande)
    {
        if (commande is null) return;

        try
        {
            await _commandeService.ReceptionnerAsync(commande.Id, new Dictionary<int, int>());
            MessageStatut = $"Commande #{commande.Id} réceptionnée, stock mis à jour.";
            await RafraichirAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la réception : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SupprimerCommandeAsync(CommandeFournisseur? commande)
    {
        if (commande is null) return;

        try
        {
            await _commandeService.SupprimerAsync(commande.Id);
            MessageStatut = $"Commande #{commande.Id} supprimée.";
            await RafraichirAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la suppression : {ex.InnerException?.Message ?? ex.Message}";
        }
    }
}