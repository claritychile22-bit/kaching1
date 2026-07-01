using System.Globalization;
using System.Windows.Data;

namespace Kaching.Windows.Converters;

public sealed class PercentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal percentage)
        {
            return percentage.ToString("P0", culture);
        }

        return "0 %";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
