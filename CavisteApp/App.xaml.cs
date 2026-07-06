using System.Windows;
using CavisteApp.Data;
using CavisteApp.Views;
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
        using (var db = new CavisteDbContext())
        {
            db.Database.Migrate();
        }

        // Écran de connexion : fixe le rôle (visiteur/administrateur) avant
        // d'ouvrir la fenêtre principale (contrôle d'accès par rôle).
        var connexion = new RoleSelectionWindow();
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
