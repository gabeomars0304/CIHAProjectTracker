using MudBlazor;

public class GroupColors
{
    public static Color For(ItemGroup group)
    {
        return Color.Default;
    }

    //Inline style for a custom-colored group, or "" for default-colored tags.
    public static string Style(ItemGroup group)
    {
        var hex = group.ColorHex;
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#') return "";

        return $"background-color:{hex};color:{Contrast(hex)};";
    }
    public static string FillStyle(ItemGroup group)
    {
        var custom = Style(group);
        if (custom != "") return custom;

        var palette = group.Name.Trim().ToLowerInvariant() switch
        {
            "project" => "tertiary",
            "action item" => "warning",
            "project on hold" => "info",
            "issues/blockers" => "error",
            _ => null
        };

        return palette is null
            ? ""
            : $"background-color:var(--mud-palette-{palette});color:var(--mud-palette-{palette}-text);";        //Just so default reads better dependent on background
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