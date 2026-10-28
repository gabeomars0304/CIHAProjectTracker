using MudBlazor;

public class TagColors
{
    public static Color For(ItemTag tag)
    {
        if (!string.IsNullOrWhiteSpace(tag.ColorHex)) return Color.Default;

        return tag.Name.Trim().ToLowerInvariant() switch
        {
            "project" => Color.Tertiary,
            "action item" => Color.Warning,
            "project on hold" => Color.Info,
            "issues/blockers" => Color.Error,
            _ => Color.Default
        };
    }

    //Inline style for a custom-colored tag, or "" for default-colored tags.
    public static string Style(ItemTag tag)
    {
        var hex = tag.ColorHex;
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#') return "";

        return $"background-color:{hex};color:{Contrast(hex)};";
    }
    public static string FillStyle(ItemTag tag)
    {
        var custom = Style(tag);
        if (custom != "") return custom;

        var palette = tag.Name.Trim().ToLowerInvariant() switch
        {
            "project" => "tertiary",
            "action item" => "warning",
            "project on hold" => "info",
            "issues/blockers" => "error",
            _ => null
        };

        return palette is null
            ? ""
            : $"background-color:var(--mud-palette-{palette});color:var(--mud-palette-{palette}-text);";
    }

    //Black or white text, whichever reads better on the background
    private static string Contrast(string hex)
    {
        try
        {
            var r = Convert.ToInt32(hex.Substring(1, 2), 16);
            var g = Convert.ToInt32(hex.Substring(3, 2), 16);
            var b = Convert.ToInt32(hex.Substring(5, 2), 16);
            return (0.299 * r + 0.587 * g + 0.114 * b) > 150 ? "#000000" : "#ffffff";
        }
        catch (FormatException) { return "#ffffff"; }
    }
}