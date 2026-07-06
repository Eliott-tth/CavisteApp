using System.Windows;
using CavisteApp.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace CavisteApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        QuestPDF.Settings.License = LicenseType.Community;

        // Applique les migrations en attente (crée le fichier caviste.db si besoin).
        using var db = new CavisteDbContext();
        db.Database.Migrate();
    }
}