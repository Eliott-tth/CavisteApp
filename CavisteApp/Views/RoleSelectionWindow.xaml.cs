using System.Windows;
using CavisteApp.Services;

namespace CavisteApp.Views;

public partial class RoleSelectionWindow : Window
{
    // Mot de passe administrateur simple, à des fins de démonstration du contrôle d'accès par rôle.
    private const string MotDePasseAdmin = "admin123";

    public RoleSelectionWindow()
    {
        InitializeComponent();
    }

    private void RadioRole_Checked(object sender, RoutedEventArgs e)
    {
        if (PanelMotDePasse is null) return;
        PanelMotDePasse.Visibility = RadioAdministrateur.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void BoutonEntrer_Click(object sender, RoutedEventArgs e)
    {
        if (RadioAdministrateur.IsChecked == true)
        {
            if (ChampMotDePasse.Password != MotDePasseAdmin)
            {
                TexteErreur.Text = "Mot de passe incorrect.";
                return;
            }
            SessionContext.Instance.EstAdministrateur = true;
        }
        else
        {
            SessionContext.Instance.EstAdministrateur = false;
        }

        DialogResult = true;
        Close();
    }
}
