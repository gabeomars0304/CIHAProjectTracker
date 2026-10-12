public class ItemTag
{
    public int Id { get; set; }
    public string Name {  get; set; } = string.Empty;
    public List<TrackedItem> Items { get; set; } = [];
}
