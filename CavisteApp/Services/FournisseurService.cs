using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

public class FournisseurService
{
    private readonly Random _random = new();

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

        var candidats = await db.Vins.Where(v => !v.EstSupprime && v.FournisseurId == null).ToListAsync();
        if (candidats.Count == 0)
            candidats = await db.Vins.Where(v => !v.EstSupprime).ToListAsync();

        if (candidats.Count > 0)
        {
            var nombre = _random.Next(3, Math.Min(9, candidats.Count) + 1);
            var choisis = candidats.OrderBy(v => _random.Next()).Take(nombre).ToList();
            foreach (var vin in choisis)
                vin.FournisseurId = fournisseur.Id;
            await db.SaveChangesAsync();
        }

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
