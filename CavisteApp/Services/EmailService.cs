using System;
using System.Threading.Tasks;
using CavisteApp.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CavisteApp.Services;

public class EmailService
{
    private readonly SmtpOptions _options;

    public EmailService()
    {
        _options = SmtpOptions.LoadFromFile();
    }

    public bool EstConfigure =>
        !string.IsNullOrWhiteSpace(_options.Host) &&
        !string.IsNullOrWhiteSpace(_options.EmailAdministrateur);

    public async Task EnvoyerAlerteStockBasAsync(Vin vin)
    {
        var corps = $"""
            Alerte de stock bas

            Vin : {vin.Nom} ({vin.Type})
            Stock actuel : {vin.Stock} bouteille(s)
            Seuil configuré : {vin.SeuilBas} bouteille(s)

            Une commande fournisseur est nécessaire pour réapprovisionner cette référence.
            """;

        await EnvoyerAsync(_options.EmailAdministrateur, $"[Caviste] Alerte stock bas : {vin.Nom}", corps);
    }

    public async Task EnvoyerCodeConfirmationAsync(string email, string code)
    {
        var corps = $"""
            Bienvenue sur Cave du Sommelier !

            Voici ton code de confirmation de compte : {code}

            Ce code expire dans 30 minutes.
            """;

        await EnvoyerAsync(email, "Confirme ton compte - Cave du Sommelier", corps);
    }

    public async Task EnvoyerCodeReinitialisationAsync(string email, string code)
    {
        var corps = $"""
            Tu as demandé la réinitialisation de ton mot de passe.

            Voici ton code de réinitialisation : {code}

            Ce code expire dans 30 minutes. Si tu n'es pas à l'origine de cette demande, ignore cet email.
            """;

        await EnvoyerAsync(email, "Réinitialisation de mot de passe - Cave du Sommelier", corps);
    }

    private async Task EnvoyerAsync(string destinataire, string sujet, string corps)
    {
        if (!EstConfigure)
            throw new InvalidOperationException(
                "Le SMTP n'est pas configuré. Copie appsettings.example.json vers appsettings.json et renseigne tes identifiants.");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.EmailExpediteur));
        message.To.Add(MailboxAddress.Parse(destinataire));
        message.Subject = sujet;
        message.Body = new TextPart("plain") { Text = corps };

        using var client = new SmtpClient();
        var securite = _options.UtiliserSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_options.Host, _options.Port, securite);
        await client.AuthenticateAsync(_options.Utilisateur, _options.MotDePasse);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
