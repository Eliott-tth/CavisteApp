using System;

namespace CavisteApp.Services;

public enum SectionApplication
{
    Accueil,
    TableauDeBord,
    Vente,
    Stock,
    Clients,
    Fournisseurs,
    AlertesCommandes,
    Generation,
    Utilisateurs
}

public class NavigationService
{
    public static NavigationService Instance { get; } = new();

    public event Action<SectionApplication>? NavigationDemandee;

    private NavigationService() { }

    public void NaviguerVers(SectionApplication section) => NavigationDemandee?.Invoke(section);
}
