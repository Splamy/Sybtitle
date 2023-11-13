namespace Sybtitle;

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

public static partial class HumanTimeSpan
{
    // lang = regex
    public const string RegexString = @"^(\+|-)?(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?(?:(\d+)ms)?$";
    [GeneratedRegex(RegexString, RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex TimeReg();

    public static TimeSpan? Parse(string? value)
    {
        if (value is null)
        {
            return null;
        }

        return ParseFromHuman(value)
            ?? ParseFromDefault(value)
            ?? ParseFromDigital(value)
            ?? ParseFromXml(value);
    }

    private static TimeSpan? ParseFromHuman(string value)
    {
        static int AsNum(string svalue)
        {
            if (string.IsNullOrEmpty(svalue))
            {
                return 0;
            }

            return int.TryParse(svalue, out var num) ? num : 0;
        }

        var match = TimeReg().Match(value);
        if (match.Success)
        {
            try
            {
                var sign = match.Groups[1].Value == "-" ? -1 : 1;

                return new TimeSpan(
                    AsNum(match.Groups[2].Value),
                    AsNum(match.Groups[3].Value),
                    AsNum(match.Groups[4].Value),
                    AsNum(match.Groups[5].Value),
                    AsNum(match.Groups[6].Value)) * sign;
            }
            catch { }
        }
        return null;
    }

    private static TimeSpan? ParseFromDigital(string value)
    {
        double sign;
        if (value.StartsWith('-'))
        {
            sign = -1;
            value = value[1..];
        }
        else if (value.StartsWith('+'))
        {
            sign = 1;
            value = value[1..];
        }
        else
        {
            sign = 1;
        }

        if (value.Contains(':'))
        {
            var splittime = value.Split(':');

            if (splittime.Length == 2
                && int.TryParse(splittime[0], out var minutes)
                && double.TryParse(splittime[1], NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds))
            {
                return (TimeSpan.FromSeconds(seconds) + TimeSpan.FromMinutes(minutes)) * sign;
            }
        }
        else
        {
            if (double.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds))
            {
                return TimeSpan.FromSeconds(seconds) * sign;
            }
        }
        return null;
    }

    private static TimeSpan? ParseFromXml(string value)
    {
        try { return XmlConvert.ToTimeSpan(value); }
        catch (FormatException) { return null; }
    }

    private static TimeSpan? ParseFromDefault(string value) => TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timeSpan) ? timeSpan : null;

    public static string FormatToHuman(this TimeSpan timeSpan)
    {
        var strb = new StringBuilder();

        if (timeSpan.Ticks < 0)
        {
            strb.Append('-');
        }

        var fullDays = (int)timeSpan.TotalDays;
        if (fullDays > 0)
        {
            strb.Append(fullDays).Append('d');
        }

        var h = timeSpan.Hours;
        if (h > 0)
        {
            strb.Append(h).Append('h');
        }

        var m = timeSpan.Minutes;
        if (m > 0)
        {
            strb.Append(m).Append('m');
        }

        var s = timeSpan.Seconds;
        if (s > 0)
        {
            strb.Append(s).Append('s');
        }

        var ms = timeSpan.Milliseconds;
        if (ms > 0)
        {
            strb.Append(ms).Append("ms");
        }

        return strb.ToString();
    }
}

public class HumanTimeSpanTypeConverter : TimeSpanConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str && HumanTimeSpan.Parse(str) is { } timeSpan)
        {
            return timeSpan;
        }
        else
        {
            return base.ConvertFrom(context, culture, value);
        }
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan.FormatToHuman();
        }
        else
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

public static class ModuleInit
{
    [ModuleInitializer]
    public static void ModuleInitializer()
    {
        TypeDescriptor.AddAttributes(typeof(TimeSpan), new TypeConverterAttribute(typeof(HumanTimeSpanTypeConverter)));
    }
}