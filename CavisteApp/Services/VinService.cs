using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

/// <summary>
/// CRUD sur l'entité Vin réalisé via EF Core (ORM), conformément à
/// l'exigence "fonctions CRUD réalisées à l'aide d'un ORM" du cahier des charges.
/// </summary>
public class VinService
{
    public async Task<List<Vin>> ListerAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Vins
            .Include(v => v.Fournisseur)
            .OrderBy(v => v.Nom)
            .ToListAsync();
    }

    public async Task<Vin> AjouterAsync(Vin vin)
    {
        using var db = new CavisteDbContext();
        db.Vins.Add(vin);
        await db.SaveChangesAsync();
        return vin;
    }

    public async Task AjouterPlusieursAsync(IEnumerable<Vin> vins)
    {
        using var db = new CavisteDbContext();
        db.Vins.AddRange(vins);
        await db.SaveChangesAsync();
    }

    public async Task ModifierAsync(Vin vin)
    {
        using var db = new CavisteDbContext();
        db.Vins.Update(vin);
        await db.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        using var db = new CavisteDbContext();
        var vin = await db.Vins.FindAsync(id);
        if (vin is not null)
        {
            db.Vins.Remove(vin);
            await db.SaveChangesAsync();
        }
    }

    /// <summary>Ajuste le stock d'un vin (utilisé par la vente et la réception de commande).</summary>
    public async Task AjusterStockAsync(int vinId, int quantiteDelta)
    {
        using var db = new CavisteDbContext();
        var vin = await db.Vins.FindAsync(vinId);
        if (vin is not null)
        {
            vin.Stock += quantiteDelta;
            await db.SaveChangesAsync();
        }
    }

    /// <summary>Retourne les vins actuellement sous leur seuil de stock bas.</summary>
    public async Task<List<Vin>> ListerVinsEnAlerteAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Vins.Where(v => v.Stock <= v.SeuilBas).ToListAsync();
    }
}