using System.Windows;
using CavisteApp.Services;

namespace CavisteApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NavigationService.Instance.NavigationDemandee += SurNavigationDemandee;
        Closed += (_, _) => NavigationService.Instance.NavigationDemandee -= SurNavigationDemandee;
    }

    private void SurNavigationDemandee(SectionApplication section)
    {
        OngletsPrincipaux.SelectedIndex = section switch
        {
            SectionApplication.Accueil => 0,
            SectionApplication.TableauDeBord => 1,
            SectionApplication.Vente => 2,
            SectionApplication.Stock => 3,
            SectionApplication.Clients => 4,
            SectionApplication.Fournisseurs => 5,
            SectionApplication.AlertesCommandes => 6,
            SectionApplication.Generation => 7,
            SectionApplication.Utilisateurs => 8,
            _ => 0
        };
    }
}
