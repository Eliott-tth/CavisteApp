using CommunityToolkit.Mvvm.ComponentModel;

namespace CavisteApp.Services;

/// <summary>
/// Contexte de session partagé par toute l'application : porte le rôle de
/// l'utilisateur courant (Visiteur / Administrateur). Les vues se lient à
/// <see cref="Instance"/> pour activer/désactiver leurs actions de
/// modification (contrôle d'accès par rôle exigé par le cahier des charges).
/// </summary>
public partial class SessionContext : ObservableObject
{
    public static SessionContext Instance { get; } = new();

    [ObservableProperty]
    private bool estAdministrateur;

    public string RoleAffiche => EstAdministrateur ? "Administrateur" : "Visiteur";

    partial void OnEstAdministrateurChanged(bool value) => OnPropertyChanged(nameof(RoleAffiche));

    private SessionContext() { }
}
