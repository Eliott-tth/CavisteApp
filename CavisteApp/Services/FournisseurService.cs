using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

/// <summary>CRUD sur l'entité Fournisseur, réalisé via EF Core (ORM).</summary>
public class FournisseurService
{
    public async Task<List<Fournisseur>> ListerAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Fournisseurs.OrderBy(f => f.Nom).ToListAsync();
    }

    public async Task<Fournisseur> AjouterAsync(Fournisseur fournisseur)
    {
        using var db = new CavisteDbContext();
        db.Fournisseurs.Add(fournisseur);
        await db.SaveChangesAsync();
        return fournisseur;
    }

    public async Task ModifierAsync(Fournisseur fournisseur)
    {
        using var db = new CavisteDbContext();
        db.Fournisseurs.Update(fournisseur);
        await db.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        using var db = new CavisteDbContext();
        var fournisseur = await db.Fournisseurs.FindAsync(id);
        if (fournisseur is not null)
        {
            db.Fournisseurs.Remove(fournisseur);
            await db.SaveChangesAsync();
        }
    }
}
