using System;
using System.IO;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Data;

/// <summary>
/// Contexte EF Core de l'application. Utilise SQLite en fichier local
/// (caviste.db, créé automatiquement à côté de l'exécutable).
/// </summary>
public class CavisteDbContext : DbContext
{
    public DbSet<Vin> Vins => Set<Vin>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<Vente> Ventes => Set<Vente>();
    public DbSet<LigneVente> LignesVente => Set<LigneVente>();
    public DbSet<CommandeFournisseur> CommandesFournisseur => Set<CommandeFournisseur>();
    public DbSet<LigneCommandeFournisseur> LignesCommandeFournisseur => Set<LigneCommandeFournisseur>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(DbConnectionHelper.ConnectionString);
        // Décommenter pendant le développement pour voir les requêtes SQL générées :
        // optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Vin / Fournisseur (1-N) ---
        modelBuilder.Entity<Vin>()
            .HasOne(v => v.Fournisseur)
            .WithMany(f => f.Vins)
            .HasForeignKey(v => v.FournisseurId)
            .OnDelete(DeleteBehavior.SetNull);

        // --- Vente / Client (N-1) ---
        modelBuilder.Entity<Vente>()
            .HasOne(v => v.Client)
            .WithMany(c => c.Ventes)
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- LigneVente : dépend de Vente ET de Vin ---
        modelBuilder.Entity<LigneVente>()
            .HasOne(l => l.Vente)
            .WithMany(v => v.Lignes)
            .HasForeignKey(l => l.VenteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LigneVente>()
            .HasOne(l => l.Vin)
            .WithMany(v => v.LignesVente)
            .HasForeignKey(l => l.VinId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- CommandeFournisseur / Fournisseur (N-1) ---
        modelBuilder.Entity<CommandeFournisseur>()
            .HasOne(c => c.Fournisseur)
            .WithMany(f => f.Commandes)
            .HasForeignKey(c => c.FournisseurId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- LigneCommandeFournisseur : dépend de CommandeFournisseur ET de Vin ---
        modelBuilder.Entity<LigneCommandeFournisseur>()
            .HasOne(l => l.CommandeFournisseur)
            .WithMany(c => c.Lignes)
            .HasForeignKey(l => l.CommandeFournisseurId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LigneCommandeFournisseur>()
            .HasOne(l => l.Vin)
            .WithMany(v => v.LignesCommande)
            .HasForeignKey(l => l.VinId)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
