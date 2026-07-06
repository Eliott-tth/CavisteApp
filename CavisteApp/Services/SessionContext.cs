using CavisteApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CavisteApp.Services;

public partial class SessionContext : ObservableObject
{
    public static SessionContext Instance { get; } = new();

    [ObservableProperty]
    private Utilisateur? utilisateurConnecte;

    public bool EstAdministrateur => UtilisateurConnecte?.Role == RoleUtilisateur.Administrateur;

    public string RoleAffiche => UtilisateurConnecte is null
        ? "Non connecté"
        : (EstAdministrateur ? "Administrateur" : "Visiteur");

    partial void OnUtilisateurConnecteChanged(Utilisateur? value)
    {
        OnPropertyChanged(nameof(EstAdministrateur));
        OnPropertyChanged(nameof(RoleAffiche));
    }

    private SessionContext() { }
}
