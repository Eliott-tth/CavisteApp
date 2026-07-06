using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CavisteApp.Models;

public class Vente
{
    [Key]
    public int Id { get; set; }

    public DateTime DateVente { get; set; } = DateTime.Now;

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public ICollection<LigneVente> Lignes { get; set; } = new List<LigneVente>();

    [NotMapped]
    public decimal MontantTotal => Lignes.Sum(l => l.Quantite * l.PrixUnitaire);
}

public class LigneVente
{
    [Key]
    public int Id { get; set; }

    public int VenteId { get; set; }
    public Vente Vente { get; set; } = null!;

    public int VinId { get; set; }
    public Vin Vin { get; set; } = null!;

    public int Quantite { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrixUnitaire { get; set; }

    [NotMapped]
    public decimal SousTotal => Quantite * PrixUnitaire;
}
