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

/// <summary>
/// Permet à n'importe quelle page (typiquement la page d'accueil) de demander
/// à MainWindow de basculer sur un autre onglet, sans lien direct entre les
/// ViewModels et la fenêtre principale.
/// </summary>
public class NavigationService
{
    public static NavigationService Instance { get; } = new();

    public event Action<SectionApplication>? NavigationDemandee;

    private NavigationService() { }

    public void NaviguerVers(SectionApplication section) => NavigationDemandee?.Invoke(section);
}
