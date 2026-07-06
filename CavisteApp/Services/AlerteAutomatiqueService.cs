using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CavisteApp.Models;

namespace CavisteApp.Services;

public class AlerteAutomatiqueService
{
    private readonly EmailService _emailService = new();
    private readonly CommandeFournisseurService _commandeService = new();

    public async Task VerifierApresModificationStockAsync(Vin vin)
    {
        if (!vin.EstEnAlerte) return;

        try
        {
            await _emailService.EnvoyerAlerteStockBasAsync(vin);
        }
        catch
        {
        }

        if (vin.FournisseurId is not null)
        {
            var dejaEnCours = await _commandeService.CommandeEnCoursExistePourVinAsync(vin.Id);
            if (!dejaEnCours)
            {
                var quantite = Math.Max(vin.SeuilBas * 3, 12);
                await _commandeService.CreerCommandeAsync(
                    vin.FournisseurId.Value,
                    new List<(int VinId, int Quantite)> { (vin.Id, quantite) });
            }
        }
    }
}
