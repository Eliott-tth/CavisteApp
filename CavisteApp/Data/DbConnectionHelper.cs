using System;
using System.IO;

namespace CavisteApp.Data;

public static class DbConnectionHelper
{
    public static readonly string DbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "caviste.db");

    public static string ConnectionString => $"Data Source={DbPath}";
}
