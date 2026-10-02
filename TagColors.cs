using MudBlazor;
public class TagColors
{
    public static Color For(ItemTag tag) => tag.Name.Trim().ToLowerInvariant() switch
    {
        "project" => Color.Info,
        "action item" => Color.Warning,
        "project on hold" => Color.Error,
         _                  => Color.Default
    };
}