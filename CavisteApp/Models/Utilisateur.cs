using System;
using System.ComponentModel.DataAnnotations;

namespace CavisteApp.Models;

public enum RoleUtilisateur
{
    Visiteur,
    Administrateur
}

/// <summary>
/// Compte utilisateur de l'application : identifiant = email, mot de passe
/// haché, rôle (Visiteur/Administrateur), et informations nécessaires à la
/// confirmation de compte et à la réinitialisation du mot de passe par email.
/// </summary>
public class Utilisateur
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasseHash { get; set; } = string.Empty;

    public RoleUtilisateur Role { get; set; } = RoleUtilisateur.Visiteur;

    public bool EstConfirme { get; set; }

    public string? CodeConfirmation { get; set; }
    public string? CodeReinitialisation { get; set; }
    public DateTime? DateExpirationCode { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.Now;
}
