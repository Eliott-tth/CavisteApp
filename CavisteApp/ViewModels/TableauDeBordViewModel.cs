using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Models;
using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

/// <summary>Page d'accueil : indicateurs clés, alertes de stock bas et ventes récentes.</summary>
public partial class TableauDeBordViewModel : ObservableObject
{
    private readonly VinService _vinService = new();
    private readonly VenteService _venteService = new();

    [ObservableProperty] private int nombreVins;
    [ObservableProperty] private int stockTotal;
    [ObservableProperty] private int ventesDuMois;
    [ObservableProperty] private int nombreAlertes;

    [ObservableProperty] private ObservableCollection<Vin> vinsEnAlerte = new();
    [ObservableProperty] private ObservableCollection<Vente> ventesRecentes = new();

    public TableauDeBordViewModel()
    {
        _ = ChargerAsync();
    }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        var vins = await _vinService.ListerAsync();
        NombreVins = vins.Count;
        StockTotal = vins.Sum(v => v.Stock);

        var alertes = vins.Where(v => v.EstEnAlerte).ToList();
        NombreAlertes = alertes.Count;
        VinsEnAlerte = new ObservableCollection<Vin>(alertes.Take(5));

        var ventes = await _venteService.ListerAsync();
        VentesDuMois = ventes.Count(v => v.DateVente.Month == DateTime.Now.Month && v.DateVente.Year == DateTime.Now.Year);
        VentesRecentes = new ObservableCollection<Vente>(ventes.Take(5));
    }
}
