using CavisteApp.Data;
using CavisteApp.Models;
using Microsoft.Data.Sqlite;

namespace CavisteApp.Services;

public class ClientService
{
    public async Task<List<Client>> ListerAsync()
    {
        var resultat = new List<Client>();
        using var connexion = new SqliteConnection(DbConnectionHelper.ConnectionString);
        await connexion.OpenAsync();

        const string sql = "SELECT Id, Nom, Prenom, Email, AdressePostale FROM Clients ORDER BY Nom;";
        using var commande = new SqliteCommand(sql, connexion);
        using var lecteur = await commande.ExecuteReaderAsync();
        while (await lecteur.ReadAsync())
        {
            resultat.Add(new Client
            {
                Id = lecteur.GetInt32(0),
                Nom = lecteur.GetString(1),
                Prenom = lecteur.GetString(2),
                Email = lecteur.GetString(3),
                AdressePostale = lecteur.GetString(4)
            });
        }
        return resultat;
    }

    public async Task<int> AjouterAsync(Client client)
    {
        using var connexion = new SqliteConnection(DbConnectionHelper.ConnectionString);
        await connexion.OpenAsync();

        const string sql = """
            INSERT INTO Clients (Nom, Prenom, Email, AdressePostale)
            VALUES ($nom, $prenom, $email, $adresse);
            SELECT last_insert_rowid();
            """;
        using var commande = new SqliteCommand(sql, connexion);
        commande.Parameters.AddWithValue("$nom", client.Nom);
        commande.Parameters.AddWithValue("$prenom", client.Prenom);
        commande.Parameters.AddWithValue("$email", client.Email);
        commande.Parameters.AddWithValue("$adresse", client.AdressePostale);

        var nouvelId = (long)(await commande.ExecuteScalarAsync() ?? 0L);
        client.Id = (int)nouvelId;
        return client.Id;
    }

    public async Task ModifierAsync(Client client)
    {
        using var connexion = new SqliteConnection(DbConnectionHelper.ConnectionString);
        await connexion.OpenAsync();

        const string sql = """
            UPDATE Clients
            SET Nom = $nom, Prenom = $prenom, Email = $email, AdressePostale = $adresse
            WHERE Id = $id;
            """;
        using var commande = new SqliteCommand(sql, connexion);
        commande.Parameters.AddWithValue("$nom", client.Nom);
        commande.Parameters.AddWithValue("$prenom", client.Prenom);
        commande.Parameters.AddWithValue("$email", client.Email);
        commande.Parameters.AddWithValue("$adresse", client.AdressePostale);
        commande.Parameters.AddWithValue("$id", client.Id);

        await commande.ExecuteNonQueryAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        using var connexion = new SqliteConnection(DbConnectionHelper.ConnectionString);
        await connexion.OpenAsync();

        const string sql = "DELETE FROM Clients WHERE Id = $id;";
        using var commande = new SqliteCommand(sql, connexion);
        commande.Parameters.AddWithValue("$id", id);
        await commande.ExecuteNonQueryAsync();
    }
}
