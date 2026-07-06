# Caviste — Logiciel de caisse (Projet Expérientiel Collaboratif PC2)

Application de bureau WPF/C# pour la gestion des ventes, du stock et des
fournisseurs d'un caviste.

## Stack technique

| Besoin | Choix | Justification |
|---|---|---|
| Application lourde | WPF (.NET 9) | Imposé par les consignes, MVVM avec CommunityToolkit.Mvvm |
| Base de données | SQLite + EF Core 9 | Léger, aucun serveur à installer, ORM complet |
| PDF (ticket de caisse) | QuestPDF | Bibliothèque tierce, licence communautaire gratuite |
| Email (alerte stock bas) | MailKit | Bibliothèque tierce, standard pour SMTP en .NET |
| API distante | sampleapis.com/wines | Gratuite, sans clé, catégories reds/whites/sparkling/rosé (voir veille dans `WineApiService.cs`) |

## Prérequis

- Visual Studio 2022 (charge de travail ".NET desktop development")
- .NET 9 SDK
- Outils EF Core : `dotnet tool install --global dotnet-ef`

## Mise en route

```bash
cd CavisteApp
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

La base `caviste.db` est créée automatiquement au premier lancement
(`App.xaml.cs` appelle `Database.Migrate()`).

## Architecture du projet

```
CavisteApp/
├── Models/          # Entités métier (Vin, Client, Fournisseur, Vente, CommandeFournisseur...)
├── Data/            # DbContext EF Core + factory de design-time
├── Services/        # Logique métier (API, PDF, email, stock) — indépendante de l'UI
├── ViewModels/       # MVVM : logique de présentation (CommunityToolkit.Mvvm)
├── Views/           # XAML (fenêtres et contrôles utilisateur)
└── Converters/       # Convertisseurs XAML (ex : booléen -> couleur du voyant d'alerte)
```

## Méthode de gestion de projet

Méthode **Kanban** sur 3 jours, alignée sur le déroulement imposé par le
cahier des charges :

| Jour | Colonne Kanban | Livrable |
|---|---|---|
| J1 | Base de données | Modèle EF Core, migrations, import API |
| J2 | Stock & Client | CRUD Vin/Client, IHM associées |
| J3 | Fournisseur & Alerte | CRUD Fournisseur, commandes, alertes mail/visuelles |

## Configuration SMTP (alertes email, J3)

Le fichier `appsettings.json` contenant les vrais identifiants SMTP n'est **pas versionné** (voir `.gitignore`). Avant de tester l'envoi d'alertes :

1. Copie `CavisteApp/appsettings.example.json` en `CavisteApp/appsettings.json`
2. Renseigne tes identifiants (ex : compte Gmail avec un "mot de passe d'application", pas ton mot de passe principal)
3. Relance l'application (le fichier est copié à côté de l'exécutable au build)

Tant que ce fichier n'existe pas, le bouton "Envoyer alerte mail" affiche un message d'erreur explicite plutôt que de planter l'application — le voyant visuel clignotant et la création de commande fonctionnent indépendamment de la configuration email.

Outil collaboratif : **GitHub Projects** (tableau Kanban) + **GitHub** pour le
code (dépôt privé, commits réguliers et tracés — voir consigne `[GIT]`).

## Suivi par rapport à la grille de notation

- OOP + IHM avec interactions : classes métier + WPF/MVVM
- CRUD via requêtes primitives (ADO.NET) **et** via ORM (EF Core) sur des
  entités dépendantes : voir `Services/` (deux chemins d'accès aux données
  documentés séparément, cf. J2).
- Bibliothèque tierce (QuestPDF) + service distant (API vins + SMTP)
- Méthode de gestion de projet présentée avec Kanban
