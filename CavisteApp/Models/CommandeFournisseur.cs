using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Models;

public enum StatutCommande
{
    EnAttente,
    Validee,
    Receptionnee
}

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
