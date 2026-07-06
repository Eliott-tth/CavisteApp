using System;
using System.Security.Cryptography;

namespace CavisteApp.Services;

/// <summary>
/// Hache et vérifie les mots de passe avec PBKDF2 (sel aléatoire par
/// utilisateur, 100 000 itérations SHA-256) : jamais de mot de passe en clair
/// en base de données.
/// </summary>
public static class PasswordHasher
{
    private const int TailleSel = 16;
    private const int TailleHash = 32;
    private const int Iterations = 100_000;

    public static string Hacher(string motDePasse)
    {
        var sel = RandomNumberGenerator.GetBytes(TailleSel);
        var hash = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, Iterations, HashAlgorithmName.SHA256, TailleHash);
        return $"{Convert.ToBase64String(sel)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verifier(string motDePasse, string hashStocke)
    {
        var parties = hashStocke.Split('.');
        if (parties.Length != 2) return false;

        var sel = Convert.FromBase64String(parties[0]);
        var hashAttendu = Convert.FromBase64String(parties[1]);
        var hashCalcule = Rfc2898DeriveBytes.Pbkdf2(motDePasse, sel, Iterations, HashAlgorithmName.SHA256, TailleHash);

        return CryptographicOperations.FixedTimeEquals(hashCalcule, hashAttendu);
    }
}
