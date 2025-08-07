using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SemanticCode.Converters;

public class MessageTypeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string messageType)
            return Brushes.Gray;

        return messageType switch
        {
            "user" => new SolidColorBrush(Color.Parse("#2563eb")), // Blue
            "assistant" => new SolidColorBrush(Color.Parse("#059669")), // Green  
            "system" => new SolidColorBrush(Color.Parse("#dc2626")), // Red
            _ => Brushes.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MessageTypeDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string messageType)
            return "";

        return messageType switch
        {
            "user" => "用户",
            "assistant" => "助手",
            "system" => "系统",
            _ => messageType
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}