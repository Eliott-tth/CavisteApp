using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Models;

/// <summary>
/// Client du caviste, enregistré lors d'une vente pour l'édition du ticket de caisse.
/// </summary>
public class Client
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Prenom { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string AdressePostale { get; set; } = string.Empty;

    public ICollection<Vente> Ventes { get; set; } = new List<Vente>();

    public string NomComplet => $"{Prenom} {Nom}";
}