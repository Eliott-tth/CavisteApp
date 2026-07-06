namespace CavisteApp.Models;

/// <summary>
/// Type / couleur d'un vin. Correspond aux catégories exposées par l'API
/// distante (endpoints reds / whites / rosés / sparkling de sampleapis.com).
/// </summary>
public enum TypeVin
{
    Rouge,
    Blanc,
    Rose,
    Petillant
}