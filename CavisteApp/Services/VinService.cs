using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

/// <summary>
/// CRUD sur l'entité Vin réalisé via EF Core (ORM). Toute modification qui
/// touche au stock déclenche automatiquement la vérification d'alerte. La
/// "suppression" est une suppression douce (EstSupprime) pour ne jamais casser
/// l'historique des ventes/commandes qui référencent déjà le vin.
/// </summary>
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

    /// <summary>
    /// Retire le vin du catalogue. Si aucune vente/commande ne le référence,
    /// il est vraiment effacé ; sinon (contrainte FOREIGN KEY), il est
    /// simplement masqué (EstSupprime = true) pour préserver l'historique.
    /// </summary>
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

    /// <summary>Ajuste le stock d'un vin (utilisé par la vente et la réception de commande).</summary>
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

    /// <summary>Retourne les vins actuellement sous leur seuil de stock bas.</summary>
    public async Task<List<Vin>> ListerVinsEnAlerteAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Vins.Where(v => !v.EstSupprime && v.Stock <= v.SeuilBas).ToListAsync();
    }
}
