using System.Windows;
using CavisteApp.Services;

namespace CavisteApp.Views;

public partial class RegisterWindow : Window
{
    private readonly UtilisateurService _utilisateurService = new();
    private string _emailEnCours = string.Empty;

    public RegisterWindow()
    {
        InitializeComponent();
    }

    private async void BoutonInscrire_Click(object sender, RoutedEventArgs e)
    {
        var email = ChampEmail.Text.Trim();
        var motDePasse = ChampMotDePasse.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse))
        {
            TexteMessage.Text = "Renseigne un email et un mot de passe.";
            return;
        }

        var resultat = await _utilisateurService.InscrireAsync(email, motDePasse);
        TexteMessage.Text = resultat.Message;

        if (resultat.Succes)
        {
            _emailEnCours = email;
            PanelInscription.Visibility = Visibility.Collapsed;
            PanelConfirmation.Visibility = Visibility.Visible;
        }
    }

    private async void BoutonConfirmer_Click(object sender, RoutedEventArgs e)
    {
        var code = ChampCode.Text.Trim();
        var resultat = await _utilisateurService.ConfirmerCompteAsync(_emailEnCours, code);
        TexteMessage.Text = resultat.Message;

        if (resultat.Succes)
        {
            MessageBox.Show("Compte confirmé, tu peux maintenant te connecter.", "Succès");
            Close();
        }
    }
}
