using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BEAM.Converter;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Enum.TryParse(targetType, value?.ToString(), out var result))
        {
            return result;
        }
        return null;
    }
}