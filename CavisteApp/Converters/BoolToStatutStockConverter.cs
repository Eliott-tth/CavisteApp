using System;
using System.Globalization;
using System.Windows.Data;

namespace CavisteApp.Converters;

public class BoolToStatutStockConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "Stock bas" : "En stock";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
