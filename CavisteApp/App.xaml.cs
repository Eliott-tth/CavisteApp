using System.Windows;
using CavisteApp.Data;
using CavisteApp.Services;
using CavisteApp.Views;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace CavisteApp;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        QuestPDF.Settings.License = LicenseType.Community;

        using (var db = new CavisteDbContext())
        {
            db.Database.Migrate();
        }

        await new UtilisateurService().AssurerAdminParDefautAsync();

        var connexion = new LoginWindow();
        var resultat = connexion.ShowDialog();

        if (resultat != true)
        {
            Shutdown();
            return;
        }

        var fenetrePrincipale = new MainWindow();
        MainWindow = fenetrePrincipale;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        fenetrePrincipale.Show();
    }
}
