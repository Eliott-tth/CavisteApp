using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

/// <summary>
/// Gère la création des ventes : vérifie le stock disponible, décrémente le
/// stock des vins vendus, et persiste la vente avec ses lignes (EF Core).
/// </summary>
public class VenteService
{
    public async Task<List<Vente>> ListerAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Ventes
            .Include(v => v.Client)
            .Include(v => v.Lignes).ThenInclude(l => l.Vin)
            .OrderByDescending(v => v.DateVente)
            .ToListAsync();
    }

    /// <summary>
    /// Crée une vente pour un client avec une liste de lignes (vin, quantité).
    /// Lève une exception si le stock est insuffisant pour un des vins : dans ce
    /// cas, rien n'est enregistré (transaction annulée).
    /// </summary>
    public async Task<Vente> CreerVenteAsync(int clientId, List<(int VinId, int Quantite)> lignes)
    {
        using var db = new CavisteDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();

        var vente = new Vente
        {
            ClientId = clientId,
            DateVente = DateTime.Now,
            Lignes = new List<LigneVente>()
        };

        foreach (var (vinId, quantite) in lignes)
        {
            var vin = await db.Vins.FindAsync(vinId);
            if (vin is null)
                throw new InvalidOperationException($"Vin introuvable (Id={vinId}).");
            if (vin.Stock < quantite)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour '{vin.Nom}' (disponible : {vin.Stock}, demandé : {quantite}).");

            vin.Stock -= quantite;
            vente.Lignes.Add(new LigneVente
            {
                VinId = vinId,
                Quantite = quantite,
                PrixUnitaire = vin.Prix
            });
        }

        db.Ventes.Add(vente);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await db.Ventes
            .Include(v => v.Client)
            .Include(v => v.Lignes).ThenInclude(l => l.Vin)
            .FirstAsync(v => v.Id == vente.Id);
    }
}
