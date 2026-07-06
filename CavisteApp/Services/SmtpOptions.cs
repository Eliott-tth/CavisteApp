using System;
using System.IO;
using System.Text.Json;

namespace CavisteApp.Services;

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Utilisateur { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
    public string EmailExpediteur { get; set; } = string.Empty;
    public string EmailAdministrateur { get; set; } = string.Empty;
    public bool UtiliserSsl { get; set; } = true;

    public static SmtpOptions LoadFromFile()
    {
        var chemin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (!File.Exists(chemin))
            return new SmtpOptions();

        var json = File.ReadAllText(chemin);
        var options = JsonSerializer.Deserialize<SmtpConfigWrapper>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return options?.Smtp ?? new SmtpOptions();
    }

    private class SmtpConfigWrapper
    {
        public SmtpOptions Smtp { get; set; } = new();
    }
}
