using System.Windows;
using CavisteApp.Services;

namespace CavisteApp.Views;

public partial class LoginWindow : Window
{
    private readonly UtilisateurService _utilisateurService = new();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void BoutonConnexion_Click(object sender, RoutedEventArgs e)
    {
        var email = ChampEmail.Text.Trim();
        var motDePasse = ChampMotDePasse.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse))
        {
            TexteMessage.Text = "Renseigne ton email et ton mot de passe.";
            return;
        }

        var resultat = await _utilisateurService.ConnecterAsync(email, motDePasse);
        if (!resultat.Succes)
        {
            TexteMessage.Text = resultat.Message;
            return;
        }

        SessionContext.Instance.UtilisateurConnecte = resultat.Utilisateur;
        DialogResult = true;
        Close();
    }

    private void BoutonCreerCompte_Click(object sender, RoutedEventArgs e)
    {
        new RegisterWindow().ShowDialog();
    }

    private void BoutonMotDePasseOublie_Click(object sender, RoutedEventArgs e)
    {
        new ForgotPasswordWindow().ShowDialog();
    }
}
