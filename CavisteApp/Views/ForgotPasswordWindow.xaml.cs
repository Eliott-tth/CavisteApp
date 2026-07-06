using System.Windows;
using CavisteApp.Services;

namespace CavisteApp.Views;

public partial class ForgotPasswordWindow : Window
{
    private readonly UtilisateurService _utilisateurService = new();
    private string _emailEnCours = string.Empty;

    public ForgotPasswordWindow()
    {
        InitializeComponent();
    }

    private async void BoutonEnvoyer_Click(object sender, RoutedEventArgs e)
    {
        var email = ChampEmail.Text.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            TexteMessage.Text = "Renseigne ton email.";
            return;
        }

        var resultat = await _utilisateurService.DemanderReinitialisationAsync(email);
        TexteMessage.Text = resultat.Message;

        if (resultat.Succes)
        {
            _emailEnCours = email;
            PanelDemande.Visibility = Visibility.Collapsed;
            PanelReinitialisation.Visibility = Visibility.Visible;
        }
    }

    private async void BoutonReinitialiser_Click(object sender, RoutedEventArgs e)
    {
        var code = ChampCode.Text.Trim();
        var nouveauMotDePasse = ChampNouveauMotDePasse.Password;

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(nouveauMotDePasse))
        {
            TexteMessage.Text = "Renseigne le code et le nouveau mot de passe.";
            return;
        }

        var resultat = await _utilisateurService.ReinitialiserMotDePasseAsync(_emailEnCours, code, nouveauMotDePasse);
        TexteMessage.Text = resultat.Message;

        if (resultat.Succes)
        {
            MessageBox.Show("Mot de passe réinitialisé, tu peux te connecter.", "Succès");
            Close();
        }
    }
}
