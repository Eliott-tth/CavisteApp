using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CavisteApp.Converters;

/// <summary>
/// Convertit un booléen (ex : Vin.EstEnAlerte) en Brush : rouge si vrai, vert sinon.
/// Utilisé pour le voyant visuel de stock bas dans les grilles.
/// </summary>
public class BoolToAlertBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enAlerte = value is bool b && b;
        return enAlerte ? Brushes.Crimson : Brushes.MediumSeaGreen;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}