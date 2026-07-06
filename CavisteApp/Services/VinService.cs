using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

public class VinService
{
    private readonly AlerteAutomatiqueService _alerteService = new();

    public async Task<List<Vin>> ListerAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Vins
            .Include(v => v.Fournisseur)
            .Where(v => !v.EstSupprime)
            .OrderBy(v => v.Nom)
            .ToListAsync();
    }

    public async Task<Vin> AjouterAsync(Vin vin)
    {
        using var db = new CavisteDbContext();
        db.Vins.Add(vin);
        await db.SaveChangesAsync();
        await _alerteService.VerifierApresModificationStockAsync(vin);
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
        await _alerteService.VerifierApresModificationStockAsync(vin);
    }

    public async Task SupprimerAsync(int id)
    {
        using var db = new CavisteDbContext();
        var vin = await db.Vins.FindAsync(id);
        if (vin is null) return;

        try
        {
            db.Vins.Remove(vin);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.Entry(vin).State = EntityState.Unchanged;
            vin.EstSupprime = true;
            await db.SaveChangesAsync();
        }
    }

    public async Task AjusterStockAsync(int vinId, int quantiteDelta)
    {
        using var db = new CavisteDbContext();
        var vin = await db.Vins.FindAsync(vinId);
        if (vin is not null)
        {
            vin.Stock += quantiteDelta;
            await db.SaveChangesAsync();
            await _alerteService.VerifierApresModificationStockAsync(vin);
        }
    }

    public async Task<List<Vin>> ListerVinsEnAlerteAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Vins.Where(v => !v.EstSupprime && v.Stock <= v.SeuilBas).ToListAsync();
    }
}
