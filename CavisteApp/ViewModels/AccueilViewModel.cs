using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

/// <summary>
/// Page d'accueil affichée après connexion : message de bienvenue et accès
/// rapide à toutes les catégories de l'application.
/// </summary>
public partial class AccueilViewModel : ObservableObject
{
    public bool EstAdministrateur => SessionContext.Instance.EstAdministrateur;

    [RelayCommand]
    private void Naviguer(SectionApplication section) => NavigationService.Instance.NaviguerVers(section);
}
