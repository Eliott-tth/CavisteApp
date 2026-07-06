using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

public class CommandeFournisseurService
{
    public async Task<List<CommandeFournisseur>> ListerAsync()
    {
        using var db = new CavisteDbContext();
        return await db.CommandesFournisseur
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes).ThenInclude(l => l.Vin)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync();
    }

    public async Task<CommandeFournisseur> CreerCommandeAsync(int fournisseurId, List<(int VinId, int Quantite)> lignes)
    {
        using var db = new CavisteDbContext();

        var commande = new CommandeFournisseur
        {
            FournisseurId = fournisseurId,
            DateCommande = DateTime.Now,
            Statut = StatutCommande.EnAttente,
            Lignes = lignes.Select(l => new LigneCommandeFournisseur
            {
                VinId = l.VinId,
                QuantiteCommandee = l.Quantite
            }).ToList()
        };

        db.CommandesFournisseur.Add(commande);
        await db.SaveChangesAsync();
        return commande;
    }

    public async Task ValiderAsync(int commandeId)
    {
        using var db = new CavisteDbContext();
        var commande = await db.CommandesFournisseur.FindAsync(commandeId);
        if (commande is not null)
        {
            commande.Statut = StatutCommande.Validee;
            await db.SaveChangesAsync();
        }
    }

    public async Task ReceptionnerAsync(int commandeId, Dictionary<int, int> quantitesRecuesParLigne)
    {
        using var db = new CavisteDbContext();
        var commande = await db.CommandesFournisseur
            .Include(c => c.Lignes).ThenInclude(l => l.Vin)
            .FirstOrDefaultAsync(c => c.Id == commandeId);

        if (commande is null) return;

        foreach (var ligne in commande.Lignes)
        {
            var quantiteRecue = quantitesRecuesParLigne.TryGetValue(ligne.Id, out var q) ? q : ligne.QuantiteCommandee;
            ligne.QuantiteRecue = quantiteRecue;
            ligne.Vin.Stock += quantiteRecue;
        }

        commande.Statut = StatutCommande.Receptionnee;
        commande.DateReception = DateTime.Now;

        await db.SaveChangesAsync();
    }

    public async Task<bool> CommandeEnCoursExistePourVinAsync(int vinId)
    {
        using var db = new CavisteDbContext();
        return await db.CommandesFournisseur
            .Where(c => c.Statut != StatutCommande.Receptionnee)
            .SelectMany(c => c.Lignes)
            .AnyAsync(l => l.VinId == vinId);
    }

    public async Task SupprimerAsync(int commandeId)
    {
        using var db = new CavisteDbContext();
        var commande = await db.CommandesFournisseur.FindAsync(commandeId);
        if (commande is not null)
        {
            db.CommandesFournisseur.Remove(commande);
            await db.SaveChangesAsync();
        }
    }
}
