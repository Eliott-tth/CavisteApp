using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Models;

public class Fournisseur
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Telephone { get; set; }

    [MaxLength(250)]
    public string? Adresse { get; set; }

    public ICollection<Vin> Vins { get; set; } = new List<Vin>();
    public ICollection<CommandeFournisseur> Commandes { get; set; } = new List<CommandeFournisseur>();
}
