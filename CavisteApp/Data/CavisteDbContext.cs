using System;
using System.IO;
using CavisteApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Data;

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

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Vin>()
            .HasOne(v => v.Fournisseur)
            .WithMany(f => f.Vins)
            .HasForeignKey(v => v.FournisseurId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Vente>()
            .HasOne(v => v.Client)
            .WithMany(c => c.Ventes)
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<CommandeFournisseur>()
            .HasOne(c => c.Fournisseur)
            .WithMany(f => f.Commandes)
            .HasForeignKey(c => c.FournisseurId)
            .OnDelete(DeleteBehavior.Restrict);

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
