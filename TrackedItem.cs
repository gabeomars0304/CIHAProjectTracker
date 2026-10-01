public class TrackedItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; } = DateTime.Now;

    public DateTime? EndDate { get; set; } = DateTime.Now;

    public string Status { get; set; } = String.Empty;

    public bool IsCompleted { get; set; } = false;

    public List<ItemTask> Tasks { get; set; } = [];

    public List<string> Notes { get; set; } = [];

    public List<ItemTag> Tags { get; set; } = [];

    public int PercentageComplete { get; set; } = 0;
}