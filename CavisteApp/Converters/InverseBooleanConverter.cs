using System.Globalization;
using System.Windows.Data;

namespace CavisteApp.Converters;

/// <summary>Inverse un booléen. Utilisé par exemple pour désactiver un bouton pendant un import en cours.</summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !(value is bool b && b);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !(value is bool b && b);
}