using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Models;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

public partial class LignePanier : ObservableObject
{
    public Vin Vin { get; }

    [ObservableProperty]
    private int quantite;

    public LignePanier(Vin vin, int quantite)
    {
        Vin = vin;
        Quantite = quantite;
    }

    public decimal SousTotal => Vin.Prix * Quantite;

    partial void OnQuantiteChanged(int value) => OnPropertyChanged(nameof(SousTotal));
}

public partial class VenteViewModel : ObservableObject
{
    private readonly VinService _vinService = new();
    private readonly ClientService _clientService = new();
    private readonly VenteService _venteService = new();
    private readonly PdfService _pdfService = new();

    [ObservableProperty] private ObservableCollection<Vin> vinsDisponibles = new();
    [ObservableProperty] private ObservableCollection<Client> clientsDisponibles = new();
    [ObservableProperty] private ObservableCollection<LignePanier> panier = new();
    [ObservableProperty] private ObservableCollection<Vente> historiqueVentes = new();

    [ObservableProperty] private Client? clientSelectionne;
    [ObservableProperty] private Vin? vinSelectionne;
    [ObservableProperty] private int quantiteAAjouter = 1;

    [ObservableProperty] private string messageStatut = string.Empty;
    [ObservableProperty] private string? dernierTicketGenere;

    public decimal MontantTotal => Panier.Sum(l => l.SousTotal);

    public VenteViewModel()
    {
        Panier.CollectionChanged += (_, _) => OnPropertyChanged(nameof(MontantTotal));
        _ = InitialiserAsync();
    }

    private async Task InitialiserAsync()
    {
        var vins = await _vinService.ListerAsync();
        VinsDisponibles = new ObservableCollection<Vin>(vins.Where(v => v.Stock > 0));

        var clients = await _clientService.ListerAsync();
        ClientsDisponibles = new ObservableCollection<Client>(clients);

        var ventes = await _venteService.ListerAsync();
        HistoriqueVentes = new ObservableCollection<Vente>(ventes);
    }

    [RelayCommand]
    private async Task RafraichirAsync() => await InitialiserAsync();

    [RelayCommand]
    private void AjouterAuPanier()
    {
        if (VinSelectionne is null)
        {
            MessageStatut = "Sélectionne un vin.";
            return;
        }
        if (QuantiteAAjouter <= 0)
        {
            MessageStatut = "La quantité doit être supérieure à 0.";
            return;
        }

        var ligneExistante = Panier.FirstOrDefault(l => l.Vin.Id == VinSelectionne.Id);
        var quantiteDejaDansPanier = ligneExistante?.Quantite ?? 0;

        if (quantiteDejaDansPanier + QuantiteAAjouter > VinSelectionne.Stock)
        {
            MessageStatut = $"Stock insuffisant pour '{VinSelectionne.Nom}' (disponible : {VinSelectionne.Stock}).";
            return;
        }

        if (ligneExistante is not null)
            ligneExistante.Quantite += QuantiteAAjouter;
        else
            Panier.Add(new LignePanier(VinSelectionne, QuantiteAAjouter));

        OnPropertyChanged(nameof(MontantTotal));
        QuantiteAAjouter = 1;
        MessageStatut = string.Empty;
    }

    [RelayCommand]
    private void RetirerDuPanier(LignePanier? ligne)
    {
        if (ligne is null) return;
        Panier.Remove(ligne);
    }

    [RelayCommand]
    private void ViderPanier() => Panier.Clear();

    [RelayCommand]
    private async Task ValiderVenteAsync()
    {
        if (ClientSelectionne is null)
        {
            MessageStatut = "Sélectionne un client.";
            return;
        }
        if (Panier.Count == 0)
        {
            MessageStatut = "Le panier est vide.";
            return;
        }

        try
        {
            var lignes = Panier.Select(l => (l.Vin.Id, l.Quantite)).ToList();
            var vente = await _venteService.CreerVenteAsync(ClientSelectionne.Id, lignes);

            DernierTicketGenere = _pdfService.GenererTicket(vente);
            MessageStatut = $"Vente n°{vente.Id} enregistrée. Ticket généré : {DernierTicketGenere}";

            Panier.Clear();
            await InitialiserAsync();
        }
        catch (Exception ex)
        {
            MessageStatut = $"Échec de la vente : {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    [RelayCommand]
    private void OuvrirDernierTicket()
    {
        if (string.IsNullOrEmpty(DernierTicketGenere) || !File.Exists(DernierTicketGenere))
        {
            MessageStatut = "Aucun ticket à ouvrir.";
            return;
        }

        Process.Start(new ProcessStartInfo(DernierTicketGenere) { UseShellExecute = true });
    }
}
