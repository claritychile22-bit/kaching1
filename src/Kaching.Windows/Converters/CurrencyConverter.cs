using System.Globalization;
using System.Windows.Data;

namespace Kaching.Windows.Converters;

public sealed class CurrencyConverter : IValueConverter
{
    private static readonly CultureInfo Chile = new("es-CL");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            return amount.ToString("C0", Chile);
        }

        return "$0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
