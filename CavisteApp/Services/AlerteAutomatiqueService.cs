using System.Threading.Tasks;
using CavisteApp.Models;

namespace CavisteApp.Services;

/// <summary>
/// Vérifie automatiquement, après toute opération qui modifie le stock d'un
/// vin (vente, ajustement manuel), s'il est passé sous son seuil, et envoie
/// l'email d'alerte sans aucune action de l'utilisateur. Les échecs d'envoi
/// (SMTP non configuré, réseau indisponible) sont absorbés silencieusement :
/// ils ne doivent jamais faire échouer une vente ou une modification de stock.
/// </summary>
public class AlerteAutomatiqueService
{
    private readonly EmailService _emailService = new();

    public async Task VerifierApresModificationStockAsync(Vin vin)
    {
        if (!vin.EstEnAlerte) return;

        try
        {
            await _emailService.EnvoyerAlerteStockBasAsync(vin);
        }
        catch
        {
            // SMTP non configuré ou indisponible : ne jamais bloquer l'opération métier pour ça.
        }
    }
}
