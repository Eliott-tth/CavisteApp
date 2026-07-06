using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CavisteApp.Models;

/// <summary>
/// Représente une référence de vin gérée par le caviste : identité du produit,
/// prix de vente, quantité en stock et seuil déclenchant une alerte de
/// réapprovisionnement.
/// </summary>
public class Vin
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    public TypeVin Type { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Prix { get; set; }

    /// <summary>Quantité de bouteilles actuellement en stock.</summary>
    public int Stock { get; set; }

    /// <summary>Seuil en dessous duquel une alerte de stock bas est déclenchée (ex : 5).</summary>
    public int SeuilBas { get; set; } = 5;

    /// <summary>Fournisseur habituel de cette référence (optionnel).</summary>
    public int? FournisseurId { get; set; }
    public Fournisseur? Fournisseur { get; set; }

    /// <summary>Provenance du vin lorsqu'il a été importé depuis l'API distante (nullable pour les vins saisis manuellement).</summary>
    public string? Origine { get; set; }

    /// <summary>URL de la photo de la bouteille (fournie par l'API distante lors de l'import).</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Indique si le vin est actuellement sous son seuil de stock bas.</summary>
    [NotMapped]
    public bool EstEnAlerte => Stock <= SeuilBas;

    public ICollection<LigneVente> LignesVente { get; set; } = new List<LigneVente>();
    public ICollection<LigneCommandeFournisseur> LignesCommande { get; set; } = new List<LigneCommandeFournisseur>();
}
