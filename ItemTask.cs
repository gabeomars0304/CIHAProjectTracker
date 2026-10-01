public class ItemTask
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; } = DateTime.Now;

    public DateTime? EndDate { get; set; } = DateTime.Now;

    public string Status { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public List<ItemTask> SubTasks { get; set; } = [];

    public List<string> Notes { get; set; } = [];

    public int PercentageComplete { get; set; } = 0;
}
