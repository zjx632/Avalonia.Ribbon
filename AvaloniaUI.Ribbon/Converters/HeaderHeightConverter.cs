using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaUI.Ribbon.Converters;

public class HeaderHeightConverter : IValueConverter
{
    public static readonly HeaderHeightConverter Instance = new();

    /// <summary>
    /// 转换布尔值为高度值
    /// ShowGroupBoxHeaders 为 true 时返回 98（带Header）
    /// ShowGroupBoxHeaders 为 false 时返回 86（无Header）
    /// </summary>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool showHeader)
        {
            return showHeader ? 98.0 : 86.0;
        }
        return 98.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
