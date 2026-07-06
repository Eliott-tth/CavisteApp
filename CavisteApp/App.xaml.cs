using System.Windows;
using CavisteApp.Data;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Applique les migrations en attente (crée le fichier caviste.db si besoin).
        using var db = new CavisteDbContext();
        db.Database.Migrate();
    }
}
