using MudBlazor;
public class TagColors
{
    public static Color For(ItemTag tag) => tag.Name.Trim().ToLowerInvariant() switch
    {
        "project" => Color.Tertiary,
        "action item" => Color.Warning,
        "project on hold" => Color.Info,
        "issues/blockers" => Color.Error,
         _                  => Color.Default
    };
}