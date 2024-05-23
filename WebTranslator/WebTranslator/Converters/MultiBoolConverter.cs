using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace WebTranslator.Converters;

public sealed class BoolAndMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return BoolHelper.PreConvert(values, targetType, parameter, culture).All(x => x);
    }
}

public sealed class BoolOrMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return BoolHelper.PreConvert(values, targetType, parameter, culture).Any(x => x);
    }
}

public sealed class BoolNotMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return BoolHelper.PreConvert(values, targetType, parameter, culture).All(x => !x);
    }
}

internal static class BoolHelper
{
    internal static IList<bool> PreConvert(IList<object?> values, Type targetType, object? parameter,
        CultureInfo culture)
    {
        if (!targetType.IsAssignableFrom(typeof(bool)))
            throw new NotSupportedException();

        if (values.Count == 0)
            return new List<bool>();

        for (var i = 0; i < values.Count; i++)
        {
            if (values[i] is not bool)
                throw new NotSupportedException();

            values[i] ??= false;
        }

        return values.Cast<bool>().ToList();
    }
}