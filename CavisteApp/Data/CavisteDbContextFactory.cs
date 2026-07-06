using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CavisteApp.Data;

/// <summary>
/// Permet à l'outil "dotnet ef migrations" de créer une instance du contexte
/// sans avoir besoin de démarrer l'application WPF complète.
/// </summary>
public class CavisteDbContextFactory : IDesignTimeDbContextFactory<CavisteDbContext>
{
    public CavisteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CavisteDbContext>();
        return new CavisteDbContext();
    }
}
