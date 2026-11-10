using System.Globalization;

namespace TP_Product.Helpers;

public static class Money
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static string Format(decimal value) => value.ToString("N3", Fr) + " DT";

    public static string FormatShort(decimal value) => value.ToString("N0", Fr) + " DT";

    public static string Stars(double rating)
    {
        var rounded = (int)Math.Round(rating);
        return new string('★', Math.Clamp(rounded, 0, 5)) + new string('☆', Math.Clamp(5 - rounded, 0, 5));
    }
}

public static class DisplayText
{
    public static string Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";

        var parts = name.Split(new[] { ' ', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var single = parts[0];
            return single.Length >= 2 ? single[..2].ToUpperInvariant() : single.ToUpperInvariant();
        }

        return (parts[0][0].ToString() + parts[^1][0]).ToUpperInvariant();
    }

    public static string Truncate(string? text, int length = 110)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.Length <= length ? text : text[..length].TrimEnd() + "…";
    }
}
