using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Models;

public enum StatutCommande
{
    EnAttente,
    Validee,
    Receptionnee
}

/// <summary>
/// Commande passée auprès d'un fournisseur suite à une alerte de stock bas.
/// Le passage au statut <see cref="StatutCommande.Receptionnee"/> incrémente
/// le stock des vins concernés (cf. StockService).
/// </summary>
public class CommandeFournisseur
{
    [Key]
    public int Id { get; set; }

    public int FournisseurId { get; set; }
    public Fournisseur Fournisseur { get; set; } = null!;

    public DateTime DateCommande { get; set; } = DateTime.Now;
    public DateTime? DateReception { get; set; }

    public StatutCommande Statut { get; set; } = StatutCommande.EnAttente;

    public ICollection<LigneCommandeFournisseur> Lignes { get; set; } = new List<LigneCommandeFournisseur>();
}

/// <summary>
/// Ligne de détail d'une commande fournisseur : quantité commandée pour un vin
/// donné, et quantité effectivement reçue (peut différer en cas de livraison partielle).
/// </summary>
public class LigneCommandeFournisseur
{
    [Key]
    public int Id { get; set; }

    public int CommandeFournisseurId { get; set; }
    public CommandeFournisseur CommandeFournisseur { get; set; } = null!;

    public int VinId { get; set; }
    public Vin Vin { get; set; } = null!;

    public int QuantiteCommandee { get; set; }
    public int? QuantiteRecue { get; set; }
}
