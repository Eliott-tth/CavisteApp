using System;
using System.IO;

namespace CavisteApp.Data;

/// <summary>
/// Centralise le chemin du fichier SQLite pour que le contexte EF Core et les
/// accès en SQL brut (ADO.NET) pointent toujours vers la même base.
/// </summary>
public static class DbConnectionHelper
{
    public static readonly string DbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "caviste.db");

    public static string ConnectionString => $"Data Source={DbPath}";
}
