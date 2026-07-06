using System;
using System.Threading.Tasks;
using CavisteApp.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CavisteApp.Services;

/// <summary>
/// Envoie une alerte email à l'administrateur lorsqu'un vin passe sous son
/// seuil de stock bas, conformément au cahier des charges. Les paramètres
/// SMTP sont lus depuis appsettings.json (voir appsettings.example.json).
/// </summary>
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

    /// <summary>
    /// Envoie une alerte pour un vin sous son seuil. Contient le nom du vin,
    /// la quantité en stock, le seuil configuré, et l'indication qu'une
    /// commande fournisseur est nécessaire (cf. cahier des charges).
    /// </summary>
    public async Task EnvoyerAlerteStockBasAsync(Vin vin)
    {
        if (!EstConfigure)
            throw new InvalidOperationException(
                "Le SMTP n'est pas configuré. Copie appsettings.example.json vers appsettings.json et renseigne tes identifiants.");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.EmailExpediteur));
        message.To.Add(MailboxAddress.Parse(_options.EmailAdministrateur));
        message.Subject = $"[Caviste] Alerte stock bas : {vin.Nom}";

        message.Body = new TextPart("plain")
        {
            Text = $"""
                Alerte de stock bas

                Vin : {vin.Nom} ({vin.Type})
                Stock actuel : {vin.Stock} bouteille(s)
                Seuil configuré : {vin.SeuilBas} bouteille(s)

                Une commande fournisseur est nécessaire pour réapprovisionner cette référence.
                """
        };

        using var client = new SmtpClient();
        var securite = _options.UtiliserSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        await client.ConnectAsync(_options.Host, _options.Port, securite);
        await client.AuthenticateAsync(_options.Utilisateur, _options.MotDePasse);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
