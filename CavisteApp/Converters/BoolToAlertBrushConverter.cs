using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CavisteApp.Converters;

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
