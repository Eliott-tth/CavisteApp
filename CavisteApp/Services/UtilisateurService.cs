using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Services;

public class ResultatConnexion
{
    public bool Succes { get; set; }
    public string Message { get; set; } = string.Empty;
    public Utilisateur? Utilisateur { get; set; }
}

public class UtilisateurService
{
    private readonly EmailService _emailService = new();
    private readonly Random _random = new();

    public async Task<List<Utilisateur>> ListerAsync()
    {
        using var db = new CavisteDbContext();
        return await db.Utilisateurs.OrderBy(u => u.Email).ToListAsync();
    }

    public async Task<ResultatConnexion> InscrireAsync(string email, string motDePasse)
    {
        using var db = new CavisteDbContext();

        if (await db.Utilisateurs.AnyAsync(u => u.Email == email))
            return new ResultatConnexion { Succes = false, Message = "Un compte existe déjà avec cet email." };

        var code = GenererCode();
        var utilisateur = new Utilisateur
        {
            Email = email,
            MotDePasseHash = PasswordHasher.Hacher(motDePasse),
            Role = RoleUtilisateur.Visiteur,
            EstConfirme = false,
            CodeConfirmation = code,
            DateExpirationCode = DateTime.Now.AddMinutes(30)
        };

        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        try
        {
            await _emailService.EnvoyerCodeConfirmationAsync(email, code);
        }
        catch (Exception ex)
        {
            return new ResultatConnexion
            {
                Succes = true,
                Message = $"Compte créé mais l'email n'a pas pu être envoyé ({ex.Message}). Code de confirmation : {code}"
            };
        }

        return new ResultatConnexion { Succes = true, Message = "Compte créé. Vérifie ta boîte mail pour le code de confirmation." };
    }

    public async Task<ResultatConnexion> ConfirmerCompteAsync(string email, string code)
    {
        using var db = new CavisteDbContext();
        var utilisateur = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);

        if (utilisateur is null)
            return new ResultatConnexion { Succes = false, Message = "Compte introuvable." };
        if (utilisateur.CodeConfirmation != code || utilisateur.DateExpirationCode < DateTime.Now)
            return new ResultatConnexion { Succes = false, Message = "Code invalide ou expiré." };

        utilisateur.EstConfirme = true;
        utilisateur.CodeConfirmation = null;
        utilisateur.DateExpirationCode = null;
        await db.SaveChangesAsync();

        return new ResultatConnexion { Succes = true, Message = "Compte confirmé, tu peux te connecter.", Utilisateur = utilisateur };
    }

    public async Task<ResultatConnexion> ConnecterAsync(string email, string motDePasse)
    {
        using var db = new CavisteDbContext();
        var utilisateur = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);

        if (utilisateur is null || !PasswordHasher.Verifier(motDePasse, utilisateur.MotDePasseHash))
            return new ResultatConnexion { Succes = false, Message = "Email ou mot de passe incorrect." };

        if (!utilisateur.EstConfirme)
            return new ResultatConnexion { Succes = false, Message = "Compte non confirmé. Vérifie ta boîte mail." };

        return new ResultatConnexion { Succes = true, Message = "Connexion réussie.", Utilisateur = utilisateur };
    }

    public async Task<ResultatConnexion> DemanderReinitialisationAsync(string email)
    {
        using var db = new CavisteDbContext();
        var utilisateur = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);

        if (utilisateur is null)
            return new ResultatConnexion { Succes = false, Message = "Aucun compte associé à cet email." };

        var code = GenererCode();
        utilisateur.CodeReinitialisation = code;
        utilisateur.DateExpirationCode = DateTime.Now.AddMinutes(30);
        await db.SaveChangesAsync();

        try
        {
            await _emailService.EnvoyerCodeReinitialisationAsync(email, code);
        }
        catch (Exception ex)
        {
            return new ResultatConnexion { Succes = true, Message = $"Email non envoyé ({ex.Message}). Code : {code}" };
        }

        return new ResultatConnexion { Succes = true, Message = "Code de réinitialisation envoyé par email." };
    }

    public async Task<ResultatConnexion> ReinitialiserMotDePasseAsync(string email, string code, string nouveauMotDePasse)
    {
        using var db = new CavisteDbContext();
        var utilisateur = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email);

        if (utilisateur is null || utilisateur.CodeReinitialisation != code || utilisateur.DateExpirationCode < DateTime.Now)
            return new ResultatConnexion { Succes = false, Message = "Code invalide ou expiré." };

        utilisateur.MotDePasseHash = PasswordHasher.Hacher(nouveauMotDePasse);
        utilisateur.CodeReinitialisation = null;
        utilisateur.DateExpirationCode = null;
        await db.SaveChangesAsync();

        return new ResultatConnexion { Succes = true, Message = "Mot de passe réinitialisé." };
    }

    public async Task ModifierRoleAsync(int utilisateurId, RoleUtilisateur nouveauRole)
    {
        using var db = new CavisteDbContext();
        var utilisateur = await db.Utilisateurs.FindAsync(utilisateurId);
        if (utilisateur is not null)
        {
            utilisateur.Role = nouveauRole;
            await db.SaveChangesAsync();
        }
    }

    public async Task SupprimerAsync(int utilisateurId)
    {
        using var db = new CavisteDbContext();
        var utilisateur = await db.Utilisateurs.FindAsync(utilisateurId);
        if (utilisateur is not null)
        {
            db.Utilisateurs.Remove(utilisateur);
            await db.SaveChangesAsync();
        }
    }

    private string GenererCode() => _random.Next(100000, 999999).ToString();

    public async Task AssurerAdminParDefautAsync()
    {
        using var db = new CavisteDbContext();

        if (await db.Utilisateurs.AnyAsync(u => u.Role == RoleUtilisateur.Administrateur))
            return;

        db.Utilisateurs.Add(new Utilisateur
        {
            Email = "admin@caviste.fr",
            MotDePasseHash = PasswordHasher.Hacher("Admin123!"),
            Role = RoleUtilisateur.Administrateur,
            EstConfirme = true
        });

        await db.SaveChangesAsync();
    }
}
