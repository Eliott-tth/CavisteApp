using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

public class VenteService
{
    private readonly AlerteAutomatiqueService _alerteService = new();

    public async Task<List<Vente>> ListerAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Ventes
            .Include(v => v.Client)
            .Include(v => v.Lignes).ThenInclude(l => l.Vin)
            .OrderByDescending(v => v.DateVente)
            .ToListAsync();
    }

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

        var vinsAffectes = new List<Vin>();

        foreach (var (vinId, quantite) in lignes)
        {
            var vin = await db.Vins.FindAsync(vinId);
            if (vin is null)
                throw new InvalidOperationException($"Vin introuvable (Id={vinId}).");
            if (vin.Stock < quantite)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour '{vin.Nom}' (disponible : {vin.Stock}, demandé : {quantite}).");

            vin.Stock -= quantite;
            vinsAffectes.Add(vin);
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

        foreach (var vin in vinsAffectes)
            await _alerteService.VerifierApresModificationStockAsync(vin);

        return await db.Ventes
            .Include(v => v.Client)
            .Include(v => v.Lignes).ThenInclude(l => l.Vin)
            .FirstAsync(v => v.Id == vente.Id);
    }
}
