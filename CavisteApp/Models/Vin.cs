using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CavisteApp.Models;

public class Vin
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    public TypeVin Type { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Prix { get; set; }

    public int Stock { get; set; }

    public int SeuilBas { get; set; } = 5;

    public int? FournisseurId { get; set; }
    public Fournisseur? Fournisseur { get; set; }

    public string? Origine { get; set; }

    public string? ImageUrl { get; set; }

    public bool EstSupprime { get; set; }

    [NotMapped]
    public bool EstEnAlerte => Stock <= SeuilBas;

    public ICollection<LigneVente> LignesVente { get; set; } = new List<LigneVente>();
    public ICollection<LigneCommandeFournisseur> LignesCommande { get; set; } = new List<LigneCommandeFournisseur>();
}
