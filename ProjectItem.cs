public class ProjectItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public string Status { get; set; } = String.Empty;

    public bool IsCompleted { get; set; } = false;

    public List<ProjectTask> Tasks { get; set; } = [];

    public int PercentageComplete { get; set; } = 0;
}
